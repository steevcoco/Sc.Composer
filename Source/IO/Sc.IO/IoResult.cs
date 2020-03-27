using System;
using System.Threading;
using Sc.Util.System;


namespace Sc.IO
{
	/// <summary>
	/// Results for <see cref="IoResult.Result"/>.
	/// </summary>
	public enum IoResultState
	{
		/// <summary>
		/// The operation was successful.
		/// </summary>
		Success,

		/// <summary>
		/// Used by methods that specify a contract for the operation; and it does not
		/// read or write the expected result.
		/// </summary>
		BadData,

		/// <summary>
		/// THe operation has been canceled.
		/// </summary>
		Cancelled,

		/// <summary>
		/// Error.
		/// </summary>
		Faulted
	}


	/// <summary>
	/// Used to begin and end reads and writes, and convey IO status and a result.
	/// Multiple operations will accumulate state on a single instance.
	/// </summary>
	public class IoResult
	{
		/// <summary>
		/// Monitors all properties.
		/// </summary>
		protected readonly object SyncLock = new object();

		private IoResultState result = IoResultState.Success;
		private long bytesReadOrWritten;
		private Exception error;


		/// <summary>
		/// Constructor. This sets <see cref="CancellationToken"/> to
		/// <see cref="System.Threading.CancellationToken.None"/>.
		/// </summary>
		public IoResult()
				: this(CancellationToken.None) { }

		/// <summary>
		/// Constructor; sets an optional <see cref="CancellationToken"/>.
		/// </summary>
		/// <param name="cancellationToken">Optional.</param>
		public IoResult(CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			if (CancellationToken.CanBeCanceled) {
				CancellationToken.Register(
						() =>
						{
							lock (SyncLock) {
								if (IsSuccess)
									setResult(IoResultState.Cancelled, null);
							}
						});
			}
		}


		private IoResult setResult(IoResultState value, Exception exception)
		{
			lock (SyncLock) {
				if (result != IoResultState.Success)
					return this;
				result = value;
				if (exception != null)
					error = exception;
				return this;
			}
		}


		/// <summary>
		/// This method sets <see cref="BytesReadOrWritten"/> directly
		/// to the given <paramref name="newValue"/>.
		/// </summary>
		/// <param name="newValue">NOTICE: not tested.</param>
		/// <returns>This instance for chaining.</returns>
		public IoResult SetBytesReadOrWritten(long newValue)
		{
			lock (SyncLock) {
				bytesReadOrWritten = newValue;
				return this;
			}
		}

		/// <summary>
		/// This method adds the given <paramref name="addValue"/>
		/// to <see cref="BytesReadOrWritten"/>.
		/// </summary>
		/// <param name="addValue">NOTICE: not tested.</param>
		/// <returns>This instance for chaining.</returns>
		public IoResult AddBytesReadOrWritten(long addValue)
		{
			lock (SyncLock) {
				bytesReadOrWritten += addValue;
				return this;
			}
		}

		/// <summary>
		/// Note: this method only changes the state if this is currently
		/// <see cref="IoResultState.Success"/>.
		/// Sets <see cref="Result"/> to <see cref="IoResultState.Faulted"/>,
		/// and sets an optional <see cref="Error"/>.
		/// If the <paramref name="exception"/> is null,
		/// any currently-set error will remain.
		/// </summary>
		/// <param name="exception">NOTICE: can be null.</param>
		/// <returns>This instance for chaining.</returns>
		public IoResult Fault(Exception exception)
			=> setResult(IoResultState.Faulted, exception);

		/// <summary>
		/// Note: this method only changes the state if this is currently
		/// <see cref="IoResultState.Success"/>.
		/// Sets <see cref="Result"/> to <see cref="IoResultState.BadData"/>,
		/// and sets an optional <see cref="Error"/>.
		/// If the <paramref name="exception"/> is null,
		/// any currently-set error will remain.
		/// </summary>
		/// <param name="exception">NOTICE: can be null.</param>
		/// <returns>This instance for chaining.</returns>
		public IoResult BadData(Exception exception = null)
			=> setResult(IoResultState.BadData, exception);

		/// <summary>
		/// Note: this method only changes the state if this is currently
		/// <see cref="IoResultState.Success"/>.
		/// Sets <see cref="Result"/> to <see cref="IoResultState.Cancelled"/>,
		/// and sets an optional <see cref="Error"/>.
		/// If the <paramref name="exception"/> is null,
		/// any currently-set error will remain.
		/// </summary>
		/// <param name="exception">NOTICE: can be null.</param>
		/// <returns>This instance for chaining.</returns>
		public IoResult Cancel(Exception exception = null)
			=> setResult(IoResultState.Cancelled, exception);

		/// <summary>
		/// Note: this method only changes the state if this is currently
		/// <see cref="IoResultState.Success"/>. This method will
		/// set this <see cref="Result"/> and <see cref="Error"/>
		/// from the given <paramref name="other"/> result; yet,
		/// if the <paramref name="other"/> <see cref="Error"/> is null,
		/// any currently-set error here will remain.
		/// </summary>
		/// <param name="other">Not null.</param>
		/// <returns>This instance for chaining.</returns>
		public IoResult SetResultFrom(IoResult other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));
			return setResult(other.Result, other.Error);
		}


		/// <summary>
		/// Provided on construction, or else is <c>CancellationToken.None</c>.
		/// </summary>
		public CancellationToken CancellationToken { get; }

		/// <summary>
		/// This defaults to <see cref="IoResultState.Success"/>.
		/// This is set when the Task is complete.
		/// </summary>
		public IoResultState Result
		{
			get {
				lock (SyncLock) {
					return result;
				}
			}
		}

		/// <summary>
		/// If this was a read operation, this holds the count of bytes read.
		/// If this was a write, this
		/// this holds the count of bytes written.
		/// If multiple operations are invoked, the value is accumulated.
		/// </summary>
		public long BytesReadOrWritten
		{
			get {
				lock (SyncLock) {
					return bytesReadOrWritten;
				}
			}
		}

		/// <summary>
		/// Defaults to null; and will be set if the Task faulted.
		/// Set this by invoking <see cref="Fault"/>
		/// or <see cref="setResult"/>.
		/// </summary>
		public Exception Error
		{
			get {
				lock (SyncLock) {
					return error;
				}
			}
		}


		/// <summary>
		/// This property returns <see cref="Result"/> == <see cref="IoResultState.Success"/>.
		/// </summary>
		public bool IsSuccess
			=> Result == IoResultState.Success;

		/// <summary>
		/// This property returns <see cref="Result"/> == <see cref="IoResultState.BadData"/>.
		/// </summary>
		public bool IsBadData
			=> Result == IoResultState.BadData;

		/// <summary>
		/// This property returns <see cref="Result"/> == <see cref="IoResultState.Cancelled"/>;
		/// OR, if the <see cref="CancellationToken"/> is cancelled.
		/// </summary>
		public bool IsCancelled
			=> CancellationToken.IsCancellationRequested || (Result == IoResultState.Cancelled);

		/// <summary>
		/// This property returns <see cref="Result"/> == <see cref="IoResultState.Faulted"/>.
		/// </summary>
		public bool IsFaulted
			=> Result == IoResultState.Faulted;


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}"
					+ "["
					+ $"{Result}"
					+ $", {nameof(IoResult.BytesReadOrWritten)}: {BytesReadOrWritten}"
					+ $"{(Error != null ? $", {Error.Message}" : string.Empty)}"
					+ "]";
	}


	/// <summary>
	/// Extends <see cref="IoResult"/> and adds a <see cref="State"/> that can be used by your
	/// implementation. 
	/// </summary>
	/// <typeparam name="TState">This is the type of the state returned by your method
	/// implementing the read or write.</typeparam>
	public class IoResult<TState>
			: IoResult
	{
		private TState state;


		/// <summary>
		/// Default constructor. This sets <see cref="CancellationToken"/> to
		/// <see cref="CancellationToken.None"/>.
		/// </summary>
		public IoResult() { }

		/// <summary>
		/// Constructor: sets the <see cref="State"/>; and optionally the <see cref="CancellationToken"/>.
		/// </summary>
		/// <param name="state">Optional.</param>
		/// <param name="cancellationToken">Optional.</param>
		public IoResult(TState state, CancellationToken cancellationToken = default)
				: base(cancellationToken)
			=> State = state;


		/// <summary>
		/// Arbitrary state set by the method implementing the read or write.
		/// </summary>
		public TState State
		{
			get {
				lock (SyncLock) {
					return state;
				}
			}
			set {
				lock (SyncLock) {
					state = value;
				}
			}
		}

		/// <summary>
		/// A convenience method that sets <see cref="State"/> to this
		/// <paramref name="value"/>; and then returns this instance
		/// for chaining.
		/// </summary>
		/// <param name="value">The new <see cref="State"/>: not tested here.</param>
		/// <returns>This instance for chaining.</returns>
		public IoResult<TState> SetState(TState value)
		{
			lock (SyncLock) {
				state = value;
				return this;
			}
		}


		public override string ToString()
			=> $"{base.ToString()}"
					+ "["
					+ $", {nameof(IoResult<TState>.State)}: {State}"
					+ "]";
	}
}
