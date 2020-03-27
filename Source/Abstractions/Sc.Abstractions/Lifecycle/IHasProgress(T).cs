using System;


namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// This interface defines an object that is reporting an <see cref="IProgress{T}"/>
	/// <see cref="Progress"/>.
	/// </summary>
	/// <typeparam name="TProgress">The <see cref="IProgress{T}"/> type.</typeparam>
	public interface IHasProgress<TProgress>
	{
		/// <summary>
		/// This instance's current progress. When changed, this will raise the
		/// <see cref="ProgressChanged"/> event.
		/// </summary>
		TProgress Progress { get; }

		/// <summary>
		/// This event is raised with each progress report posted by this object.
		/// </summary>
		event EventHandler<TProgress> ProgressChanged;
	}
}
