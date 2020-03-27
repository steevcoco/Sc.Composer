using System;


namespace Sc.BasicContainer.Specialized
{
	/// <summary>
	/// Simple <see cref="IServiceProvider"/> that invokes a provided
	/// <see cref="Func{TResult}"/> for <see cref="GetService"/>.
	/// </summary>
	public class DelegateServiceProvider
			: IServiceProvider,
					IDisposable
	{
		private readonly object syncLock = new object();
		private Func<Type, object> getServiceDelegate;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getService">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public DelegateServiceProvider(Func<Type, object> getService)
			=> GetServiceDelegate = getService;


		/// <summary>
		/// The delegate. Notice that this is set null when this is disposed.
		/// </summary>
		protected Func<Type, object> GetServiceDelegate
		{
			get {
				lock (syncLock) {
					return getServiceDelegate;
				}
			}
			set {
				lock (syncLock) {
					getServiceDelegate
							= value
							?? throw new ArgumentNullException(nameof(DelegateServiceProvider.GetServiceDelegate));
				}
			}
		}

		public object GetService(Type serviceType)
		{
			lock (syncLock) {
				return getServiceDelegate?.Invoke(serviceType);
			}
		}


		public void Dispose()
			=> getServiceDelegate = null;
	}
}
