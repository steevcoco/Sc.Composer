using System;
using System.Collections.Generic;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Diagnostics;
using Sc.Collections;


namespace Sc.BasicContainer.Implementation
{
	/// <summary>
	/// Holds state for a top-level Construct or Inject request for
	/// <see cref="ServiceConstructorMethods"/>.
	/// </summary>
	internal sealed class ServiceConstructorRequest
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="logger">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ServiceConstructorRequest(ITrace logger = null)
			=> Logger = logger ?? throw new ArgumentNullException(nameof(logger));


		/// <summary>
		/// The logger for this operation. Not null.
		/// </summary>
		internal ITrace Logger { get; }

		/// <summary>
		/// Internal logging stack. Not null.
		/// </summary>
		internal ISequence<object> TraceStack { get; } = new Sequence<object>(true);

		/// <summary>
		/// Internal list of types being constructed on this operation. Not null.
		/// The list will be added to and removed from as types are constructed
		/// --- it will not wind up containing all types, nor have any
		/// predicatble order.
		/// </summary>
		internal List<Type> ConstructingTypes { get; } = new List<Type>(16);

		/// <summary>
		/// Internal list of all dependencies under this constructed instance. Not null.
		/// </summary>
		internal MultiDictionary<Type, Type> Dependencies { get; } = new MultiDictionary<Type, Type>();
	}
}
