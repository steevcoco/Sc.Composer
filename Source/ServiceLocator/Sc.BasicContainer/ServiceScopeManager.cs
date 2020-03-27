using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sc.Abstractions.ServiceLocator;
using Sc.Util.System;


namespace Sc.BasicContainer
{
	/// <summary>
	/// <see cref="IServiceScopeManager"/> implementation.
	/// Note that this implementation is <see cref="IDisposable"/>, but
	/// it will not dispose any added services or scopes.
	/// </summary>
	public sealed class ServiceScopeManager
			: IServiceScopeManager,
					IDisposable
	{
		private readonly Dictionary<Type, Dictionary<object, object>> registrations
				= new Dictionary<Type, Dictionary<object, object>>(8);


		/// <summary>
		/// Removes the service under the <paramref name="scopeType"/>
		/// and the <paramref name="scope"/> key instance.
		/// </summary>
		/// <param name="scopeType">Required <c>TScope</c> type.</param>
		/// <param name="scope">Required scope key instance; must be of type
		/// <paramref name="scopeType"/>.</param>
		/// <returns>True if found and removed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public bool Remove(Type scopeType, object scope)
		{
			if (scopeType == null)
				throw new ArgumentNullException(nameof(scopeType));
			if (scope == null)
				throw new ArgumentNullException(nameof(scope));
			if (!scopeType.IsInstanceOfType(scope)) {
				throw new ArgumentException(
						$"Scope is not of scope type '{scopeType.GetFriendlyFullName()}' --- {scope}.");
			}
			lock (registrations) {
				if (registrations.TryGetValue(scopeType, out Dictionary<object, object> scopes)
						&& scopes.Remove(scope)) {
					if (scopes.Count == 0)
						registrations.Remove(scopeType);
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Returns a new array of all added service instances of the given
		/// <typeparamref name="TService"/> type, and EITHER returns instances
		/// added under ALL scopes --- if the <paramref name="scopeType"/>
		/// IS NULL --- or otherwise returns all services under that
		/// <paramref name="scopeType"/>, added under any Scope key
		/// for that type (only).
		/// </summary>
		/// <param name="scopeType">NOTICE: CAN be null; and if null, then ALL
		/// services oif the given <typeparamref name="TService"/> type are returned
		/// --- under all scope types and scope keys. Otherwise all services
		/// of the <typeparamref name="TService"/> type added under this
		/// <paramref name="scopeType"/> are returned, for all of those
		/// scope keys.</param>
		/// <returns>Not null; may be empty</returns>
		public TService[] GetAllServices<TService>(Type scopeType)
		{
			lock (registrations) {
				return scopeType == null
						? registrations.SelectMany(scopeTypeDictionary => scopeTypeDictionary.Value.Values)
								.OfType<TService>()
								.ToArray()
						: registrations.TryGetValue(scopeType, out Dictionary<object, object> scopeDictionary)
								? scopeDictionary.Values
										.OfType<TService>()
										.ToArray()
								: new TService[0];
			}
		}


		public bool TryGet<TScope, TService>(TScope scope, out TService service)
		{
			if (scope == null)
				throw new ArgumentNullException(nameof(scope));
			lock (registrations) {
				if (registrations.TryGetValue(typeof(TScope), out Dictionary<object, object> scopes)
						&& scopes.TryGetValue(scope, out object value)
						&& value is TService tValue) {
					service = tValue;
					return true;
				}
				service = default;
				return false;
			}
		}

		public IDisposable GetOrAdd<TScope, TService>(
				TScope scope,
				out TService service,
				out bool wasAdded,
				Func<TScope, TService> serviceFactory)
		{
			lock (registrations) {
				if (TryGet(scope, out service)) {
					wasAdded = false;
					return DelegateDisposable.NoOp();
				}
				service = serviceFactory(scope);
				if (service == null) {
					throw new ArgumentException(
							$"{nameof(service)} result is null for {typeof(TService).GetFriendlyFullName()}"
							+ $" --- for scope '{scope}'.",
							nameof(serviceFactory));
				}
				if (!registrations.TryGetValue(typeof(TScope), out Dictionary<object, object> scopes)) {
					scopes = new Dictionary<object, object>(8);
					registrations[typeof(TScope)] = scopes;
				} else
					Debug.Assert(!scopes.ContainsKey(scope), "!scopes.ContainsKey(scope)");
				scopes[scope] = service;
				wasAdded = true;
				return DelegateDisposable.With(scope, s => Remove(typeof(TScope), s));

			}
		}


		public void Dispose()
		{
			lock (registrations) {
				registrations.Clear();
			}
		}
	}
}
