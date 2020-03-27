using System;


namespace Sc.Util.System
{
	/// <summary>
	/// Static <see cref="Environment"/> helpers.
	/// </summary>
	public static class EnvironmentHelper
	{
		/// <summary>
		/// Static helper method gets the specified boolean environment
		/// <paramref name="variable"/> from the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">Specifies the environment target to search.</param>
		/// <param name="variable">The variable to get.</param>
		/// <param name="defaultToTrue">Specifies the default interpreted value. This
		/// defaults to false: the environment variable must parse into a boolean
		/// as "True", and then the method will return true. Any other value will
		/// return false. Also returns false if the variable is not defined.
		/// If this is set <see langword="true"/>, then the method will only
		/// return false if the variable is not defined or otherwise only if
		/// the defined value is "False" --- any defined value other than
		/// "False" will return true.</param>
		/// <returns>Returns false if the <paramref name="variable"/> is not defined.
		/// Otherwise, if <paramref name="defaultToTrue"/> is false, then this returns
		/// true only if the value parses into a boolean as "True". If
		/// <paramref name="defaultToTrue"/> is true, then the method returns true
		/// if the varaible is defined with any value that is not "False".</returns>
		public static bool IsVariableSet(
				this EnvironmentVariableTarget target,
				string variable,
				bool defaultToTrue = false)
		{
			try {
				variable = Environment.GetEnvironmentVariable(variable, target);
				if (variable == null)
					return false;
				return bool.TryParse(variable, out bool value)
						? value
						: defaultToTrue;
			} catch {
				return false;
			}
		}

		/// <summary>
		/// Static helper method gets the specified boolean environment
		/// <paramref name="variable"/> from any <see cref="EnvironmentVariableTarget"/>.
		/// Searches <see cref="EnvironmentVariableTarget.Process/>, then
		/// <see cref="EnvironmentVariableTarget.User"/>, then
		/// <see cref="EnvironmentVariableTarget.Machine"/>.
		/// </summary>
		/// <param name="target">Specifies the environment target to search.</param>
		/// <param name="variable">The variable to get.</param>
		/// <param name="defaultToTrue">Specifies the default interpreted value. This
		/// defaults to false: the environment variable must parse into a boolean
		/// as "True", and then the method will return true. Any other value will
		/// return false. Also returns false if the variable is not defined.
		/// If this is set <see langword="true"/>, then the method will only
		/// return false if the variable is not defined or otherwise only if
		/// the defined value is "False" --- any defined value other than
		/// "False" will return true.</param>
		/// <returns>Returns false if the <paramref name="variable"/> is not defined.
		/// Otherwise, if <paramref name="defaultToTrue"/> is false, then this returns
		/// true only if the value parses into a boolean as "True". If
		/// <paramref name="defaultToTrue"/> is true, then the method returns true
		/// if the varaible is defined with any value that is not "False".</returns>
		public static bool IsVariableSet(string variable, bool defaultToTrue = false)
		{
			try {
				variable = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process)
						?? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User)
						?? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
				if (variable == null)
					return false;
				return bool.TryParse(variable, out bool value)
						? value
						: defaultToTrue;
			} catch {
				return false;
			}
		}
	}
}
