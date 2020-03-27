using Sc.Util.System;


namespace Sc.Util.Rendering
{
	/// <summary>
	/// Static helpers for <see cref="Rendering"/> objects.
	/// </summary>
	public static class ColorTransformHelper
	{
		private static readonly LazyWeakReference<EnumValueRange<AlphaLevel>.Set> alphaLevels
				= new LazyWeakReference<EnumValueRange<AlphaLevel>.Set>(
						EnumValueRange<AlphaLevel>.Get);


		/// <summary>
		/// Static helper method converts this <paramref name="alphaLevel"/>
		/// to a <see cref="double"/> opacity value, in <c>[0.0, 1.0]</c>.
		/// </summary>
		/// <param name="alphaLevel">This level to convert.</param>
		/// <returns>Opacity value, in <c>[0.0, 1.0].</returns>
		public static double Opacity(this AlphaLevel alphaLevel)
			=> (double)alphaLevel / 255D;

		/// <summary>
		/// Static helper method converts this <paramref name="opacity"/>
		/// to an <see cref="AlphaLevel"/> Enum member value.
		/// </summary>
		/// <param name="opacity">This opacity value to convert to an
		/// Enum member value, in <c>[0.0, 1.0].</param>
		/// <param name="returnStrictEnumMembers">Defaults to true: the returned
		/// value is the nearest actual Enum constant member defined on
		/// <see cref="AlphaLevel"/>. If set false, then the return value is
		/// a <see langword="byte"/> value in <c>[0, 255]</c>, converted directly
		/// from the opacity; and may NOT correspond with a defined enum value.</param>
		/// <returns><see cref="AlphaLevel"/> value; in <c>[0, 255]</c>.</returns>
		public static AlphaLevel ToAlphaLevel(this double opacity, bool returnStrictEnumMembers = true)
		{
			return returnStrictEnumMembers
					? ColorTransformHelper.alphaLevels.Get()
							.Get(ToDoubleValue(opacity))
									.Value
					: (AlphaLevel)(byte)ToDoubleValue(opacity);
			static double ToDoubleValue(double value)
				=> global::System.Math.Round(
						global::System.Math.Max(0D, global::System.Math.Min(1D, value))
								* 255D);
		}
	}
}
