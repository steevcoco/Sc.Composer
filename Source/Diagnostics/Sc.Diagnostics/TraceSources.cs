using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sc.Abstractions.Diagnostics;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Static source for <see cref="TraceSource"/> instances. This class maintains
	/// a dictionary of <see cref="WeakReference"/> to <see cref="TraceSource"/>
	/// instances (that implement <see cref="ITrace"/>); which are fetched
	/// by a <see cref="Type"/> or <see cref="Assembly"/>.
	/// Because the weak cache is maintained, the class supports global
	/// configuration; but it does not maintain a ConfigurationSection.
	/// To perform configuration, you may use the static
	/// <see cref="AddSelector"/>
	/// method to configure all new instances.
	/// </summary>
	public static class TraceSources
	{
		private static readonly WeakReferenceDictionary<string, SimpleTraceSource> traceSources;
		private static List<ITraceSourceSelector> selectors;
		private static int lastGcGen2CollectionCount;


		/// <summary>
		/// Static initializer.
		/// </summary>
		static TraceSources()
			=> TraceSources.traceSources = new WeakReferenceDictionary<string, SimpleTraceSource>();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void pruneUnsafe()
		{
			if (TraceSources.lastGcGen2CollectionCount == GC.CollectionCount(2))
				return;
			TraceSources.lastGcGen2CollectionCount = GC.CollectionCount(2);
			TraceSources.traceSources.Prune();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static SimpleTraceSource gerOrAdd(string name)
		{
			lock (TraceSources.traceSources) {
				TraceSources.pruneUnsafe();
				return TraceSources.traceSources.GetOrAdd(name, Add);
				SimpleTraceSource Add()
				{
					SimpleTraceSource result = new SimpleTraceSource(new TraceSource(name));
					if (TraceSources.selectors == null)
						return result;
					foreach (ITraceSourceSelector selector in TraceSources.selectors) {
						selector.Select(result);
					}
					return result;
				}
			}
		}


		/// <summary>
		/// Provided to allow configuring all new <see cref="TraceSource"/>
		/// instances constructed here. This method retains the argument,
		/// and it is invoked NOW with all existing sources; and will be invoked with
		/// all new sources until removed (see <see cref="RemoveSelector"/>).
		/// Notice that the delegate is expected to execute quickly.
		/// </summary>
		/// <param name="selector">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void AddSelector(ITraceSourceSelector selector)
		{
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));
			lock (TraceSources.traceSources) {
				if (TraceSources.selectors == null) {
					TraceSources.selectors = new List<ITraceSourceSelector>(1)
					{
							selector
					};
				} else {
					if (TraceSources.selectors.Contains(selector))
						return;
					TraceSources.selectors.Add(selector);
				}
				TraceSources.pruneUnsafe();
				foreach (SimpleTraceSource traceSource in TraceSources.traceSources.Values) {
					selector.Select(traceSource);
				}
			}
		}

		/// <summary>
		/// Removes a delegate added in <see cref="AddSelector"/>.
		/// </summary>
		/// <param name="selector">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void RemoveSelector(ITraceSourceSelector selector)
		{
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));
			lock (TraceSources.traceSources) {
				if (TraceSources.selectors != null) {
					if (!TraceSources.selectors.Remove(selector))
						return;
					if (TraceSources.selectors.Count == 0)
						TraceSources.selectors = null;
				}
				foreach (SimpleTraceSource traceSource in TraceSources.traceSources.Values) {
					selector.Remove(traceSource);
				}
			}
		}


		/// <summary>
		/// Invokes <see cref="TraceSource.Flush"/> on all live instances.
		/// </summary>
		public static void FlushAll()
		{
			lock (TraceSources.traceSources) {
				TraceSources.pruneUnsafe();
				foreach (SimpleTraceSource traceSource in TraceSources.traceSources.Values) {
					traceSource.TraceSource.Flush();
				}
			}
		}


		/// <summary>
		/// Fetches or creates the weakly-held singleton instance for
		/// the <see cref="AssemblyName.Name"/> of the given <see cref="Assembly"/>.
		/// </summary>
		/// <param name="assembly">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SimpleTraceSource For(Assembly assembly)
			=> TraceSources.gerOrAdd(
					(assembly
							?? throw new ArgumentNullException(nameof(assembly)))
					.GetName()
					.Name);

		/// <summary>
		/// Fetches or creates the weakly-held singleton instance
		/// for the <see cref="TypeHelper"/> <see cref="TypeHelper.GetFriendlyFullName"/>
		/// of the given <see cref="Type"/>.
		/// </summary>
		/// <typeparam name="T">The captured type.</typeparam>
		/// <returns>Not null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SimpleTraceSource For<T>()
			=> TraceSources.For(typeof(T));

		/// <summary>
		/// Fetches or creates the weakly-held singleton instance
		/// for the <see cref="TypeHelper"/> <see cref="TypeHelper.GetFriendlyFullName"/>
		/// of the given <see cref="Type"/>.
		/// </summary>
		/// <param name="type">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SimpleTraceSource For(Type type)
			=> TraceSources.gerOrAdd(type?.GetFriendlyFullName() ?? throw new ArgumentNullException(nameof(type)));
	}
}
