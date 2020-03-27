namespace Sc.Abstractions.Messaging
{
	/// <summary>
	/// Defines an interface that invokes or handles updates of a given message type. The interface defines
	/// one <see cref="Update{T}"/> method, which receives the event; and it returns a boolean value. The
	/// return value is defined by your service: it may indicate that your object will or will not continue
	/// to process messages; or that your object has or has not changed state; and it may represent another
	/// signal entirely, or also not be implemented.
	/// </summary>
	/// <typeparam name="TMessage">The message type.</typeparam>
	public interface IMessageUpdate<in TMessage>
	{
		/// <summary>
		/// Process the message if this is a Client; or dispatch messages here if this is a Server.
		/// </summary>
		/// <param name="message">A reference.</param>
		/// <returns>The return signal is defined by your implementation.</returns>
		bool Update<T>(ref T message)
				where T : TMessage;
	}
}
