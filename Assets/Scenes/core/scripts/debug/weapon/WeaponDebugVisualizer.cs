using FiringSquad.Gameplay;
using UnityEngine;

namespace FiringSquad.Debug
{
	public class WeaponDebugVisualizer : MonoBehaviour
	{
		[SerializeField] private Mesh mConeMesh;
		[SerializeField] private Material mWireframe;

		private BaseWeaponScript mCurrentScript;
		private bool mEnabled;

		private void Start()
		{
			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.F5, INPUT_ToggleState, KeatsLib.Unity.Input.InputLevel.None);
		}

		private void INPUT_ToggleState()
		{
			mEnabled = !mEnabled;
		}

		void Update()
		{
			//Matrix4x4 transform = new Matrix4x4();
			//transform.ToM
			Matrix4x4 localToWorld = transform.localToWorldMatrix;
			//Matrix4x4 localPos = Matrix4x4.Translate(transform.localPosition);
			Matrix4x4 localShift = Matrix4x4.Translate(new Vector3(0, 0, 0.5f));

			Graphics.DrawMesh(mConeMesh, localToWorld * localShift, mWireframe, 0);
		}
	}
}
