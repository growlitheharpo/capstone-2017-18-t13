using UnityEngine;

/// <summary>
/// Controller attached to the main camera to reflect main menu options.
/// </summary>
public class CameraOptionsController : MonoBehaviour
{
	private Camera mCamera;

	private void Awake()
	{
		mCamera = GetComponent<Camera>();
		EventManager.OnApplyOptionsData += HandleOptionsUpdate;
	}

	private void HandleOptionsUpdate(IOptionsData data)
	{
		mCamera.fieldOfView = data.fieldOfView;
		AudioListener.volume = data.masterVolume;
	}

	private void OnDestroy()
	{
		EventManager.OnApplyOptionsData -= HandleOptionsUpdate;
	}
}
