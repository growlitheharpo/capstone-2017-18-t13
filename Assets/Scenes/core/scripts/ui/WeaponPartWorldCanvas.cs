using System.Collections;
using System.Linq;
using FiringSquad.Gameplay;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

public class WeaponPartWorldCanvas : MonoBehaviour
{
	[SerializeField] private UIText mPartText;
	[SerializeField] private UIText mAlreadyHasText;
	[SerializeField] private CanvasGroup mCanvasGroup;

	private Transform mPlayerRef;
	private BaseWeaponScript mPlayerWeapon;
	private WeaponPartScript mLinkedPart;

	private void Awake()
	{
		StartCoroutine(GrabPlayerReference());
		mAlreadyHasText.gameObject.SetActive(false);
		mCanvasGroup.gameObject.SetActive(false);
	}

	public void LinkToObject(WeaponPartScript part)
	{
		mCanvasGroup.gameObject.SetActive(true);
		mLinkedPart = part;
		mPartText.text = part.prettyName;
	}

	private IEnumerator GrabPlayerReference()
	{
		while (mPlayerRef == null)
		{
			yield return null;
			CltPlayer script = FindObjectsOfType<CltPlayer>().FirstOrDefault(x => x.isCurrentPlayer);
			if (script != null)
			{
				mPlayerRef = script.eye.transform;
				mPlayerWeapon = (BaseWeaponScript)script.weapon;
			}
		}
	}

	// Update is called once per frame
	private void Update()
	{
		if (mPlayerRef == null)
			return;

		DoAlpha();
		Rotate();
		UpdateDoesHave();
	}

	private void Rotate()
	{
		Vector3 direction = transform.position - mPlayerRef.position;
		Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
		rot = Quaternion.Euler(0.0f, rot.eulerAngles.y, 0.0f);
		transform.rotation = rot;
	}

	private void DoAlpha()
	{
		Vector3 direction = transform.position - mPlayerRef.position;
		float dot = Vector3.Dot(direction.normalized, mPlayerRef.forward);
		dot = (Mathf.Pow(dot, 10.0f) - 0.6f) * 2.5f;

		mCanvasGroup.alpha = dot;
	}

	private void UpdateDoesHave()
	{
		WeaponPartScript current = mPlayerWeapon.currentParts[mLinkedPart.attachPoint];
		mAlreadyHasText.gameObject.SetActive(current.partId == mLinkedPart.partId);
	}
}
