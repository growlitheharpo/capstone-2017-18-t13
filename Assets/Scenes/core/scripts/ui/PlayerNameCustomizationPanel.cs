using FiringSquad.Core;
using FiringSquad.Core.Input;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	public class PlayerNameCustomizationPanel : MonoBehaviour
	{
		[SerializeField] private InputField mInputField;
		[SerializeField] private ActionProvider mConfirmButton;

		private CltPlayer mPlayerRef;

		private void Awake()
		{
			mConfirmButton.OnClick += ConfirmName;
			EventManager.LocalGUI.OnRequestNameChange += OnRequestNameChange;

			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			mConfirmButton.OnClick -= ConfirmName;
			EventManager.LocalGUI.OnRequestNameChange -= OnRequestNameChange;
		}

		private void OnRequestNameChange(CltPlayer obj)
		{
			mPlayerRef = obj;
			gameObject.SetActive(true);

			ServiceLocator.Get<IInput>()
				.DisableInputLevel(InputLevel.Gameplay)
				.DisableInputLevel(InputLevel.HideCursor);
		}

		private void ConfirmName()
		{
			ServiceLocator.Get<IInput>()
				.EnableInputLevel(InputLevel.Gameplay)
				.EnableInputLevel(InputLevel.HideCursor);

			mPlayerRef.SetPlayerName(mInputField.text);
			Destroy(gameObject);
		}
	}
}
