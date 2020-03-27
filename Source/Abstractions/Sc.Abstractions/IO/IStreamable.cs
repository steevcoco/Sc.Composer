using System.IO;


namespace Sc.Abstractions.IO
{
	/// <summary>
	/// Defines an object that can serialize and deserialize itself to a <see cref="Stream"/>.
	/// </summary>
	public interface IStreamable
	{
		/// <summary>
		/// Deserialize this object from the <see cref="Stream"/>. NOTICE that you must not close
		/// the stream.
		/// </summary>
		/// <param name="stream">Not null.</param>
		void ReadFrom(Stream stream);

		/// <summary>
		/// Serialize this object to the <see cref="Stream"/>. NOTICE that you must not close
		/// the stream.
		/// </summary>
		/// <param name="stream">Not null.</param>
		void WriteTo(Stream stream);
	}
}
