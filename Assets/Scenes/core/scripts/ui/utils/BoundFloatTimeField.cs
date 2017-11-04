using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	public class BoundFloatTimeField : BoundUIElement<float>
	{
		private Text mTextElement;

		protected override void Awake()
		{
			base.Awake();
			mTextElement = GetComponent<Text>();
		}

		protected override void HandlePropertyChanged()
		{
			float timer = property.value;
			string minSec = string.Format("{0}:{1:00}", (int)timer / 60, (int)timer % 60);
			mTextElement.text = minSec;
		}
	}
}
