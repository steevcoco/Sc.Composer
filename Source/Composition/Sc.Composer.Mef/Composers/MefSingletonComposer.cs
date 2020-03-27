using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using Sc.Collections;
using Sc.Diagnostics;
using Sc.Util.Collections;


namespace Sc.Composer.Mef.Composers
{
	/// <summary>
	/// A basic Mef <see cref="IComposer{TTarget}"/>
	/// implementation that performs composition; and then provides
	/// a thread-safe collection of shared Exported parts.
	/// The collection is augmented on each composition.
	/// This can be used as a backing collection for discovered parts of your Export type(s). This
	/// class extends <see cref="MefComposer"/> to provide a simple thread-safe collection for the
	/// <see cref="ExportsList"/>. All Exports are composed into a new <see cref="CompositionContext"/>
	/// on each event, and are added to this <see cref="ExportsList"/>. You can provide a delegate
	/// to filter each composition event. If you retain any new Exports, then the
	/// <see cref="CompositionHost"/> hosting those Exports is also retained --- and otherwise it
	/// is disposed immediately. All <see cref="CompositionHost"/> containers are retained until
	/// this class discovers that you have removed all exports hosted there, and at that time
	/// the host will be disposed. You may also mutate this exports collection at any time. When
	/// this object is disposed, you may optionally specify that the hosts are NOT disposed
	/// --- preventing the hosts from disposing the exports that they contain at that time.
	/// Please notice that the <see cref="ExportsList"/> may not be mutated recursively.
	/// Can be subclassed.
	/// Notice also, this class extends <see cref="Composer{TTarget}"/>, and this by
	/// default sets <see cref="Composer{TTarget}.ComposeExceptionPolicy"/> to
	/// <see cref="ComposerExceptionPolicy.ThrowNone"/>.
	/// </summary>
	public class MefSingletonComposer
			: MefComposer
	{
		private static readonly CompositionHost unmanagedCompositionHost;


		static MefSingletonComposer()
		{
			MefSingletonComposer.unmanagedCompositionHost = new ContainerConfiguration().CreateContainer();
			MefSingletonComposer.unmanagedCompositionHost.Dispose();
		}


		/// <summary>
		/// Convenience constructor method that creates a new <see cref="MefSingletonComposer"/>, that is
		/// created from a new <see cref="ConventionBuilder"/>, for the given <typeparamref name="TExport"/>
		/// type, and is <c>Shared</c>. The returned object composes only for the single type.
		/// Notice also, this class extends <see cref="Composer{TTarget}"/>, and this by
		/// default sets <see cref="Composer{TTarget}.ComposeExceptionPolicy"/> to
		/// <see cref="ComposerExceptionPolicy.ThrowNone"/>.
		/// </summary>
		/// <typeparam name="TExport">The single export type for the composer.</typeparam>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).
		/// If the type is abstract then ForType is not applied.</param>
		/// <param name="disposeCompositionHostsWithThis">Specifies how all created
		/// <see cref="CompositionHost"/> instances are handled when this object is disposed: notice
		/// that this defaults to true --- when disposed, the contained exports may be disposed. If
		/// not disposed, the references are simply dropped.</param>
		/// <param name="disposeProvidersWithThis">Specifies how added PROVIDERS are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		/// <param name="handleNewExports">Optional argument for the result.</param>
		/// <returns>Not null.</returns>
		public static MefSingletonComposer ForSingleType<TExport>(
				bool? onlyDerivedTypesExclusively,
				bool disposeCompositionHostsWithThis = true,
				bool disposeProvidersWithThis = true,
				Func<List<object>, IReadOnlyList<object>, Action> handleNewExports = null)
			=> MefSingletonComposer.ForSingleType(
					typeof(TExport),
					onlyDerivedTypesExclusively,
					disposeCompositionHostsWithThis,
					disposeProvidersWithThis,
					handleNewExports);

		/// <summary>
		/// Convenience constructor method that creates a new <see cref="MefSingletonComposer"/>, that is
		/// created from a new <see cref="ConventionBuilder"/>, for the given
		/// <paramref name="exportType"/>
		/// type, and is <c>Shared</c>. The returned object composes only for the single type.
		/// Notice also, this class extends <see cref="Composer{TTarget}"/>, and this by
		/// default sets <see cref="Composer{TTarget}.ComposeExceptionPolicy"/> to
		/// <see cref="ComposerExceptionPolicy.ThrowNone"/>.
		/// </summary>
		/// <param name="exportType">The single export type for the composer.</param>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).
		/// If the type is abstract then ForType is not applied.</param>
		/// <param name="disposeCompositionHostsWithThis">Specifies how all created
		/// <see cref="CompositionHost"/> instances are handled when this object is disposed: notice
		/// that this defaults to true --- when disposed, the contained exports may be disposed. If
		/// not disposed, the references are simply dropped.</param>
		/// <param name="disposeProvidersWithThis">Specifies how added PROVIDERS are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		/// <param name="handleNewExports">Optional argument for the result.</param>
		/// <returns>Not null.</returns>
		public static MefSingletonComposer ForSingleType(
				Type exportType,
				bool? onlyDerivedTypesExclusively,
				bool disposeCompositionHostsWithThis = true,
				bool disposeProvidersWithThis = true,
				Func<List<object>, IReadOnlyList<object>, Action> handleNewExports = null)
		{
			if (exportType == null)
				throw new ArgumentNullException(nameof(exportType));
			return new MefSingletonComposer(
					exportType.AsSingle(),
					MefComposerHelper.CreateDefaultConventions(exportType, onlyDerivedTypesExclusively),
					disposeCompositionHostsWithThis,
					disposeProvidersWithThis,
					handleNewExports);
		}

		/// <summary>
		/// Convenience constructor method that creates a new <see cref="MefSingletonComposer"/>, that is
		/// created from a new <see cref="ConventionBuilder"/>, for each given <c>exportTypes</c>,
		/// and each is <c>Shared</c>. The returned object composes only for the given types.
		/// Notice also, this class extends <see cref="Composer{TTarget}"/>, and this by
		/// default sets <see cref="Composer{TTarget}.ComposeExceptionPolicy"/> to
		/// <see cref="ComposerExceptionPolicy.ThrowNone"/>.
		/// </summary>
		/// <param name="exportTypes">The shared export types for the composer.</param>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).
		/// If the type is abstract then ForType is not applied.</param>
		/// <param name="disposeCompositionHostsWithThis">Specifies how all created
		/// <see cref="CompositionHost"/> instances are handled when this object is disposed: notice
		/// that this defaults to true --- when disposed, the contained exports may be disposed. If
		/// not disposed, the references are simply dropped.</param>
		/// <param name="disposeProvidersWithThis">Specifies how added PROVIDERS are handled when
		/// this object is disposed: notice that this defaults to true.</param>s
		/// <param name="handleNewExports">Optional argument for the result.</param>
		/// <returns>Not null.</returns>
		public static MefSingletonComposer ForTypes(
				IEnumerable<Type> exportTypes,
				bool? onlyDerivedTypesExclusively,
				bool disposeCompositionHostsWithThis = true,
				bool disposeProvidersWithThis = true,
				Func<List<object>, IReadOnlyList<object>, Action> handleNewExports = null)
		{
			if (exportTypes == null)
				throw new ArgumentNullException(nameof(exportTypes));
			Type[] exportTypesArray = exportTypes.ToArray();
			ConventionBuilder conventions = new ConventionBuilder();
			foreach (Type exportType in exportTypesArray) {
				conventions.ApplyConventions(exportType, onlyDerivedTypesExclusively);
			}
			return new MefSingletonComposer(
					exportTypesArray,
					conventions,
					disposeCompositionHostsWithThis,
					disposeProvidersWithThis,
					handleNewExports);
		}


		private readonly Func<List<object>, IReadOnlyList<object>, Action> handleNewExports;

		private readonly ReadWriteCollection<MultiDictionary<CompositionHost, object>> exports
				= new ReadWriteCollection<MultiDictionary<CompositionHost, object>>(
						new MultiDictionary<CompositionHost, object>(
								EquatableHelper.ReferenceEqualityComparer<CompositionHost>(),
								0),
						dictionary => new MultiDictionary<CompositionHost, object>(
								dictionary,
								EquatableHelper.ReferenceEqualityComparer<CompositionHost>()));

		private bool disposeCompositionHostsWithThis;


		/// <summary>
		/// Constructor. Notice that the default <paramref name="handleNewExports"/> delegate
		/// will always compose all Assemblies on every event sequence, and add all new Exports that
		/// do not compare value-equal to any existing Export. You may prefer to filter either the
		/// Assemblies or the composed Exports.
		/// Notice also, this class extends <see cref="Composer{TTarget}"/>, and this by
		/// default sets <see cref="Composer{TTarget}.ComposeExceptionPolicy"/> to
		/// <see cref="ComposerExceptionPolicy.ThrowNone"/>.
		/// </summary>
		/// <param name="exportTypes">Required: defines the Exports that will be composed on
		/// each event. All Exports of each type here are fetched from the
		/// <see cref="CompositionHost"/> that is constructed in the
		/// <see cref="Composer{TTarget}.GetTarget"/> handler; and these objects are added to
		/// the <see cref="ExportsList"/> (or handled by your handler).</param>
		/// <param name="defaultConventions">Optional conventions used on each composition to construct each
		/// <see cref="ContainerConfiguration"/>.</param>
		/// <param name="disposeCompositionHostsWithThis">Specifies how all created
		/// <see cref="CompositionHost"/> instances are handled when this object is disposed: notice
		/// that this defaults to true --- when disposed, the contained exports may be disposed. If
		/// not disposed, the references are simply dropped.</param>
		/// <param name="disposeProvidersWithThis">Specifies how added PROVIDERS are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		/// <param name="handleNewExports">An optional handler that is invoked on every composition, which
		/// does all the work of adding new Exports, and potentially removing (and Disposing) existing
		/// Exports. This composer will perform a part composition event sequence, and then it invokes this
		/// delegate. The first argument is mutable, and contains the current <see cref="ExportsList"/>.
		/// You must directly modify this list as desired. If you remove any elements, you must handle
		/// any disposal yourself. The second argument contains the new Exports composed on this composition
		/// event. You must add any desired elements. These new Exports are constructed in a new
		/// <see cref="CompositionHost"/>, which is retained if you retain any of these objects, and
		/// otherwise will be disposed immediately after invoking this delegate. You should only dispose
		/// any new Exports that you do not add if you are able to safely do so, and
		/// if the disposal of that transient <see cref="CompositionHost"/> will not be sufficient. If
		/// you don't provide this delegate, the composer always adds any new Exports that do not compare
		/// value-equal to any existing Export, and will NOT dispose any new Exports that were not
		/// added --- it presumes that the disposal of that transient <see cref="CompositionHost"/>
		/// is sufficient, and prevents disposing objects that may be linked in a graph. Your delegate
		/// also returns an Action: this may be null, and if not null, it is invoked after the
		/// mutation is complete. This delegate is invoked on the
		/// <see cref="Composer{TTarget}.WithAllParts"/> event --- after all
		/// <see cref="IProvideParts{TTarget}"/> callbacks, and before
		/// <see cref="IBootstrap{TTarget}"/>.</param>
		public MefSingletonComposer(
				IEnumerable<Type> exportTypes,
				AttributedModelProvider defaultConventions,
				bool disposeCompositionHostsWithThis = true,
				bool disposeProvidersWithThis = true,
				Func<List<object>, IReadOnlyList<object>, Action> handleNewExports = null)
				: base(defaultConventions, disposeProvidersWithThis)
		{
			if (exportTypes == null)
				throw new ArgumentNullException(nameof(exportTypes));
			ExportTypes = exportTypes.ToArray();
			if (ExportTypes.Count == 0)
				throw new ArgumentException(nameof(exportTypes));
			this.disposeCompositionHostsWithThis = disposeCompositionHostsWithThis;
			this.handleNewExports = handleNewExports ?? HandleNewExports;
			Action HandleNewExports(List<object> exportsList, IReadOnlyList<object> newExports)
			{
				foreach (object newExport in newExports) {
					if (!exportsList.Contains(newExport))
						exportsList.Add(newExport);
				}
				return null;
			}
		}


		private TResult handleMutate<TResult>(
				MultiDictionary<CompositionHost, object> exportsList,
				Func<List<object>, TResult> mutate,
				CompositionHost addUnder)
		{
			IEqualityComparer<object> referenceEqualityComparer
					= EquatableHelper.ReferenceEqualityComparer<object>();
			MultiDictionary<CompositionHost, object> currentCopy = exports.CopyCollection(exportsList);
			List<object> currentValuesCopy = exportsList.GetAllValues();
			List<object> currentValues = exportsList.GetAllValues();
			TResult result = mutate(currentValues);
			// Removed?
			foreach (object currentValueCopy in currentValuesCopy) {
				if (currentValues.Contains(currentValueCopy, referenceEqualityComparer))
					continue;
				KeyValuePair<CompositionHost, object> entry
						= currentCopy.FirstOrDefault(kv => object.ReferenceEquals(currentValueCopy, kv.Value));
				exportsList.Remove(entry);
				if (!exportsList.ContainsKey(entry.Key)
						&& !object.ReferenceEquals(MefSingletonComposer.unmanagedCompositionHost, entry.Key))
					entry.Key.Dispose();
			}
			// Added?
			foreach (object currentValue in currentValues) {
				if (currentValuesCopy.Contains(currentValue, referenceEqualityComparer))
					continue;
				exportsList.AddValue(addUnder, currentValue);
			}
			return result;
		}


		protected sealed override void WithAllParts(ContainerConfiguration target)
		{
			Action Mutate(MultiDictionary<CompositionHost, object> exportsList)
			{
				try {
					CompositionHost compositionHost = target.CreateContainer();
					List<object> newExports = new List<object>();
					foreach (Type compositionContract in ExportTypes) {
						newExports.AddRange(compositionHost.GetExports(compositionContract));
					}
					Action OnMutate(List<object> list)
						=> handleNewExports(list, newExports);
					return handleMutate(exportsList, OnMutate, compositionHost);
				} catch (Exception exception) {
					TraceSources.For<MefSingletonComposer>()
							.Error(
									exception,
									"Catching exception refreshing Exports: {0}",
									exception.Message);
					throw;
				}
			}
			exports.Mutate(Mutate)
					?.Invoke();
		}


		/// <summary>
		/// The value given on construction.
		/// </summary>
		public virtual bool DisposeCompositionHostsWithThis
		{
			get {
				lock (SyncLock) {
					return disposeCompositionHostsWithThis;
				}
			}
			set {
				lock (SyncLock) {
					disposeCompositionHostsWithThis = value;
				}
			}
		}

		/// <summary>
		/// The actual list of Export Types that are composed on each event.
		/// </summary>
		public IReadOnlyCollection<Type> ExportTypes { get; }

		/// <summary>
		/// Returns a new list of all current Exports. This list is thread safe because it
		/// is a new copy, but note that reads of this property are not atomic, and so if the
		/// composer updates while you inspect the list, your next read of this property may
		/// return a different collection. To perform an atomic operation,
		/// you may use the <see cref="Mutate"/> method.
		/// </summary>
		public IReadOnlyList<object> ExportsList
			=> exports.Collection.GetAllValues();

		/// <summary>
		/// This method allows you to arbitrarily mutate the <see cref="ExportsList"/> at any time.
		/// This method is thread safe. Notice that the collection cannot be mutated recursively.
		/// </summary>
		/// <param name="mutate">Not null. This will be passed a copy of the current <see cref="ExportsList"/>
		/// value: you must mutate the argument as desired, and that collection then becomes the
		/// new <see cref="ExportsList"/>. Your delegate executes under a Monitor.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void Mutate(Action<List<object>> mutate)
		{
			void Mutate(MultiDictionary<CompositionHost, object> exportsList)
			{
				bool OnMutate(List<object> list)
				{
					mutate(list);
					return true;
				}
				handleMutate(exportsList, OnMutate, MefSingletonComposer.unmanagedCompositionHost);
			}
			exports.Mutate(Mutate);
		}

		/// <summary>
		/// This method allows you to arbitrarily mutate the <see cref="ExportsList"/> at any time.
		/// This method is thread safe. This method allows your
		/// <see cref="Func{TIn,TResult}"/> to return a value back from this method. Notice that the
		/// collection cannot be mutated recursively.
		/// </summary>
		/// <typeparam name="TResult">Your own result type.</typeparam>
		/// <param name="mutate">Not null. This will be passed a copy of the current <see cref="ExportsList"/>
		/// value: you must mutate the argument as desired, and that collection then becomes the
		/// new <see cref="ExportsList"/>. Your delegate executes under a Monitor. The result from the
		/// Func is defined by your own invoker: you return any arbitrary value that you may consume
		/// yourself.</param>
		/// <returns>The result of your own <see cref="Func{TIn,TResult}"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public TResult Mutate<TResult>(Func<List<object>, TResult> mutate)
		{
			TResult Mutate(MultiDictionary<CompositionHost, object> exportsList)
				=> handleMutate(exportsList, mutate, MefSingletonComposer.unmanagedCompositionHost);
			return exports.Mutate(Mutate);
		}


		protected override void Dispose(bool isDisposing)
		{
			if (!isDisposing
					|| IsDisposed) {
				base.Dispose(isDisposing);
				return;
			}
			void Mutate(MultiDictionary<CompositionHost, object> exportsList)
			{
				if (DisposeCompositionHostsWithThis) {
					foreach (CompositionHost compositionHost in exportsList.Keys) {
						if (!object.ReferenceEquals(MefSingletonComposer.unmanagedCompositionHost, compositionHost))
							compositionHost.Dispose();
					}
				}
				exportsList.Clear();
			}
			exports.Mutate(Mutate);
			base.Dispose(true);
		}
	}
}
