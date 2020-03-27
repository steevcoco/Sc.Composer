namespace Sc.Abstractions.Math
{
	/// <summary>
	/// Enumerates methods by which signed left and right hand values can be compared.
	/// </summary>
	public enum SignedComparison
			: byte
	{
		/// <summary>
		/// Indicates that the more positive value is considered larger.
		/// </summary>
		MorePositive,

		/// <summary>
		/// Indicates that the absolute value of each value is used in the comparison.
		/// </summary>
		AbsoluteMagnitude,

		/// <summary>
		/// Indicates that the more negative value is considered larger.
		/// </summary>
		MoreNegative,

		/// <summary>
		/// Indicates that if the left-hand argument is negative, then the comparison is as with
		/// <see cref="MoreNegative"/>. If the left-hand argument is positive, then the comparison
		/// is as with <see cref="MorePositive"/>.
		/// </summary>
		RespectLeftSign,

		/// <summary>
		/// Indicates that if the right-hand argument is negative, then the comparison is as with
		/// <see cref="MoreNegative"/>. If the right-hand argument is positive, then the comparison
		/// is as with <see cref="MorePositive"/>.
		/// </summary>
		RespectRightSign,
	}
}
