using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to bind the fill value of a UI image to a BoundProperty in code.
	/// </summary>
	public class BoundIntFillPercentField : BaseBoundFillPercentField<int>
	{
		protected override void RecalculateFill(Image image, int full, int current)
		{
			image.fillAmount = (float)current / full;
		}
	}
}
