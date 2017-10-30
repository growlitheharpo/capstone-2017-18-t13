using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public class BoundStringField : BoundUIElement<string>
	{
		private UIText mTextElement;

		protected override void Awake()
		{
			base.Awake();
			mTextElement = GetComponent<UIText>();
		}

		protected override void HandlePropertyChanged()
		{
			mTextElement.text = property.value;
		}
	}
}
