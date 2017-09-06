using UnityEngine;
using Input = KeatsLib.Unity.Input;

namespace Prototype2
{
	public class UIManager : MonoBehaviour
	{
		[SerializeField] private ActionProvider mBarrel01Button;
		[SerializeField] private ActionProvider mBarrel02Button;
		[SerializeField] private ActionProvider mScope01Button;
		[SerializeField] private ActionProvider mScope02Button;
		[SerializeField] private GameObject mBarrel01;
		[SerializeField] private GameObject mBarrel02;
		[SerializeField] private GameObject mScope01;
		[SerializeField] private GameObject mScope02;
		[SerializeField] private PlayerWeaponScript mPlayerWeaponRef;

		private bool mEnabled = true; //everything starts enabled

		private void Start()
		{
			mBarrel01Button.OnClick += ApplyBarrel01;
			mBarrel02Button.OnClick += ApplyBarrel02;
			mScope01Button.OnClick += ApplyScope01;
			mScope02Button.OnClick += ApplyScope02;

			EventManager.OnUIToggle += HandleUIToggle;
			EventManager.UIToggle();
		}

		private void HandleUIToggle()
		{
			mEnabled = !mEnabled;
			SetChildrenState(mEnabled);

			ServiceLocator.Get<IInput>()
				.SetInputLevelState(Input.InputLevel.Gameplay, !mEnabled);
		}

		private void SetChildrenState(bool state)
		{
			foreach (Transform t in transform)
				t.gameObject.SetActive(state);
		}

		private void OnDestroy()
		{
			mBarrel01Button.OnClick -= ApplyBarrel01;
			mBarrel02Button.OnClick -= ApplyBarrel02;
			mScope01Button.OnClick -= ApplyScope01;
			mScope02Button.OnClick -= ApplyScope02;
			EventManager.OnUIToggle -= HandleUIToggle;
		}

		private void ApplyBarrel01()
		{
			mPlayerWeaponRef.AttachNewPart(PlayerWeaponScript.Attachment.Barrel, Instantiate(mBarrel01).GetComponent<WeaponPartScript>());
			EventManager.Notify(EventManager.UIToggle);
		}

		private void ApplyBarrel02()
		{
			mPlayerWeaponRef.AttachNewPart(PlayerWeaponScript.Attachment.Barrel, Instantiate(mBarrel02).GetComponent<WeaponPartScript>());
			EventManager.Notify(EventManager.UIToggle);
		}

		private void ApplyScope01()
		{

			mPlayerWeaponRef.AttachNewPart(PlayerWeaponScript.Attachment.Scope, Instantiate(mScope01).GetComponent<WeaponPartScript>());
			EventManager.Notify(EventManager.UIToggle);
		}

		private void ApplyScope02()
		{
			mPlayerWeaponRef.AttachNewPart(PlayerWeaponScript.Attachment.Scope, Instantiate(mScope02).GetComponent<WeaponPartScript>());
			EventManager.Notify(EventManager.UIToggle);
		}
	}
}
