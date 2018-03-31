using FiringSquad.Core;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// A "dummy" implementation of the base weapon script that has no actual functionality.
	/// </summary>
	public class MenuDemoWeaponScript : MonoBehaviour, IModifiableWeapon
	{
		/// Inspector variables
		[SerializeField] private WeaponData mDefaultData;
		[SerializeField] private WeaponPartCollection mDefaultParts;
		[SerializeField] private float mRotationRate;

		/// Private variables
		private BaseWeaponView mWeaponView;

		/// <inheritdoc cref="IModifiableWeapon" />
		public IWeaponBearer bearer { get { return null; } set { } }

		/// <inheritdoc cref="IModifiableWeapon" />
		public Transform aimRoot { get { return null; } set { } }

		/// <inheritdoc cref="IModifiableWeapon" />
		public Vector3 positionOffset { get { return Vector3.zero; } set { } }

		/// <inheritdoc />
		public bool aimDownSightsActive { get { return false; } }

		/// <inheritdoc />
		public int shotsLeftInClip { get { return 0; } }

		/// <inheritdoc />
		public WeaponData baseData { get { return mDefaultData; } }

		/// <inheritdoc />
		public WeaponData currentData { get; private set; }

		/// <inheritdoc />
		public WeaponPartCollection currentParts { get; private set; }

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mWeaponView = GetComponent<BaseWeaponView>();
			currentData = new WeaponData(baseData);
			currentParts = new WeaponPartCollection();
		}

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			ResetToDefaultParts();
		}

		/// <inheritdoc />
		public void AttachNewPart(byte partId, int durability = WeaponPartScript.USE_DEFAULT_DURABILITY)
		{
			if (partId == 0)
				return;

			WeaponPartScript prefab = ServiceLocator.Get<IWeaponPartManager>().GetPrefabScript(partId);
			WeaponPartScript instance = prefab.SpawnForWeapon(this);

			mWeaponView.MoveAttachmentToPoint(instance);
			currentParts[instance.attachPoint] = instance;
			instance.durability = durability == WeaponPartScript.USE_DEFAULT_DURABILITY ? prefab.durability : durability;

			currentData = WeaponData.ActivatePartEffects(baseData, currentParts);

			foreach (var r in GetComponentsInChildren<Renderer>())
				ObjectHighlight.instance.AddRendererToHighlightList(r);
		}

		/// <inheritdoc />
		public void ResetToDefaultParts()
		{
			foreach (WeaponPartScript p in mDefaultParts)
				AttachNewPart(p.partId, WeaponPartScript.INFINITE_DURABILITY);
		}

		/// <summary>
		/// Unity's Update function
		/// </summary>
		private void Update()
		{
			transform.Rotate(Vector3.up, mRotationRate * Time.deltaTime);
		}

		/// <inheritdoc />
		public float GetCurrentRecoil()
		{
			return 0.0f;
		}

		/// <inheritdoc />
		public float GetCurrentDispersionFactor(bool forceNotZero)
		{
			return forceNotZero ? 0.0f : 0.5f;
		}

		/// <inheritdoc />
		public void BindPropertiesToUI() { }

		/// <inheritdoc />
		public void FireWeaponHold() { }

		/// <inheritdoc />
		public void FireWeaponUp() { }

		/// <inheritdoc />
		public void EnterAimDownSightsMode() { }

		/// <inheritdoc />
		public void ExitAimDownSightsMode() { }

		/// <inheritdoc />
		public void Reload() { }
	}
}
