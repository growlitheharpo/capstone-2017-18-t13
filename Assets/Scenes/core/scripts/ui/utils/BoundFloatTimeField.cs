using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <inheritdoc />
	public class BoundFloatTimeField : BoundUIElement<float>
	{
		/// Private variables
		private Text mTextElement;

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();
			mTextElement = GetComponent<Text>();
		}

		/// <inheritdoc />
		protected override void HandlePropertyChanged()
		{
			float timer = property.value;
			string minSec = string.Format("{0}:{1:00}", (int)timer / 60, (int)timer % 60);
			mTextElement.text = minSec;
		}
	}
}
