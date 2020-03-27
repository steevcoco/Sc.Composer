using System.IO;


namespace Sc.Abstractions.IO
{
	/// <summary>
	/// Defines an object that can serialize and deserialize itself to a
	/// <see cref="UnmanagedMemoryAccessor"/>.
	/// </summary>
	public interface IMappable
	{
		/// <summary>
		/// Deserialize this object from the <see cref="UnmanagedMemoryAccessor"/>. NOTICE that you must
		/// not close the accessor.
		/// </summary>
		/// <param name="position">The position at which to begin reading.</param>
		/// <param name="accessor">Not null.</param>
		void ReadFrom(long position, UnmanagedMemoryAccessor accessor);

		/// <summary>
		/// Serialize this object to the <see cref="UnmanagedMemoryAccessor"/>. NOTICE that you must
		/// not close the accessor.
		/// </summary>
		/// <param name="position">The position at which to begin writing.</param>
		/// <param name="accessor">Not null.</param>
		void WriteTo(long position, UnmanagedMemoryAccessor accessor);
	}
}
