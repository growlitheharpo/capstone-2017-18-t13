using UnityEngine;

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
