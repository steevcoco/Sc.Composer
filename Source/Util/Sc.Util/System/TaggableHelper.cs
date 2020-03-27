using System;
using System.Runtime.Serialization;
using Sc.Abstractions.System;


namespace Sc.Util.System
{
	/// <summary>
	/// A helper class for classes to implement <see cref="ITaggable"/>.
	/// The interface can be implemented by delegating to methods on this object.
	/// Thread safe. Also implements <see cref="IDisposable"/>; and
	/// is <see cref="DataContractAttribute"/>.
	/// Can be subclassed; and provides <see cref="TagsChanged"/>.
	/// See also <see cref="TaggableHelper{TKey, TValue}"/>.
	/// </summary>
	[DataContract]
	public class TaggableHelper
			: TaggableHelper<object, object> { }
}
