using System.Collections.Generic;
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

		private LineRenderer mLineRenderer;
		private BaseWeaponScript mCurrentScript;
		private bool mEnabled;

		private void Start()
		{
			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.F5, INPUT_ToggleState, KeatsLib.Unity.Input.InputLevel.None);

			mLineRenderer = GetComponent<LineRenderer>();
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
			if (mCurrentScript == null)
			{
				mLineRenderer.positionCount = 0;
				return;
			}

			WeaponData weaponStats = BaseWeaponScript.DebugHelper.GetWeaponData(mCurrentScript);
			Transform target = BaseWeaponScript.DebugHelper.GetWeaponAimRoot(mCurrentScript, mOverrideEye);

			Vector3 hitPoint = GetHitPoint(target);

			float scaleVal = Mathf.Tan(Mathf.Asin(weaponStats.spread));
			float lengthScale = Vector3.Distance(target.position, hitPoint);

			Matrix4x4 dotScale = Matrix4x4.Scale(new Vector3(scaleVal, scaleVal, 1.0f));
			Matrix4x4 totalScale = Matrix4x4.Scale(Vector3.one * lengthScale);
			Matrix4x4 rotation = Matrix4x4.Rotate(target.rotation);
			Matrix4x4 translation = Matrix4x4.Translate(target.position);

			Graphics.DrawMesh(mConeMesh, translation * rotation * totalScale * dotScale, mWireframe, 0);
			DrawLines(scaleVal, target);
		}

		private void DrawLines(float scaleVal, Transform target)
		{
			var points = new List<Vector3>();
			for (float theta = 0; theta < 2 * Mathf.PI + 0.2f; theta += 0.1f)
			{
				Vector3 basePoint = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0.0f);
				basePoint *= scaleVal;

				Matrix4x4 translation1 = Matrix4x4.Translate(new Vector3(0.0f, 0.0f, 1.0f));
				Matrix4x4 bringInScale = Matrix4x4.Scale(Vector3.one * 0.5f);
				//Matrix4x4 rotation = Matrix4x4.Rotate(target.rotation);
				//Matrix4x4 translation2 = Matrix4x4.Translate(target.position);
				Matrix4x4 targetTransform = target.localToWorldMatrix;
				basePoint = (targetTransform * bringInScale * translation1).MultiplyPoint(basePoint);

				points.Add(basePoint);
			}

			mLineRenderer.positionCount = points.Count;
			mLineRenderer.SetPositions(points.ToArray());
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
