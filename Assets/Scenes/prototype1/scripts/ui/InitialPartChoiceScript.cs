using System;
using System.Collections.Generic;
using FiringSquad.Gameplay;
using UnityEngine;

public class InitialPartChoiceScript : MonoBehaviour
{
	[Serializable]
	public class MenuState
	{
		public ActionProvider mCloseButton;
		public GameObject mHolder;
	}

	[Header("Main Holder")] public ActionProvider mOpenMechanisms;
	public ActionProvider
		mOpenBarrels,
		mOpenScopes,
		mOpenGrips,
		mPlay;

	[Header("Mechanisms")] public ActionProvider mMechButton1;
	public ActionProvider mMechButton2;
	public GameObject mMech1, mMech2;

	[Header("Barrels")] public ActionProvider mBarrelButton1;
	public ActionProvider mBarrelButton2, mBarrelButton3;
	public GameObject mBarrel1, mBarrel2, mBarrel3;

	[Header("Scopes")] public ActionProvider mScopeButton1;
	public ActionProvider mScopeButton2;
	public GameObject mScope1, mScope2;

	[Header("Grips")] public ActionProvider mGripButton1;
	public ActionProvider mGripButton2;
	public GameObject mGrip1, mGrip2;

	public MenuState mMainState;
	public MenuState mMechs, mBarrels, mScopes, mGrips;

	private IntroWeaponChoiceManager mManager;
	private MenuState[] mPartStates;

	private void Start()
	{
		mManager = FindObjectOfType<IntroWeaponChoiceManager>();
		mPartStates = new[] { mMechs, mBarrels, mScopes, mGrips };

		foreach (MenuState state in mPartStates)
		{
			state.mCloseButton.OnClick += CloseSubstate;
			state.mHolder.SetActive(false);
		}

		SetupMainHolder();
		SetupMechanisms();
		SetupBarrels();
		SetupScopes();
		SetupGrips();
	}

	private void SetupMainHolder()
	{
		mOpenMechanisms.OnClick += () =>
		{
			mMainState.mHolder.SetActive(false);
			mMechs.mHolder.SetActive(true);
		};
		mOpenBarrels.OnClick += () =>
		{
			mMainState.mHolder.SetActive(false);
			mBarrels.mHolder.SetActive(true);
		};
		mOpenScopes.OnClick += () =>
		{
			mMainState.mHolder.SetActive(false);
			mScopes.mHolder.SetActive(true);
		};
		mOpenGrips.OnClick += () =>
		{
			mMainState.mHolder.SetActive(false);
			mGrips.mHolder.SetActive(true);
		};
		mPlay.OnClick += () =>
		{
			EventManager.RequestSceneChange(GamestateManager.PROTOTYPE1_SCENE);
		};
	}

	private void SetupMechanisms()
	{
		mMechButton1.OnClick += () =>
		{
			mManager.AttachPart(mMech1);
		};
		mMechButton2.OnClick += () =>
		{
			mManager.AttachPart(mMech2);
		};
	}

	private void SetupBarrels()
	{
		mBarrelButton1.OnClick += () =>
		{
			mManager.AttachPart(mBarrel1);
		};
		mBarrelButton2.OnClick += () =>
		{
			mManager.AttachPart(mBarrel2);
		};
		mBarrelButton3.OnClick += () =>
		{
			mManager.AttachPart(mBarrel3);
		};
	}

	private void SetupScopes()
	{
		mScopeButton1.OnClick += () =>
		{
			mManager.AttachPart(mScope1);
		};
		mScopeButton2.OnClick += () =>
		{
			mManager.AttachPart(mScope2);
		};
	}

	private void SetupGrips()
	{
		mGripButton1.OnClick += () =>
		{
			mManager.AttachPart(mGrip1);
		};
		mGripButton2.OnClick += () =>
		{
			mManager.AttachPart(mGrip2);
		};
	}

	private void OnDestroy()
	{
		foreach (MenuState state in mPartStates)
			state.mCloseButton.OnClick -= CloseSubstate;
	}

	private void CloseSubstate()
	{
		foreach (MenuState state in mPartStates)
			state.mHolder.SetActive(false);

		mMainState.mHolder.SetActive(true);
	}
}
