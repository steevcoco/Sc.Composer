using SystemMath = System.Math;


namespace Sc.Util.Rendering
{
	/// <summary>
	/// Defines Color transform settings.
	/// </summary>
	public sealed class ColorTransformSettings
	{
		/// <summary>
		/// Returns a brightness level for a Color based on a constant identifier.
		/// Notice: the returned value is for values in [0, 1], but is not bound here;
		/// and may be lower or higher.
		/// </summary>
		public double Brightness(BrightnessLevel brightness)
			=> BrightnessScale * ((double)brightness / 255D);

		/// <summary>
		/// Returns an alpha level for a Color based on a constant identifier.
		/// The return value is bound to [0, 255].
		/// </summary>
		public int Alpha(AlphaLevel alpha)
			=> SystemMath.Max(0, SystemMath.Min(255, (int)(AlphaScale * (double)alpha)));


		/// <summary>
		/// The user-given preset name for this ColorPalette. Defaults to null.
		/// This property is provided for serialization, and user editing.
		/// </summary>
		public string PresetName { get; set; }

		/// <summary>
		/// The Color transform mode for this ColorPalette. Defaults to HSL.
		/// Colors should be transformed with this value.
		/// </summary>
		public ColorTransformMode ColorTransformMode { get; set; } = ColorTransformMode.Hsl;

		/// <summary>
		/// A Brightness scaler for this ColorPalette. Defaults to 1.
		/// Brightness constants should be transformed with this value via the methods here.
		/// </summary>
		public double BrightnessScale { get; set; } = 1D;

		/// <summary>
		/// An Alpha scaler for this ColorPalette. Defaults to 1.
		/// Alpha constants should be transformed with this value via the methods here.
		/// </summary>
		public double AlphaScale { get; set; } = 1D;


		/// <summary>
		/// Returns the argument multiplied by the <see cref="BrightnessScale"/>.
		/// </summary>
		/// <param name="brightnessTransform"></param>
		/// <returns></returns>
		public double GetBrightnessTransformWithScaler(double brightnessTransform)
			=> BrightnessScale * brightnessTransform;

		/// <summary>
		/// Returns the argument multiplied by the <see cref="AlphaScale"/>
		/// and bound to [0, 255].
		/// </summary>
		/// <param name="alpha"></param>
		/// <returns></returns>
		public int TransformAlpha(int alpha)
			=> SystemMath.Max(0, SystemMath.Min(255, (int)(AlphaScale * alpha)));
	}
}
