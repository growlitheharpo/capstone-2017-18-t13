using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Weapons;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
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
		[SerializeField] private UIText mDescriptionText;
		[SerializeField] private UILayoutGroup mSubMenuGroup;
		[SerializeField] private Animator mSubMenuAnimator;
		[SerializeField] private GameObject mButtonPrefab;

		[SerializeField] private UIButton mMechanismButton;
		[SerializeField] private UIButton mSightsButton;
		[SerializeField] private UIButton mBarrelButton;
		[SerializeField] private UIButton mUnderbarrelButton;

		[SerializeField] private MenuDemoWeaponScript mWeapon;

		private string mDefaultDescription;
		private Attachment mCurrentMode;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mMechanismButton.onClick.AddListener(() => HandleClickedCategory(Attachment.Mechanism));
			mSightsButton.onClick.AddListener(() => HandleClickedCategory(Attachment.Scope));
			mBarrelButton.onClick.AddListener(() => HandleClickedCategory(Attachment.Barrel));
			mUnderbarrelButton.onClick.AddListener(() => HandleClickedCategory(Attachment.Grip));

			mDefaultDescription = mDescriptionText.text;
			ResetToDefault();
		}

		/// <summary>
		/// Reset the glossary to its default state.
		/// </summary>
		public void ResetToDefault(bool resetWeapon = true)
		{
			mSubMenuAnimator.SetBool("Enabled", false);
			mCurrentMode = (Attachment)(1 << 5); // we do this because there isn't an Attachment.None
			mDescriptionText.text = mDefaultDescription;


			if (resetWeapon)
			{
				mWeapon.ResetToDefaultParts();
				mWeapon.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
			}
		}

		/// <summary>
		/// Handle the player clicking on a specific category.
		/// </summary>
		/// <param name="category"></param>
		private void HandleClickedCategory(Attachment category)
		{
			if (mCurrentMode == category)
				ResetToDefault(false);
			else
			{
				mCurrentMode = category;

				// Someday, we could add an animation.
				mSubMenuAnimator.SetBool("Enabled", true);
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
		}
	}
}
