using System;
using System.Collections.Generic;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

namespace Prototype2
{
	public class UIManager : MonoBehaviour
	{
		public static readonly int CLIP_CURRENT = "player_clip_current".GetHashCode();
		public static readonly int CLIP_TOTAL = "player_clip_total".GetHashCode();

		[SerializeField] private ActionProvider mBarrel01Button;
		[SerializeField] private ActionProvider mBarrel02Button;
		[SerializeField] private ActionProvider mScope01Button;
		[SerializeField] private ActionProvider mScope02Button;
		[SerializeField] private ActionProvider mMech01Button;
		[SerializeField] private ActionProvider mMech02Button;
		[SerializeField] private GameObject mBarrel01;
		[SerializeField] private GameObject mBarrel02;
		[SerializeField] private GameObject mScope01;
		[SerializeField] private GameObject mScope02;
		[SerializeField] private GameObject mMech01;
		[SerializeField] private GameObject mMech02;

		private bool mEnabled = true; //everything starts enabled
		private Dictionary<int, BoundProperty> mPropertyDictionary;
		public Dictionary<int, BoundProperty> propertyMap { get { return mPropertyDictionary; } }

		private void Awake()
		{
			mPropertyDictionary = new Dictionary<int, BoundProperty>()
			{
				{ CLIP_CURRENT, null },
				{ CLIP_TOTAL, null },
			};
		}

		private void Start()
		{
			mBarrel01Button.OnClick += ApplyBarrel01;
			mBarrel02Button.OnClick += ApplyBarrel02;
			mScope01Button.OnClick += ApplyScope01;
			mScope02Button.OnClick += ApplyScope02;
			mMech01Button.OnClick += ApplyMech01;
			mMech02Button.OnClick += ApplyMech02;
			
			EventManager.OnBoundPropertyCreated += BoundPropertyCreated;
			EventManager.OnUIToggle += HandleUIToggle;
			EventManager.UIToggle();
		}

		private void OnDestroy()
		{
			mBarrel01Button.OnClick -= ApplyBarrel01;
			mBarrel02Button.OnClick -= ApplyBarrel02;
			mScope01Button.OnClick -= ApplyScope01;
			mScope02Button.OnClick -= ApplyScope02;
			EventManager.OnUIToggle -= HandleUIToggle;
			EventManager.OnBoundPropertyCreated -= BoundPropertyCreated;
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
		
		private void BoundPropertyCreated(BoundProperty boundProperty, int i)
		{
			mPropertyDictionary[i] = boundProperty;
		}

		private void ApplyBarrel01()
		{
			Instantiate(mBarrel01).GetComponent<WeaponPickupScript>().ConfirmAttach();
		}

		private void ApplyBarrel02()
		{
			Instantiate(mBarrel02).GetComponent<WeaponPickupScript>().ConfirmAttach();
		}

		private void ApplyScope01()
		{
			Instantiate(mScope01).GetComponent<WeaponPickupScript>().ConfirmAttach();
		}

		private void ApplyScope02()
		{
			Instantiate(mScope02).GetComponent<WeaponPickupScript>().ConfirmAttach();
		}

		private void ApplyMech01()
		{
			Instantiate(mMech01).GetComponent<WeaponPickupScript>().ConfirmAttach();
		}

		private void ApplyMech02()
		{
			Instantiate(mMech02).GetComponent<WeaponPickupScript>().ConfirmAttach();
		}
	}
}
