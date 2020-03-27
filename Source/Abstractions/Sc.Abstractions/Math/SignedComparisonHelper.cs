using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Sc.Abstractions.Math
{
	/// <summary>
	/// Static extension methods for <see cref="SignedComparison"/>.
	/// </summary>
	public static class SignedComparisonHelper
	{
		/// <summary>
		/// This value can serve as a default tolerance for many operations. This is a value
		/// of .5 at 16 decimal places: <c>0.0000000000000005</c>.
		/// </summary>
		public const double DefaultTolerance = 0.0000000000000005D;


		/// <summary>
		/// <see cref="IEqualityComparer{T}"/> for <c>double</c>, using a tolerance.
		/// </summary>
		private sealed class ToleranceEqualityComparer
				: IEqualityComparer<double>
		{
			private readonly double tolerance;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="tolerance">Required.</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ToleranceEqualityComparer(double tolerance)
				=> this.tolerance = tolerance;


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Equals(double x, double y)
				=> x.AreEqual(y, tolerance);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public int GetHashCode(double obj)
				=> obj.GetHashCode();
		}


		/// <summary>
		/// Compares a left and right double based on this <see cref="SignedComparison"/> mode.
		/// The method first tests if the values are equal within the given <c>tolerance</c>
		/// --- and will return zero if so.
		/// </summary>
		/// <param name="comparison">This comparison mode.</param>
		/// <param name="left">The left-hand value to compare.</param>
		/// <param name="right">The right-hand value to compare.</param>
		/// <param name="tolerance">The tolerance used for equality. Defaults to
		/// <see cref="DefaultTolerance"/>.</param>
		/// <returns>The comparison of the left-hand argument to the right according to this
		/// mode; within the <c>tolerance</c>. Zero if equal, -1 if <paramref name="left"/>
		/// is less than <paramref name="right"/>; and 1 otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CompareWith(
				this SignedComparison comparison,
				double left,
				double right,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
			=> left.CompareWith(right, comparison, tolerance);

		/// <summary>
		/// Compares a left and right double based on the given <see cref="SignedComparison"/> mode.
		/// The method first tests if the values are equal within the given <c>tolerance</c>
		/// --- and will return zero if so.
		/// </summary>
		/// <param name="left">The left-hand value to compare.</param>
		/// <param name="right">The right-hand value to compare.</param>
		/// <param name="comparison">The comparison mode. Defaults to
		/// <see cref="SignedComparison.MorePositive"/>.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>The comparison of the left-hand argument to the right according to the given
		/// mode; within the <c>tolerance</c>. Zero if equal, -1 if <paramref name="left"/>
		/// is less than <paramref name="right"/>; and 1 otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CompareWith(
				this double left,
				double right,
				SignedComparison comparison = SignedComparison.MorePositive,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
		{
			if (left.AreEqual(right, tolerance))
				return 0;
			switch (comparison) {
				case SignedComparison.MorePositive :
					return left.CompareTo(right);
				case SignedComparison.MoreNegative :
					return -left.CompareTo(right);
				case SignedComparison.AbsoluteMagnitude :
					return global::System.Math.Abs(left)
							.CompareTo(global::System.Math.Abs(right));
				case SignedComparison.RespectLeftSign :
					if (global::System.Math.Sign(left) < 0)
						return -left.CompareTo(right);
					return left.CompareTo(right);
				case SignedComparison.RespectRightSign :
					if (global::System.Math.Sign(right) < 0)
						return -left.CompareTo(right);
					return left.CompareTo(right);
				default :
					throw new NotImplementedException($"Unknown {nameof(SignedComparison)} {comparison}.");
			}
		}

		/// <summary>
		/// Compares this value with the other, within the given <c>tolerance</c>; which
		/// defaults to <see cref="DefaultTolerance"/>.
		/// </summary>
		/// <param name="a">Value.</param>
		/// <param name="b">Value.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>True if within the <c>tolerance</c>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreEqual(
				this double a,
				double b,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
		{
			if (double.IsInfinity(a))
					// ReSharper disable once CompareOfFloatsByEqualityOperator
				return a == b;
			return !double.IsInfinity(b) && (global::System.Math.Abs(a - b) < tolerance);
		}


		/// <summary>
		/// Compares this value against <c>0D</c>, within the given <c>tolerance</c>; which
		/// defaults to <see cref="DefaultTolerance"/>.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>True if within the <c>tolerance</c> from <c>0D</c>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsZero(this double value, double tolerance = SignedComparisonHelper.DefaultTolerance)
			=> global::System.Math.Abs(value) < tolerance;

		/// <summary>
		/// Compares this value against <c>0D</c>, within the given <c>tolerance</c>; which
		/// defaults to <see cref="DefaultTolerance"/>.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>True if greater than or equal to the <c>tolerance</c>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPositive(this double value, double tolerance = SignedComparisonHelper.DefaultTolerance)
			=> value >= tolerance;


		/// <summary>
		/// Compares this value against <c>0D</c>, within the given <c>tolerance</c>; which
		/// defaults to <see cref="DefaultTolerance"/>; and returns zero if zero, and
		/// otherwise one if positive, and -1 if negative.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>Zero if zero, one if positive, and -1 if negative.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetSign(this double value, double tolerance = SignedComparisonHelper.DefaultTolerance)
			=> value.IsZero(tolerance)
					? 0
					: global::System.Math.Sign(value);


		/// <summary>
		/// Compares that the two Collections' <c>Counts</c> are equal, and that for each element in a,
		/// the number of Equal elements in a is equal to the number in b; and vice-versa. NOTICE
		/// that this method allows null arguments. The comparison uses the given <c>tolerance</c>; which
		/// defaults to <see cref="DefaultTolerance"/>.
		/// </summary>
		/// <param name="a">Can be null.</param>
		/// <param name="b">Can be null.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>True if both Collections have the same number of equal elements.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SetEqual(
				this IReadOnlyCollection<double> a,
				IReadOnlyCollection<double> b,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
			=> a == null
				? b == null
					: a.Count != b?.Count
					? false
						: a.All(
									aElement => a.Count(item => item.AreEqual(aElement, tolerance))
											== b.Count(item => item.AreEqual(aElement, tolerance)))
							&& b.All(
									bElement => b.Count(item => item.AreEqual(bElement, tolerance))
											== a.Count(item => item.AreEqual(bElement, tolerance)));

		/// <summary>
		/// Compares that the two Collections are sequence equal, within the given <c>tolerance</c>; which
		/// defaults to <see cref="DefaultTolerance"/>.
		/// </summary>
		/// <param name="a">Can be null.</param>
		/// <param name="b">Can be null.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>True if both Collections have the same number of equal elements.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SequenceEqual(
				this IReadOnlyCollection<double> a,
				IReadOnlyCollection<double> b,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
			=> a == null
				? b == null
				: (b != null)
						&& a.SequenceEqual(b, new ToleranceEqualityComparer(tolerance));


		/// <summary>
		/// Finds the larger value, within the <paramref name="tolerance"/>.
		/// Returns <paramref name="x"/> if the values compare equal.
		/// </summary>
		/// <param name="comparison">The comparison mode.</param>
		/// <param name="x">The first value to compare.</param>
		/// <param name="y">The second value to compare.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>Returns <paramref name="x"/> if the values compare equal.
		/// Otherwise returns the larger value.</returns>
		public static double GetMax(
				this SignedComparison comparison,
				double x,
				double y,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
			=> x.GetMax(y, comparison, tolerance);

		/// <summary>
		/// Finds the larger value, within the <paramref name="tolerance"/>.
		/// Returns <paramref name="x"/> if the values compare equal.
		/// </summary>
		/// <param name="x">This first value to compare.</param>
		/// <param name="y">The second value to compare.</param>
		/// <param name="comparison">The comparison mode. Defaults to
		/// <see cref="SignedComparison.MorePositive"/>.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>Returns <paramref name="x"/> if the values compare equal.
		/// Otherwise returns the larger value.</returns>
		public static double GetMax(
				this double x,
				double y,
				SignedComparison comparison = SignedComparison.MorePositive,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
		{
			if (x.AreEqual(y, tolerance))
				return x;
			switch (comparison) {
				case SignedComparison.MorePositive:
					return global::System.Math.Max(x, y);
				case SignedComparison.MoreNegative:
					return global::System.Math.Min(x, y);
				case SignedComparison.AbsoluteMagnitude:
					return global::System.Math.Max(
							global::System.Math.Abs(x),
							global::System.Math.Abs(y));
				case SignedComparison.RespectLeftSign:
					if (global::System.Math.Sign(x) < 0)
						return global::System.Math.Min(x, y);
					return global::System.Math.Max(x, y);
				case SignedComparison.RespectRightSign:
					if (global::System.Math.Sign(y) < 0)
						return global::System.Math.Min(x, y);
					return global::System.Math.Max(x, y);
				default:
					throw new NotImplementedException($"Unknown {nameof(SignedComparison)} {comparison}.");
			}
		}

		/// <summary>
		/// Finds the smaller value, within the <paramref name="tolerance"/>.
		/// Returns <paramref name="x"/> if the values compare equal.
		/// </summary>
		/// <param name="comparison">The comparison mode.</param>
		/// <param name="x">The first value to compare.</param>
		/// <param name="y">The second value to compare.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>Returns <paramref name="x"/> if the values compare equal.
		/// Otherwise returns the smaller value.</returns>
		public static double GetMin(
				this SignedComparison comparison,
				double x,
				double y,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
			=> x.GetMin(y, comparison, tolerance);

		/// <summary>
		/// Finds the smaller value, within the <paramref name="tolerance"/>.
		/// Returns <paramref name="x"/> if the values compare equal.
		/// </summary>
		/// <param name="x">This first value to compare.</param>
		/// <param name="y">The second value to compare.</param>
		/// <param name="comparison">The comparison mode. Defaults to
		/// <see cref="SignedComparison.MorePositive"/>.</param>
		/// <param name="tolerance">The tolerance. Defaults to <see cref="DefaultTolerance"/>.</param>
		/// <returns>Returns <paramref name="x"/> if the values compare equal.
		/// Otherwise returns the smaller value.</returns>
		public static double GetMin(
				this double x,
				double y,
				SignedComparison comparison = SignedComparison.MorePositive,
				double tolerance = SignedComparisonHelper.DefaultTolerance)
		{
			if (x.AreEqual(y, tolerance))
				return x;
			switch (comparison) {
				case SignedComparison.MorePositive:
					return global::System.Math.Min(x, y);
				case SignedComparison.MoreNegative:
					return global::System.Math.Max(x, y);
				case SignedComparison.AbsoluteMagnitude:
					return global::System.Math.Min(
							global::System.Math.Abs(x),
							global::System.Math.Abs(y));
				case SignedComparison.RespectLeftSign:
					if (global::System.Math.Sign(x) < 0)
						return global::System.Math.Max(x, y);
					return global::System.Math.Min(x, y);
				case SignedComparison.RespectRightSign:
					if (global::System.Math.Sign(y) < 0)
						return global::System.Math.Max(x, y);
					return global::System.Math.Min(x, y);
				default:
					throw new NotImplementedException($"Unknown {nameof(SignedComparison)} {comparison}.");
			}
		}
	}
}
