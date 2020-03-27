using System.ComponentModel.DataAnnotations;
using Sc.Util.Resources;


namespace Sc.Util.Rendering
{
	/// <summary>
	/// Defines brightness levels.
	/// Values are actual byte values in [0,255].
	/// </summary>
	public enum BrightnessLevel
			: byte
	{
		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.BrightnessLevel_Bright_Name))]
		Bright = 255,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.BrightnessLevel_MediumBright_Name))]
		MediumBright = 210,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.BrightnessLevel_Medium_Name))]
		Medium = 142,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.BrightnessLevel_Dim_Name))]
		Dim = 98,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.BrightnessLevel_VeryDim_Name))]
		VeryDim = 50
	}
}
