using System;


namespace Sc.BasicContainer.Specialized
{
	/// <summary>
	/// A read only <see cref="IServiceProvider"/>, that wraps another given
	/// <see cref="IServiceProvider"/>.
	/// </summary>
	public class ReadOnlyServiceProvider
			: IServiceProvider
	{
		private IServiceProvider serviceProvider;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serviceProvider">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ReadOnlyServiceProvider(IServiceProvider serviceProvider)
			=> ServiceProvider = serviceProvider;


		/// <summary>
		/// The delegate. Not null.
		/// </summary>
		protected IServiceProvider ServiceProvider
		{
			get => serviceProvider;
			set => serviceProvider
					= value
					?? throw new ArgumentNullException(nameof(ReadOnlyServiceProvider.ServiceProvider));
		}


		public object GetService(Type serviceType)
			=> ServiceProvider.GetService(serviceType);
	}
}
