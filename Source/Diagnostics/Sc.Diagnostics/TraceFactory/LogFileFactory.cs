using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using Microsoft.VisualBasic.Logging;
using Sc.Abstractions.Application;
using Sc.Abstractions.Lifecycle;
using Sc.Util.Threading;

namespace Sc.Diagnostics.TraceFactory
{
	/// <summary>
	/// Implements an <see cref="ITraceSourceSelector"/> that will add
	/// a rolling text writer listener that writes to a file. Note: to
	/// support subclassing, this class implements <see cref="IInitialize"/>:
	/// you must initialize the instance before first use.
	/// Notice also: this factory does NOT maintain any cache of configured
	/// <see cref="TraceSource"/> instances by itself: this will by default add
	/// itself to <see cref="TraceSources"/>, which will allow this
	/// to receive and configure each source created there; and ALSO allows this
	/// to remove itself when configurations here change --- <see cref="TraceSources"/>
	/// WILL THEN invoke this with all configured sources to remove these
	/// configurations; which CAN become disposed and re-configured here.
	/// If you invoke this with any other <see cref="TraceSource"/>,
	/// then this factory WILL NOT be able to remove configurations from the
	/// source when this instance changes: ONLY sources added via
	/// <see cref="TraceSources"/> will be able to be re-configured.
	/// This class also accepts
	/// <see cref="ILogFileFactorySelector"/> delegates to select the
	/// <see cref="TraceSource"/> instances that will receive configuration here.
	/// Note that multiple selectors can be added: the factory will
	/// configure all flags accumulated by all added selectors
	/// --- it is not possible for a selector here to return
	/// <see cref="LogFileFactorySelection.None"/> and override any
	/// other selector that returns some flag. If no selectors are added,
	/// or if all return <see cref="LogFileFactorySelection.Default"/>,
	/// you may set the <see cref="DefaultTraceSourceSelection"/>
	/// property to select their behavior --- which defaults to
	/// <see cref="LogFileFactorySelection.All"/>.
	/// For configured sources, the switch level is specified
	/// by <see cref="SelectedSwitchLevel"/>; AND that defaults
	/// to <see cref="SourceLevels.Warning"/>.
	/// The <see cref="LogFileFilterLevel"/> sets the file's filter, and defaults to
	/// <see cref="SourceLevels.Information"/>. The location and name of the file is
	/// defined by a given <see cref="IAppScope"/>.
	/// You can toggle the file explicitly with <see cref="ToggleLogFile"/>; AND
	/// this defaults to TRUE.
	/// This factory will also support a <see cref="LogFileFactoryConfig"/> file
	/// that is located in this specified <see cref="GetLogFolderPath"/>,
	/// and is named as specified by <see cref="GetConfigFileName"/>.
	/// </summary>
	public class LogFileFactory
			: ILogFileFactoryConfig,
					ITraceSourceSelector,
					IInitialize,
					IDisposable
	{
		private readonly List<ILogFileFactorySelector> selectors = new List<ILogFileFactorySelector>(1);
		private readonly bool addToTraceSources;
		private bool isInitialized;
		private bool isInitializing;

		private LogFileFactorySelection defaultTraceSourceSelection = LogFileFactorySelection.All;
		private SourceLevels selectedSwitchLevel = SourceLevels.Warning;
		private SourceLevels logFileFilterLevel = SourceLevels.Information;
		private bool watchConfigFileChanges = true;
		private bool toggleLogFile = true;

		private FileLogTraceListener logFileListener;
		private LogFileFactoryConfig logFileFactoryConfig;
		private FileSystemWatcher configFileWatcher;
		private int isConfigWatcherChangeRaised;


		/// <summary>
		/// Synchronizes all operations.
		/// </summary>
		protected readonly object SyncLock = new object();

		/// <summary>
		/// Not null. Provided to the constructor; and is used to find the name of
		/// the optional trace file.
		/// </summary>
		protected readonly IAppScope AppScope;

		/// <summary>
		/// Not null. Provided on construction, and used to trace output for
		/// operations in this class --- so that this trace output here goes through
		/// a trace source defined for this selected assembly.
		/// </summary>
		protected readonly Assembly TraceFileFactoryAssembly;

		/// <summary>
		/// Is the root folder location where the <see cref="LogFileListener"/>
		/// file is located: the full folder path is returned
		/// from <see cref="GetLogFolderPath"/>.
		/// </summary>
		protected readonly Environment.SpecialFolder? LogFileRootLocation;

		/// <summary>
		/// Optional: specifies a further subfolder that is created within the
		/// <see cref="IAppScope.GetAppDataFolderPath"/> for the log file.
		/// </summary>
		protected readonly string LogFileInnerSubFolder;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="appScope">Required: the <see cref="IAppScope.GetAppDataFolderPath"/> and
		/// <see cref="IAppScope.AppGuid"/> determines the location and name of the trace log file.</param>
		/// <param name="traceFileFactoryAssembly">Required: all trace output traced by this class
		/// is traced with a trace source fetched for this assembly. The assembly is not
		/// otherwise used here.</param>
		/// <param name="logFileRootLocation">Selects the root location for the trace output
		/// file. Defaults to <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
		/// Notice that this argument is nullable: if this is null, then the root folder is
		/// <see cref="Path.GetTempPath"/>. Then, in either case, the file is then within the
		/// <see cref="IAppScope.GetAppDataFolderPath"/> within this root folder (and
		/// then any optional further subfolder specified by
		/// <paramref name="logFileInnerSubFolder"/>.</param>
		/// <param name="logFileInnerSubFolder">This can specify an optional further subfolder
		/// that is created within the <see cref="IAppScope.GetAppDataFolderPath"/>.</param>
		/// <param name="addToTraceSources">Defaults to true: this factory adds itself to
		/// <see cref="TraceSources"/> to configure all sources created there.</param>
		public LogFileFactory(
				IAppScope appScope,
				Assembly traceFileFactoryAssembly,
				Environment.SpecialFolder? logFileRootLocation
						= Environment.SpecialFolder.LocalApplicationData,
				string logFileInnerSubFolder = null,
				bool addToTraceSources = true)
		{
			AppScope = appScope ?? throw new ArgumentNullException(nameof(appScope));
			TraceFileFactoryAssembly
					= traceFileFactoryAssembly
					?? throw new ArgumentNullException(nameof(traceFileFactoryAssembly));
			LogFileRootLocation = logFileRootLocation;
			LogFileInnerSubFolder
					= string.IsNullOrWhiteSpace(logFileInnerSubFolder)
							? null
							: logFileInnerSubFolder;
			this.addToTraceSources = addToTraceSources;
			new LogFileFactoryConfig().SetPropertiesOn(this);
			trySetPropertiesFromConfigFile();
		}


		private void traceAction(string message)
		{
			TraceSources.For(TraceFileFactoryAssembly)
					.Warning($"##########    ####    {message}    ####    ##########");
			Trace.TraceWarning(
					"##########    ####    "
					+ $"{message} For: {TraceFileFactoryAssembly.GetName().Name}."
					+ "    ####    ##########");
			FlushAll();
		}

		private void setupTracingUnsafe()
		{
			bool resetIsInitializing = false;
			try {
				lock (SyncLock) {
					if (!isInitialized
							|| isInitializing)
						return;
					isInitializing = true;
					resetIsInitializing = true;
				}
				TraceSources.For(TraceFileFactoryAssembly)
						.Info("Configure tracing.");
				HandleBeginSetupTracing();
				disposeResources();
				if (ToggleLogFile) {
					lock (SyncLock) {
						logFileListener = CreateFileLogTraceListener();
					}
				}
				setupConfigFileWatcher();
				if (addToTraceSources)
					TraceSources.AddSelector(this);
				traceAction(
						LogFileListener != null
								? $"{nameof(LogFileFactory)} Trace File Initialized."
								: $"{nameof(LogFileFactory)} Reset With No Trace File.");
			} finally {
				if (resetIsInitializing) {
					lock (SyncLock) {
						isInitializing = false;
					}
				}
			}
			HandleEndSetupTracing();
		}

		private void trySetPropertiesFromConfigFile()
		{
			string configFilePath = GetConfigFileFullPath();
			if (File.Exists(configFilePath)) {
				try {
					LogFileFactoryConfig newLogFileFactoryConfig
							= LogFileFactoryConfig.LoadFromFile(configFilePath);
					bool resetIsInitializing = false;
					try {
						lock (SyncLock) {
							resetIsInitializing = !isInitializing;
							isInitializing = true;
						}
						newLogFileFactoryConfig.SetPropertiesOn(this);
					} finally {
						lock (SyncLock) {
							logFileFactoryConfig = newLogFileFactoryConfig;
							if (resetIsInitializing)
								isInitializing = false;
						}
					}
					TraceSources.For(TraceFileFactoryAssembly)
							.Info(
									"Found {0} file found at '{1}' - '{2}'.",
									nameof(LogFileFactoryConfig),
									configFilePath,
									logFileFactoryConfig);
				} catch (Exception exception) {
					TraceSources.For(TraceFileFactoryAssembly)
							.Error(
									exception,
									"Invalid {0} file found at '{1}' - '{2}'.",
									nameof(LogFileFactoryConfig),
									configFilePath,
									exception.Message);
				}
			} else {
				try {
					Directory.CreateDirectory(GetLogFolderPath());
					LogFileFactoryConfig.CreateFrom(this, out XmlDocument xmlDocument);
					File.WriteAllText(configFilePath, xmlDocument.InnerXml);
				} catch (Exception exception) {
					TraceSources.For(TraceFileFactoryAssembly)
							.Error(
									exception,
									"Error writing {0} file to '{1}' - '{2}'.",
									nameof(LogFileFactoryConfig),
									configFilePath,
									exception.Message);
				}
			}
		}

		private void disposeResources()
		{
			disposeConfigFileWatcher();
			traceAction($"{nameof(LogFileFactory)} Is Being Reset ...");
			TraceSources.RemoveSelector(this);
			lock (SyncLock) {
				logFileListener?.Dispose();
				logFileListener = null;
			}
		}

		private void setupConfigFileWatcher()
		{
			lock (SyncLock) {
				if (!isInitialized)
					return;
				disposeConfigFileWatcher();
				if (!WatchConfigFileChanges)
					return;
				try {
					Directory.CreateDirectory(GetLogFolderPath());
					configFileWatcher
							= new FileSystemWatcher(
									GetLogFolderPath(),
									GetConfigFileName())
							{
								IncludeSubdirectories = false,
								NotifyFilter = NotifyFilters.CreationTime
										| NotifyFilters.FileName
										| NotifyFilters.LastWrite,
								EnableRaisingEvents = true,
							};
					configFileWatcher.Changed += handleConfigWatcherChangeEvent;
					configFileWatcher.Created += handleConfigWatcherChangeEvent;
					configFileWatcher.Deleted += handleConfigWatcherChangeEvent;
					configFileWatcher.Error += handleConfigWatcherError;
				} catch (Exception exception) {
					disposeConfigFileWatcher();
					TraceSources.For(TraceFileFactoryAssembly)
							.Error(
									exception,
									"Exception trying to watch for {0} config file changes at '{1}' - '{2}'.",
									nameof(LogFileFactoryConfig),
									GetConfigFileFullPath(),
									exception.Message);
				}
			}
		}

		private void disposeConfigFileWatcher()
		{
			lock (SyncLock) {
				if (configFileWatcher == null)
					return;
				configFileWatcher.Changed -= handleConfigWatcherChangeEvent;
				configFileWatcher.Created -= handleConfigWatcherChangeEvent;
				configFileWatcher.Deleted -= handleConfigWatcherChangeEvent;
				configFileWatcher.Error -= handleConfigWatcherError;
				configFileWatcher.Dispose();
				configFileWatcher = null;
			}
		}

		private void handleConfigWatcherChangeEvent(object sender, EventArgs e)
		{
			int thisId;
			lock (SyncLock) {
				thisId = ++isConfigWatcherChangeRaised;
				(SynchronizationContext.Current ?? new SynchronizationContext()).Post(HandleEvent, thisId);
			}
			void HandleEvent(int id)
			{
				lock (SyncLock) {
					if (isConfigWatcherChangeRaised != id)
						return;
				}
				trySetPropertiesFromConfigFile();
				setupTracingUnsafe();
			}
		}

		private void handleConfigWatcherError(object sender, ErrorEventArgs e)
			=> TraceSources.For<LogFileFactory>()
					.Error(
							e.GetException(),
							"{0} error for config file watcher at '{1}' - '{2}'.",
							nameof(FileSystemWatcher),
							Path.Combine(GetLogFolderPath(), GetConfigFileName()),
							e.GetException()
									.Message);

		private void setLogFileListenerFilter(FileLogTraceListener listener)
		{
#if DEBUG
			listener.Filter = new EventTypeFilter(LogFileFilterLevel.MostVerbose(SourceLevels.Information));
#else
			listener.Filter = new EventTypeFilter(LogFileFilterLevel);
#endif
		}


		/// <summary>
		/// This method must construct the <see cref="LogFileListener"/>, using the
		/// <see cref="GetLogFolderPath"/>, <see cref="GetLogFileName"/>, and
		/// <see cref="LogFileFilterLevel"/>. This implementation then sets:
		/// <see cref="FileLogTraceListener.Append"/> false,
		/// <see cref="LogFileCreationScheduleOption.Daily"/>, and
		/// <see cref="TraceOptions.DateTime"/>.
		/// </summary>
		/// <returns>Not null.</returns>
		protected virtual FileLogTraceListener CreateFileLogTraceListener()
		{
			FileLogTraceListener result
					= new FileLogTraceListener(GetLogFileName())
					{
						Location = LogFileLocation.Custom,
						BaseFileName = GetLogFileName(),
						CustomLocation = GetLogFolderPath(),
						Append = false,
						LogFileCreationSchedule = LogFileCreationScheduleOption.Daily,
						TraceOutputOptions = TraceOptions.DateTime,
					};
			setLogFileListenerFilter(result);
			return result;
		}


		/// <summary>
		/// This virtual method will be invoked when this class begins to reset and
		/// reconfigure tracing. This is invoked when Initialized, when a watched config
		/// file changes, and when configuration properties are changed here.
		/// This is invoked BEFORE this class begins to dispose and reset its
		/// own resources. Configuration will then proceed, and will be followed
		/// by <see cref="HandleEndSetupTracing"/>.
		/// </summary>
		protected virtual void HandleBeginSetupTracing() { }

		/// <summary>
		/// This virtual method will be invoked when this class completes resetting and
		/// reconfigure tracing. Please see <see cref="HandleBeginSetupTracing"/>.
		/// </summary>
		protected virtual void HandleEndSetupTracing() { }


		/// <summary>
		/// This method implements the configuration for each <see cref="TraceSource"/>:
		/// this implements <see cref="ITraceSourceSelector"/>.
		/// This is implemented by invoking all added selector delegates;
		/// and configuring the argument based on the selection result. NOTICE that ANY
		/// <see cref="TraceSource"/> passed here will be configured; AND this does NOT
		/// maintain its own cache to the configured sources: if the configurations
		/// here change, ONLY sources added via <see cref="TraceSources"/> will be
		/// able to be re-configured.
		/// </summary>
		void ITraceSourceSelector.Select(SimpleTraceSource traceSource)
		{
			LogFileFactorySelection SelectAll()
			{
				LogFileFactorySelection selection = LogFileFactorySelection.Default;
				foreach (ILogFileFactorySelector selector in selectors) {
					selection |= selector.Select(
							traceSource.TraceSource,
							defaultTraceSourceSelection,
							selectedSwitchLevel);
				}
				return selection;
			}
			lock (SyncLock) {
				if (!isInitialized)
					return;
				traceSource.TraceSource.Listeners.Remove(GetLogFileName());
				LogFileFactorySelection result = SelectAll();
				switch (result) {
					case LogFileFactorySelection.None :
						return;
					case LogFileFactorySelection.Default :
						result = defaultTraceSourceSelection;
						break;
				}
				if (result.HasFlag(LogFileFactorySelection.SwitchAndFilterLevels))
					SetSwitchLevel(traceSource);
				if (result.HasFlag(LogFileFactorySelection.LogFileOutput)
						&& (logFileListener != null))
					traceSource.TraceSource.Listeners.Add(logFileListener);
			}
			void SetSwitchLevel(SimpleTraceSource simpleTraceSource)
			{
#if DEBUG
				simpleTraceSource.TraceSource.Switch.Level
						= simpleTraceSource.TraceSource.Switch.Level.MostVerbose(
								SourceLevels.Information.MostVerbose(selectedSwitchLevel));
#else
				simpleTraceSource.TraceSource.Switch.Level
						= simpleTraceSource.TraceSource.Switch.Level.MostVerbose(selectedSwitchLevel);
#endif
			}
		}

		/// <summary>
		/// This method implements <see cref="ITraceSourceSelector"/>.
		/// </summary>
		void ITraceSourceSelector.Remove(SimpleTraceSource traceSource)
			=> traceSource.TraceSource.Listeners.Remove(GetLogFileName());


		/// <summary>
		/// This method adds an <see cref="ILogFileFactorySelector"/> here to
		/// be invoked with each newly-created <see cref="TraceSource"/>.
		/// </summary>
		/// <param name="selector">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddSelector(ILogFileFactorySelector selector)
		{
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));
			lock (SyncLock) {
				if (selectors.Contains(selector))
					return;
				selectors.Add(selector);
				if (!isInitialized
						|| !addToTraceSources
						|| isInitializing)
					return;
			}
			TraceSources.RemoveSelector(this);
			TraceSources.AddSelector(this);
		}

		/// <summary>
		/// Removes a selector added in <see cref="AddSelector"/>.
		/// </summary>
		/// <param name="selector">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void RemoveSelector(ILogFileFactorySelector selector)
		{
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));
			lock (SyncLock) {
				if (!selectors.Remove(selector))
					return;
				if (!isInitialized
						|| !addToTraceSources
						|| isInitializing)
					return;
			}
			TraceSources.RemoveSelector(this);
			TraceSources.AddSelector(this);
		}


		public void Initialize()
		{
			lock (SyncLock) {
				if (isInitialized)
					return;
				isInitialized = true;
			}
			setupTracingUnsafe();
		}


		public LogFileFactorySelection DefaultTraceSourceSelection
		{
			get {
				lock (SyncLock) {
					return defaultTraceSourceSelection;
				}
			}
			set {
				lock (SyncLock) {
					if (defaultTraceSourceSelection == value)
						return;
					defaultTraceSourceSelection = value;
				}
				setupTracingUnsafe();
			}
		}
		
		public SourceLevels SelectedSwitchLevel
		{
			get {
				lock (SyncLock) {
					return selectedSwitchLevel;
				}
			}
			set {
				lock (SyncLock) {
					if (selectedSwitchLevel == value)
						return;
					selectedSwitchLevel = value;
				}
				setupTracingUnsafe();
			}
		}

		public SourceLevels LogFileFilterLevel
		{
			get {
				lock (SyncLock) {
					return logFileFilterLevel;
				}
			}
			set {
				lock (SyncLock) {
					if (logFileFilterLevel == value)
						return;
					logFileFilterLevel = value;
					if (logFileListener != null)
						logFileListener.Filter = new EventTypeFilter(logFileFilterLevel);
				}
			}
		}

		public bool WatchConfigFileChanges
		{
			get {
				lock (SyncLock) {
					return watchConfigFileChanges;
				}
			}
			set {
				lock (SyncLock) {
					if (watchConfigFileChanges == value)
						return;
					watchConfigFileChanges = value;
				}
				setupConfigFileWatcher();
			}
		}

		public bool ToggleLogFile
		{
			get {
				lock (SyncLock) {
					return toggleLogFile;
				}
			}
			set {
				lock (SyncLock) {
					toggleLogFile = value;
					switch (toggleLogFile) {
						case true when logFileListener != null :
						case false when logFileListener == null :
							return;
					}
				}
				setupTracingUnsafe();
			}
		}


		/// <summary>
		/// Not null only when file tracing is enabled. This same listener is added
		/// to all selected sources; and implements the file output. The name AND File
		/// name are set from <see cref="GetLogFileName"/>, and the folder path is set from
		/// <see cref="GetLogFolderPath"/>. Note that the instance is disposed
		/// and recreated as coinfigurations here change.
		/// </summary>
		public TraceListener LogFileListener
		{
			get {
				lock (SyncLock) {
					return logFileListener;
				}
			}
		}

		/// <summary>
		/// This property will return true when the log output file has been enabled.
		/// </summary>
		public bool IsLogFileEnabled
		{
			get {
				lock (SyncLock) {
					return LogFileListener != null;
				}
			}
		}

		/// <summary>
		/// This property will return true if this instance locates a config file
		/// as specified by <see cref="GetConfigFileName"/>.
		/// </summary>
		public bool HasLoadedConfigFile
		{
			get {
				lock (SyncLock) {
					return logFileFactoryConfig != null;
				}
			}
		}


		/// <summary>
		/// This returns the name that is used BOTH for the file output
		/// <see cref="TraceListener"/> itself, AND is the BASE name for the created output
		/// file. This returns "<see cref="IAppScope.AppGuid"/>". Notice that this
		/// name is a base name: the actual file names also contain a date/time, and/or
		/// Uid --- since this implementation uses a rolling file.
		/// </summary>
		/// <returns>Not null.</returns>
		public string GetLogFileName()
			=> AppScope.AppGuid;

		/// <summary>
		/// Returns the full path to the DIRECTORY that will contain the file output
		/// <see cref="TraceListener"/> rolling files. The complete path is the root
		/// folder defined by this <see cref="Environment.SpecialFolder"/> --- OR, if that was
		/// specified as null, then the root folder is <see cref="Path.GetTempPath"/>
		/// --- combined with the <see cref="IAppScope.GetAppDataFolderPath"/>; and,
		/// any further optional subfolder defined within.
		/// </summary>
		/// <returns>Not null: absolute, and does not include the file name.</returns>
		public string GetLogFolderPath()
		{
			string path
					= Path.Combine(
							LogFileRootLocation.HasValue
									? Environment.GetFolderPath(LogFileRootLocation.Value)
									: Path.GetTempPath(),
							AppScope.GetAppDataFolderPath());
			return string.IsNullOrEmpty(LogFileInnerSubFolder)
					? path
					: Path.Combine(path, LogFileInnerSubFolder);
		}

		/// <summary>
		/// This method returns the file name for a supported <see cref="LogFileFactoryConfig"/>
		/// Xml file, which can be located in the <see cref="GetLogFolderPath"/>. This will
		/// return "GetLogFileName()-LogFileFactoryConfig.xml".
		/// (<see cref="GetLogFileName"/> will return the <see cref="IAppScope.AppGuid"/>.)
		/// </summary>
		/// <returns>Not null.</returns>
		public string GetConfigFileName()
			=> $"{GetLogFileName()}-{nameof(LogFileFactoryConfig)}.xml";

		/// <summary>
		/// This method returns the full path to a file for a supported <see cref="LogFileFactoryConfig"/>
		/// Xml file: please see <see cref="GetConfigFileName"/>.
		/// </summary>
		/// <returns>Not null.</returns>
		public string GetConfigFileFullPath()
			=> Path.Combine(
					GetLogFolderPath(),
					GetConfigFileName());


		/// <summary>
		/// Convenience method flushes this <see cref="LogFileListener"/>, flushes
		/// <see cref="TraceSources"/>, <see cref="Trace"/>, and <see cref="Debug"/>.
		/// This will be invoked when disposed.
		/// </summary>
		public void FlushAll()
		{
			TraceSources.FlushAll();
			LogFileListener?.Flush();
			Trace.Flush();
			Debug.Flush();
		}


		/// <summary>
		/// Invoked from <see cref="Dispose"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from a finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (!isDisposing)
				return;
			traceAction($"{nameof(LogFileFactory)} Is Disposing ...");
			disposeResources();
			lock (SyncLock) {
				selectors.Clear();
				isInitialized = false;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
