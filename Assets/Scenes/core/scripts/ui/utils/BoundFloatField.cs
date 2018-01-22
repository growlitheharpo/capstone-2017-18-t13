using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	/// <inheritdoc />
	public class BoundFloatField : BoundUIElement<float>
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
