using System;
using System.Runtime.Serialization;


namespace Sc.Util.System
{
	/// <summary>
	/// Implements a time stamp as an <see cref="Expiration"/> value,
	/// which is a moment in time.
	/// The Expiration value is an Iso Date String: it is converted with
	/// <c>DateTime.ToString("O")</c>.
	/// NOTICE that this class also defines the <see cref="CreatedAt"/>
	/// time stamp; BUT this implements <see cref="IEquatable{T}"/>,
	/// and ONLY the <see cref="Expiration"/> is compared. Also implements
	/// <see cref="IComparable{T}"/>, again comparing the <see cref="Expiration"/>.
	/// </summary>
	[DataContract]
	public class TtlTimeStamp
			: IEquatable<TtlTimeStamp>,
					IComparable<TtlTimeStamp>
	{
		public static bool operator ==(TtlTimeStamp a, TtlTimeStamp b)
			=> a == null
					? b == null
					: a.Equals(b);

		public static bool operator !=(TtlTimeStamp a, TtlTimeStamp b)
			=> !(a == b);

		public static bool operator >(TtlTimeStamp a, TtlTimeStamp b)
			=> (a?.GetExpirationUtc() ?? DateTime.MinValue)
					.CompareTo(b?.GetExpirationUtc() ?? DateTime.MinValue)
					> 0;

		public static bool operator <(TtlTimeStamp a, TtlTimeStamp b)
			=> (a?.GetExpirationUtc() ?? DateTime.MinValue)
					.CompareTo(b?.GetExpirationUtc() ?? DateTime.MinValue)
					< 0;

		public static bool operator >=(TtlTimeStamp a, TtlTimeStamp b)
			=> (a?.GetExpirationUtc() ?? DateTime.MinValue)
					.CompareTo(b?.GetExpirationUtc() ?? DateTime.MinValue)
					>= 0;

		public static bool operator <=(TtlTimeStamp a, TtlTimeStamp b)
			=> (a?.GetExpirationUtc() ?? DateTime.MinValue)
					.CompareTo(b?.GetExpirationUtc() ?? DateTime.MinValue)
					<= 0;


		/// <summary>
		/// Creates an EXPIRED time stamp: both <see cref="Expiration"/> and
		/// <see cref="CreatedAt"/> are set to <see cref="DateTime.UtcNow"/>.
		/// </summary>
		public TtlTimeStamp()
		{
			CreatedAt = DateTime.UtcNow.ToString("O");
			Expiration = CreatedAt;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="timeoutMilliseconds">Specifies a milliseconds timeout from now
		/// for the <see cref="Expiration"/>.
		/// NOTE that this argument may be negative.</param>
		public TtlTimeStamp(long timeoutMilliseconds)
		{
			DateTime now = DateTime.UtcNow;
			CreatedAt = now.ToString("O");
			Expiration
					= new DateTime(
									now.Ticks + (timeoutMilliseconds * TimeSpan.TicksPerMillisecond),
									DateTimeKind.Utc)
							.ToString("O");
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="timeout">Specifies a timeout from now for the <see cref="Expiration"/>.
		/// NOTE that this argument may be negative.</param>
		public TtlTimeStamp(TimeSpan timeout)
		{
			DateTime now = DateTime.UtcNow;
			CreatedAt = now.ToString("O");
			Expiration
					= new DateTime(now.Ticks + timeout.Ticks, DateTimeKind.Utc)
							.ToString("O");
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="expiration">Explicitly sets the expiration.</param>
		public TtlTimeStamp(DateTime expiration)
		{
			CreatedAt = DateTime.UtcNow.ToString("O");
			Expiration = expiration.ToUniversalTime()
					.ToString("O");
		}


		/// <summary>
		/// The creation time stamp as a Utc Iso Date String:
		/// converted with <c>DateTime.ToString("O")</c>.
		/// </summary>
		[DataMember]
		public string CreatedAt { get; private set; }

		/// <summary>
		/// The expiration moment a a Utc Iso Date String
		/// converted with <c>DateTime.ToString("O")</c>.
		/// </summary>
		[DataMember]
		public string Expiration { get; private set; }

		/// <summary>
		/// This returns a new Date object from <see cref="CreatedAt"/>, in Utc.
		/// </summary>
		/// <returns>Not null.</returns>
		public DateTime GetCreatedAtUtc()
			=> DateTime.Parse(CreatedAt);

		/// <summary>
		/// This returns a new Date object from <see cref="Expiration"/>, in Utc.
		/// </summary>
		/// <returns>Not null.</returns>
		public DateTime GetExpirationUtc()
			=> DateTime.Parse(Expiration);

		/// <summary>
		/// Computes the difference between the <see cref="Expiration"/> and
		/// <see cref="DateTime.UtcNow"/>, and returns true if this is now expired.
		/// </summary>
		public bool IsExpired
			=> GetExpirationUtc() <= DateTime.UtcNow;

		/// <summary>
		/// This returns the <see cref="TimeSpan"/> until <see cref="Expiration"/>
		/// will expire, from <see cref="DateTime.UtcNow"/>: which MAY be
		/// negative if expired.
		/// </summary>
		/// <returns>May be negative.</returns>
		public TimeSpan TimeRemaining
			=> GetExpirationUtc() - DateTime.UtcNow;


		public override int GetHashCode()
			=> HashCodeHelper.Seed.Hash(
					GetExpirationUtc()
							.Ticks);

		public override bool Equals(object obj)
			=> Equals(obj as TtlTimeStamp);

		public bool Equals(TtlTimeStamp other)
			=> (other != null)
					&& (GetExpirationUtc() == other.GetExpirationUtc());

		public int CompareTo(TtlTimeStamp other)
			=> GetExpirationUtc()
					.CompareTo(other?.GetCreatedAtUtc() ?? DateTime.MinValue);

		public override string ToString()
			=> $"{GetType().GetFriendlyName()}"
					+ "["
					+ $"{nameof(TtlTimeStamp.IsExpired)}={IsExpired}]"
					+ $", {nameof(TtlTimeStamp.Expiration)}={Expiration}"
					+ $", {nameof(TtlTimeStamp.CreatedAt)}={CreatedAt}"
					+ "]";
	}
}
