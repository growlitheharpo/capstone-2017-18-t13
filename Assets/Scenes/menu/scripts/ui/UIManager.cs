using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu UI manager.
/// </summary>
public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject mMainElementHolder;
	[SerializeField] private ActionProvider mProto3Button;
	[SerializeField] private ActionProvider mArtProtoButton;
	[SerializeField] private ActionProvider mQuitButton;

	private void Start()
	{
		mProto3Button.OnClick += LaunchProto3;
		mArtProtoButton.OnClick += LaunchArtProto;
		mQuitButton.OnClick += ClickQuit;
	}
	
	private void LaunchProto3()
	{
		mMainElementHolder.SetActive(false);
		/*EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.BASE_WORLD));
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.PROTOTYPE3_SCENE, LoadSceneMode.Additive));*/
	}

	private void LaunchArtProto()
	{
		mMainElementHolder.SetActive(false);
		//EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.ART_PROTOTYPE_SCENE));
	}

	private void ClickQuit()
	{
		ServiceLocator.Get<IGamestateManager>()
			.RequestShutdown();
	}
}
