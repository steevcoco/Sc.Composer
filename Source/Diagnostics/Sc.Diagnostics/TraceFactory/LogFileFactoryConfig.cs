using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Sc.Diagnostics.Properties;
using Sc.Util.Collections;


namespace Sc.Diagnostics.TraceFactory
{
	/// <summary>
	/// Implements simple POCO properties to configure a <see cref="LogFileFactory"/>.
	/// You can set configuration properties here, and the <see cref="SetPropertiesOn"/>
	/// method will set these properties on a provided <see cref="LogFileFactory"/>.
	/// This also provides the <see cref="LoadFromFile"/> method to parse an
	/// Xml file into an instance of this class; and also implements
	/// <see cref="CreateFrom"/>; which the catory can use to create and
	/// save an instance from its current settings.
	/// </summary>
	public class LogFileFactoryConfig
			: ILogFileFactoryConfig
	{
		/// <summary>
		/// Convenience method creates a new <see cref="PropertyDescriptorCollection"/>
		/// for the properties defined on <see cref="ILogFileFactoryConfig"/>.
		/// </summary>
		/// <returns>Not null.</returns>
		public static IEnumerable<PropertyDescriptor> GetConfigProperties()
			=> TypeDescriptor.GetProperties(typeof(ILogFileFactoryConfig))
					.OfType<PropertyDescriptor>();


		/// <summary>
		/// Tries to load an Xml file at the <paramref name="filePath"/> and parse
		/// and set property values on a new <see cref="LogFileFactoryConfig"/>
		/// instance. The file can be formed with elements like this:
		/// <code>
		/// &lt;LogFileFactoryConfig&gt;
		/// &lt;DefaultTraceSourceSelection&gt;All&lt;/DefaultTraceSourceSelection&gt;
		/// &lt;-- ... --&gt;
		/// &lt;/LogFileFactoryConfig&gt;
		/// </code>
		/// And this method will also first try to parse any attributes defined on the root element
		/// with the property names --- which will take precedence over child elements.
		/// </summary>
		/// <param name="filePath">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="XmlException"></exception>
		public static LogFileFactoryConfig LoadFromFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			XmlDocument xmlDocument = new XmlDocument();
			try {
				xmlDocument.Load(filePath);
			} catch (FileNotFoundException) {
				throw;
			} catch (XmlException) {
				throw;
			} catch (Exception exception) {
				throw new XmlException(
						$"Unable to parse Xml document at '{filePath}'. --- '{exception.Message}'.",
						exception);
			}
			if (xmlDocument.DocumentElement == null)
				throw new XmlException($"Unable to parse Xml document at '{filePath}'.");
			LogFileFactoryConfig logFileFactoryConfig = new LogFileFactoryConfig();
			foreach (PropertyDescriptor property in LogFileFactoryConfig.GetConfigProperties()) {
				if (!xmlDocument.DocumentElement.HasChildNodes) {
					if (property.CanResetValue(logFileFactoryConfig))
						property.ResetValue(logFileFactoryConfig);
					continue;
				}
				bool reset = true;
				foreach (XmlNode node in xmlDocument.DocumentElement.GetElementsByTagName(property.Name)) {
					if (!TryParseAndSetProperty(logFileFactoryConfig, property, node.InnerText))
						continue;
					reset = false;
					break;
				}
				if (reset
						&& property.CanResetValue(logFileFactoryConfig))
					property.ResetValue(logFileFactoryConfig);
			}
			return logFileFactoryConfig;
			static bool TryParseAndSetProperty(
					LogFileFactoryConfig config,
					PropertyDescriptor property,
					string xmlValue)
			{
				if ((property.PropertyType == typeof(LogFileFactorySelection))
						|| (property.PropertyType == typeof(LogFileFactorySelection?))) {
					if (!string.IsNullOrEmpty(xmlValue)
							&& Enum.TryParse(xmlValue, out LogFileFactorySelection value)) {
						property.SetValue(config, value);
						return true;
					}
				} else if ((property.PropertyType == typeof(SourceLevels))
						|| (property.PropertyType == typeof(SourceLevels?))) {
					if (!string.IsNullOrEmpty(xmlValue)
							&& Enum.TryParse(xmlValue, out SourceLevels value)) {
						property.SetValue(config, value);
						return true;
					}
				} else if ((property.PropertyType == typeof(bool))
						|| (property.PropertyType == typeof(bool?))) {
					if (!string.IsNullOrEmpty(xmlValue)
							&& bool.TryParse(xmlValue, out bool value)) {
						property.SetValue(config, value);
						return true;
					}
				}
				if (property.CanResetValue(config))
					property.ResetValue(config);
				return false;
			}
		}

		/// <summary>
		/// Static method will construct a new <see cref="LogFileFactoryConfig"/>
		/// from the current property values on this <paramref name="logFileFactory"/>.
		/// </summary>
		/// <param name="logFileFactory">Not null.</param>
		/// <param name="xmlDocument">Returns the parsed Xml document.</param>
		/// <returns>Not null.</returns>
		public static LogFileFactoryConfig CreateFrom(LogFileFactory logFileFactory, out XmlDocument xmlDocument)
		{
			LogFileFactoryConfig logFileFactoryConfig = new LogFileFactoryConfig();
			xmlDocument = new XmlDocument
			{
				PreserveWhitespace = true,
				InnerXml = Resources.LogFileFactoryConfig
			};
			Debug.Assert(
					xmlDocument.DocumentElement != null,
					"xmlDocument.DocumentElement != null");
			Debug.Assert(
					xmlDocument.DocumentElement.HasChildNodes,
					"xmlDocument.DocumentElement.HasChildNodes");
			foreach (PropertyDescriptor property in LogFileFactoryConfig.GetConfigProperties()) {
				XmlElement element
						= xmlDocument.DocumentElement.GetElementsByTagName(property.Name)
								.OfType<XmlElement>()
								.FirstOrDefault();
				if (element == null) {
					element = xmlDocument.CreateElement(property.Name);
					xmlDocument.DocumentElement.AppendChild(element);
				}
				object propertyValue = property.GetValue(logFileFactory);
				if (propertyValue != null)
					element.InnerText = Convert.ToString(propertyValue);
				else {
					propertyValue = property.GetValue(logFileFactoryConfig);
					if (propertyValue != null)
						element.InnerText = Convert.ToString(propertyValue);
				}
			}
			return logFileFactoryConfig;
		}


		[DefaultValue(LogFileFactorySelection.All)]
		public LogFileFactorySelection DefaultTraceSourceSelection { get; set; }
			= LogFileFactorySelection.All;

		[DefaultValue(SourceLevels.Warning)]
		public SourceLevels SelectedSwitchLevel { get; set; }
			= SourceLevels.Warning;

		[DefaultValue(SourceLevels.Information)]
		public SourceLevels LogFileFilterLevel { get; set; }
			= SourceLevels.Information;

		[DefaultValue(true)]
		public bool WatchConfigFileChanges { get; set; }
			= true;

		[DefaultValue(true)]
		public bool ToggleLogFile { get; set; }
			= true;


		/// <summary>
		/// Enumerates these <see cref="GetProperties"/> and sets these values onto the
		/// <paramref name="logFileFactory"/>.
		/// </summary>
		/// <param name="logFileFactory">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void SetPropertiesOn(LogFileFactory logFileFactory)
		{
			if (logFileFactory == null)
				throw new ArgumentNullException(nameof(logFileFactory));
			foreach (PropertyDescriptor propertyDescriptor in LogFileFactoryConfig.GetConfigProperties()) {
				propertyDescriptor.SetValue(logFileFactory, propertyDescriptor.GetValue(this));
			}
		}


		public override string ToString()
			=> $"{GetType().Name}"
					+ $"{LogFileFactoryConfig.GetConfigProperties().ToStringCollection(256, p => $"{p.Name}: {p.GetValue(this)}")}";
	}
}
