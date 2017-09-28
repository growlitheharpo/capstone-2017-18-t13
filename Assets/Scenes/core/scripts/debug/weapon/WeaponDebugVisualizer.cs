using FiringSquad.Data;
using FiringSquad.Gameplay;
using UnityEngine;

namespace FiringSquad.Debug
{
	public class WeaponDebugVisualizer : MonoBehaviour
	{
		[SerializeField] private Mesh mConeMesh;
		[SerializeField] private Material mWireframe;
		[SerializeField] private bool mOverrideEye;

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

			if (!mEnabled)
			{
				mCurrentScript = null;
				return;
			}

#if UNITY_EDITOR
			GameObject target = UnityEditor.Selection.activeGameObject;

			if (target != null)
				mCurrentScript = target.GetComponentUpwards<BaseWeaponScript>();

			if (mCurrentScript == null)
				mCurrentScript = FindObjectOfType<PlayerWeaponScript>();
#else
			mCurrentScript = FindObjectOfType<PlayerWeaponScript>();
#endif
		}

		private void Update()
		{
			//Matrix4x4 transform = new Matrix4x4();
			//transform.ToM
			if (mCurrentScript == null)
				return;

			WeaponData weaponStats = BaseWeaponScript.DebugHelper.GetWeaponData(mCurrentScript);
			Transform target = BaseWeaponScript.DebugHelper.GetWeaponAimRoot(mCurrentScript, mOverrideEye);

			Vector3 hitPoint = GetHitPoint(target);

			float scaleVal = Mathf.Tan(Mathf.Asin(weaponStats.spread));

			Matrix4x4 dotScale = Matrix4x4.Scale(new Vector3(scaleVal, scaleVal, 1.0f));
			Matrix4x4 totalScale = Matrix4x4.Scale(Vector3.one * Vector3.Distance(target.position, hitPoint));
			Matrix4x4 firstTranslation = Matrix4x4.Translate(new Vector3(0.0f, 0.0f, 0.0007f));
			Matrix4x4 rotation = Matrix4x4.Rotate(target.rotation);
			Matrix4x4 translation = Matrix4x4.Translate(target.position);

			Graphics.DrawMesh(mConeMesh, translation * rotation * firstTranslation * totalScale * dotScale, mWireframe, 0);
		}

		private Vector3 GetHitPoint(Transform target)
		{
			Ray r = new Ray(target.position, target.forward);
			RaycastHit hit;

			if (!Physics.Raycast(r, out hit, 60000.0f))
				return target.position + target.forward * 2.0f;
			return hit.point;
		}
	}
}
