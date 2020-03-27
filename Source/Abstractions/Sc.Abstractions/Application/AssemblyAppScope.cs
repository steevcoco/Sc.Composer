using System;
using System.Reflection;


namespace Sc.Abstractions.Application
{
	/// <summary>
	/// Implements an <see cref="IAppScope"/> using only the Assembly Name:
	/// <see cref="IAppScope.AppName"/>, <see cref="IAppScope.AppGuid"/>,
	/// and <see cref="IAppScope.GetAppDataFolderPath"/> all return the Name.
	/// The <see cref="IAppScope.Version"/> is returned.
	/// </summary>
	public sealed class AssemblyAppScope
			: IAppScope
	{
		private readonly string appDataRootFolder;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="assembly">Not null.</param>
		/// <param name="appDataRootFolder">Optional alternative
		/// <see cref="IAppScope.GetAppDataRootFolder"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public AssemblyAppScope(Assembly assembly, string appDataRootFolder = null)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));
			AppGuid = assembly.GetName().Name;
			Version = assembly.GetName().Version;
			this.appDataRootFolder = appDataRootFolder;
		}

		
		public string AppGuid { get; }

		public string AppName
			=> AppGuid;
		
		public Version Version { get; }

		public string GetAppDataFolderPath()
			=> AppGuid;

		public string GetAppDataRootFolder()
			=> appDataRootFolder;


		public override string ToString()
			=> $"{GetType().Name}"
					+ "["
					+ $"'{AppGuid}' - '{AppName}' - '{Version}' / '{GetAppDataFolderPath()}'."
					+ "]";
	}
}
