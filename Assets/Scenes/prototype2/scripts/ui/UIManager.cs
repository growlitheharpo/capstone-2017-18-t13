using UnityEngine;

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

		private void Start()
		{
			mBarrel01Button.OnClick += ApplyBarrel01;
			mBarrel02Button.OnClick += ApplyBarrel02;
			mScope01Button.OnClick += ApplyScope01;
			mScope02Button.OnClick += ApplyScope02;
		}

		private void OnDestroy()
		{
			mBarrel01Button.OnClick -= ApplyBarrel01;
			mBarrel02Button.OnClick -= ApplyBarrel02;
			mScope01Button.OnClick -= ApplyScope01;
			mScope02Button.OnClick -= ApplyScope02;
		}

		private void ApplyBarrel01()
		{
			mPlayerWeaponRef.AttachNewPart(PlayerWeaponScript.Attachment.Barrel, Instantiate(mBarrel01).GetComponent<WeaponPartScript>());
		}

		private void ApplyBarrel02()
		{
			mPlayerWeaponRef.AttachNewPart(PlayerWeaponScript.Attachment.Barrel, Instantiate(mBarrel02).GetComponent<WeaponPartScript>());
		}

		private void ApplyScope01()
		{

			mPlayerWeaponRef.AttachNewPart(PlayerWeaponScript.Attachment.Scope, Instantiate(mScope01).GetComponent<WeaponPartScript>());
		}

		private void ApplyScope02()
		{
			mPlayerWeaponRef.AttachNewPart(PlayerWeaponScript.Attachment.Scope, Instantiate(mScope02).GetComponent<WeaponPartScript>());
		}
	}
}
