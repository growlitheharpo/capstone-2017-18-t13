using System.Reflection;
using FiringSquad.Gameplay;
using UnityEngine;

public class DEBUGFramerateChecker : MonoBehaviour
{
	private Transform mCameraRef;
	private PlayerMovementScript mMovement;

	private FieldInfo mRotationField, mRecoilField;

	private void Awake()
	{
		mMovement = FindObjectOfType<PlayerMovementScript>();
		mCameraRef = mMovement.GetComponentInChildren<Camera>().transform;

		mRotationField = typeof(PlayerMovementScript).GetField("mRotationY", BindingFlags.NonPublic | BindingFlags.Instance);
		mRecoilField = typeof(PlayerMovementScript).GetField("mRecoilAmount", BindingFlags.NonPublic | BindingFlags.Instance);
	}

    // Update is called once per frame
	private void Update()
	{
		QualitySettings.vSyncCount = 0;

		if (Input.GetKeyDown(KeyCode.L))
			ResetRotation();
	}

	private void ResetRotation()
	{
		mMovement.transform.rotation = Quaternion.identity;
		mRotationField.SetValue(mMovement, 0.0f);
		mCameraRef.rotation = Quaternion.identity;
	}

	private void DISABLED_OnGUI()
	{
		var rot = mCameraRef.rotation.eulerAngles;
		GUILayout.Label(rot.ToString());

		float recoil = (float)mRecoilField.GetValue(mMovement);
		GUILayout.Label("Recoil: " + recoil.ToString("##.000"));

		if (GUILayout.Button("180 FPS"))
			Application.targetFrameRate = 180;
		if (GUILayout.Button("120 FPS"))
			Application.targetFrameRate = 120;
		if (GUILayout.Button("60 FPS"))
			Application.targetFrameRate = 60;
		if (GUILayout.Button("30 FPS"))
			Application.targetFrameRate = 30;
		if (GUILayout.Button("15 FPS"))
			Application.targetFrameRate = 15;
		if (GUILayout.Button("5 FPS"))
			Application.targetFrameRate = 5;
		if (GUILayout.Button("DEFAULT"))
			Application.targetFrameRate = -1;
	}
}
