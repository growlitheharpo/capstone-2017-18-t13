using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	/// <inheritdoc />
	public class BoundIntField : BoundUIElement<int>
	{
		/// Inspector variables
		[SerializeField] private string mDisplayFormat;

		/// Private variables
		private UIText mTextElement;

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();
			mTextElement = GetComponent<UIText>();
		}

		/// <inheritdoc />
		protected override void HandlePropertyChanged()
		{
			mTextElement.text = property.value.ToString(mDisplayFormat);
		}
	}
}
