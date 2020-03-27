using System;


namespace Sc.Abstractions.Internal
{
	/// <summary>
	/// Static helpers.
	/// </summary>
	internal static class Helpers
	{
		/// <summary>
		/// Gets a friendly display Type Name.
		/// </summary>
		/// <param name="type">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string GetFriendlyName(this Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			string friendlyName = type.Name;
			int backtick = friendlyName.IndexOf('`');
			return backtick > 0
					? friendlyName.Substring(0, backtick)
					: friendlyName;
		}

		/// <summary>
		/// Gets a friendly display Type FullName.
		/// </summary>
		/// <param name="type">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string GetFriendlyFullName(this Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			string friendlyName = type.Name;
			int backtick = friendlyName.IndexOf('`');
			return backtick > 0
					? $"{type.Namespace}.{friendlyName.Substring(0, backtick)}"
					: $"{type.Namespace}.{friendlyName}";
		}
	}
}
