using System;
using System.IO;
using System.Xml;


namespace Sc.Util.Xml
{
	/// <summary>
	/// Static helpers for Xml objects.
	/// </summary>
	public static class XmlHelper
	{
		/// <summary>
		/// Tries to load a new <see cref="XmlDocument"/> from the
		/// <paramref name="filePath"/>. Will raise an exception if the file path does not
		/// yield a root document element.
		/// </summary>
		/// <param name="filePath">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="XmlException"></exception>
		public static XmlDocument TryLoadXmlDocumentFromFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			string originalFilePath = filePath;
			if (!File.Exists(filePath))
				filePath = Path.ChangeExtension(originalFilePath, "xml");
			if (!File.Exists(filePath))
				filePath = originalFilePath + ".xml";
			if (!File.Exists(filePath)) {
				throw new FileNotFoundException(
						$"No Xml file found at or like '{originalFilePath}'.",
						originalFilePath);
			}
			XmlDocument xmlDocument = new XmlDocument();
			try {
				xmlDocument.Load(filePath);
			} catch (XmlException) {
				throw;
			} catch (Exception exception) {
				throw new XmlException(
						$"Unable to parse Xml document at '{filePath}'. --- '{exception.Message}'.",
						exception);
			}
			if (xmlDocument.DocumentElement == null)
				throw new XmlException($"Parsed Xml file at '{filePath}' has no DocumentElement: {xmlDocument}.");
			return xmlDocument;
		}
	}
}
