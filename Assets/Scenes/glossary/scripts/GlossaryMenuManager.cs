using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Core.Weapons;
using FiringSquad.Gameplay.Weapons;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIButton = UnityEngine.UI.Button;
using UIText = UnityEngine.UI.Text;
using UILayoutGroup = UnityEngine.UI.LayoutGroup;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Gun Glossary UI manager.
	/// </summary>
	public class GlossaryMenuManager : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameObject mMainElementHolder;
		[SerializeField] private UIButton mReturnToMainButton;
		[SerializeField] private UIText mDescriptionText;
		[SerializeField] private UILayoutGroup mSubMenuGroup;
		[SerializeField] private GameObject mButtonPrefab;

		[SerializeField] private UIButton mMechanismButton;
		[SerializeField] private UIButton mSightsButton;
		[SerializeField] private UIButton mBarrelButton;
		[SerializeField] private UIButton mUnderbarrelButton;

		[SerializeField] private MenuDemoWeaponScript mWeapon;

		private Attachment mCurrentMode;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mReturnToMainButton.onClick.AddListener(ReturnToMenu);

			mMechanismButton.onClick.AddListener(() => HandleClickedCategory(Attachment.Mechanism));
			mSightsButton.onClick.AddListener(() => HandleClickedCategory(Attachment.Scope));
			mBarrelButton.onClick.AddListener(() => HandleClickedCategory(Attachment.Barrel));
			mUnderbarrelButton.onClick.AddListener(() => HandleClickedCategory(Attachment.Grip));
		}

		/// <summary>
		/// Return to the main menu
		/// </summary>
		private void ReturnToMenu()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}

		private void HandleClickedCategory(Attachment category)
		{
			if (mCurrentMode == category)
			{
				mCurrentMode = (Attachment)(1 << 5);
				mSubMenuGroup.transform.parent.gameObject.SetActive(false);
			}
			else
			{
				mCurrentMode = category;

				// Someday, we could add an animation.
				mSubMenuGroup.transform.parent.gameObject.SetActive(true);
				PopulateList(category);
			}
		}

		/// <summary>
		/// Populates the list with weapon part buttons
		/// </summary>
		private void PopulateList(Attachment slot)
		{
			// Clean the previous list
			foreach (Transform t in mSubMenuGroup.transform)
				Destroy(t.gameObject);

			var parts = ServiceLocator.Get<IWeaponPartManager>().GetAllPrefabScripts(false).Where(x => x.Value.attachPoint == slot);
			foreach (var part in parts)
			{
				UIButton newButton = Instantiate(mButtonPrefab, mSubMenuGroup.transform).GetComponent<UIButton>();
				UIText text = newButton.GetComponentInChildren<UIText>();
				text.text = part.Value.prettyName;

				byte id = part.Value.partId;
				newButton.onClick.AddListener(() => ChangePart(id));
			}
		}

		/// <summary>
		/// Changes the current attached part to the model gun
		/// </summary>
		private void ChangePart(byte partId)
		{
			WeaponPartScript part = ServiceLocator.Get<IWeaponPartManager>().GetPrefabScript(partId);
			mDescriptionText.text = part.description;
			mWeapon.AttachNewPart(partId);
			//mPartShowcase.GetComponent<BaseWeaponScript>().AttachNewPart(item.GetComponent<WeaponPartScript>().partId);
		}
	}
}
