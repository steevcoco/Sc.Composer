using System.ComponentModel.DataAnnotations;
using Sc.Util.Resources;


namespace Sc.Util.Rendering
{
	/// <summary>
	/// Defines alpha levels.
	/// Values are actual byte values in [0,255].
	/// </summary>
	public enum AlphaLevel
			: byte
	{
		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_Opaque_Name))]
		Opaque = 255,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_NearOpaque_Name))]
		NearOpaque = 243,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_VeryVeryHigh_Name))]
		VeryVeryHigh = 231,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_VeryHigh_Name))]
		VeryHigh = 219,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_High_Name))]
		High = 205,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_MediumHigh_Name))]
		MediumHigh = 185,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_MediumMediumHigh_Name))]
		MediumMediumHigh = 160,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_Medium_Name))]
		Medium = 127,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_MediumMediumLow_Name))]
		MediumMediumLow = 95,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_MediumLow_Name))]
		MediumLow = 70,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_Low_Name))]
		Low = 50,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_VeryLow_Name))]
		VeryLow = 36,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_VeryVeryLow_Name))]
		VeryVeryLow = 24,

		[Display(
				ResourceType = typeof(ScUtilResources),
				Name = nameof(ScUtilResources.AlphaLevel_Hint_Name))]
		Hint = 12,
	}
}
