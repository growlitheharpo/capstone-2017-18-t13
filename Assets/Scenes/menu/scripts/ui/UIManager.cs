using UnityEngine;

/// <summary>
/// Main menu UI manager.
/// </summary>
public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject mMainElementHolder;
	[SerializeField] private ActionProvider mProto1Button;
	[SerializeField] private ActionProvider mProto2Button;
	[SerializeField] private ActionProvider mProto3Button;
	[SerializeField] private ActionProvider mArtProtoButton;
	[SerializeField] private ActionProvider mQuitButton;

	private void Start()
	{
		mProto1Button.OnClick += LaunchProto1;
		mProto2Button.OnClick += LaunchProto2;
		mProto3Button.OnClick += LaunchProto3;
		mArtProtoButton.OnClick += LaunchArtProto;
		mQuitButton.OnClick += ClickQuit;
	}

	private void LaunchProto1()
	{
		mMainElementHolder.SetActive(false);
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.PROTOTYPE1_SETUP_SCENE));
	}
	
	private void LaunchProto2()
	{
		mMainElementHolder.SetActive(false);
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.PROTOTYPE2_SCENE));
	}

	private void LaunchProto3()
	{
		mMainElementHolder.SetActive(false);
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.PROTOTYPE3_SCENE));
	}

	private void LaunchArtProto()
	{
		mMainElementHolder.SetActive(false);
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.ART_PROTOTYPE_SCENE));
	}

	private void ClickQuit()
	{
		ServiceLocator.Get<IGamestateManager>()
			.RequestShutdown();
	}
}
