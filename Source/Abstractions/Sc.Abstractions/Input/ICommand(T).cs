using System.Windows.Input;


namespace Sc.Abstractions.Input
{
	/// <summary>
	/// Refines <see cref="ICommand"/> to specify the generic parameter type.
	/// </summary>
	/// <typeparam name="T">The parameter type.</typeparam>
	public interface ICommand<in T>
			: ICommand
	{
		/// <summary>
		/// As with <see cref="ICommand.CanExecute"/>: this takes the generic parameter type.
		/// </summary>
		/// <param name="parameter">Optional; according to the command implementation.</param>
		/// <returns>True if the command can execute, for the parameter.</returns>
		bool CanExecute(T parameter);

		/// <summary>
		/// As with <see cref="ICommand.Execute"/>: this takes the generic parameter type.
		/// </summary>
		/// <param name="parameter">Optional; according to the command implementation.</param>
		void Execute(T parameter);
	}
}
