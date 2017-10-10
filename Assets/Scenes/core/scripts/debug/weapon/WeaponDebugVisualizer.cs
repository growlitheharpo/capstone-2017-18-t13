using System.Collections.Generic;
using System.Linq;
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

		private LineRenderer[] mLineRenderers;
		private BaseWeaponScript mCurrentScript;
		private bool mEnabled;

		private Material mMinMat, mCurMat, mMaxMat;

		private void Start()
		{
			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.F5, INPUT_ToggleState, KeatsLib.Unity.Input.InputLevel.None);

			mLineRenderers = GetComponentsInChildren<LineRenderer>();

			mMinMat = new Material(mWireframe);
			mMaxMat = new Material(mWireframe);
			mCurMat = new Material(mWireframe);

			mMinMat.SetColor("_LineColor", Color.green);
			mMaxMat.SetColor("_LineColor", Color.red);
			mCurMat.SetColor("_LineColor", new Color(0.0f, 0.4f, 1.0f));

			mLineRenderers[0].material.SetColor("_LineColor", Color.green);
			mLineRenderers[1].material.SetColor("_LineColor", Color.red);
			mLineRenderers[2].material.SetColor("_LineColor", new Color(0.0f, 0.4f, 1.0f));
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

			/*if (mCurrentScript == null)
				mCurrentScript = FindObjectOfType<PlayerWeaponScript>();*/
#else
			mCurrentScript = FindObjectOfType<PlayerWeaponScript>();
#endif
		}

		private void Update()
		{
			foreach (LineRenderer l in mLineRenderers)
				l.SetPositions(new Vector3[] { });

			if (mCurrentScript == null)
			{
				return;
			}

			WeaponData weaponStats = BaseWeaponScript.DebugHelper.GetWeaponData(mCurrentScript);
			Transform target = BaseWeaponScript.DebugHelper.GetWeaponAimRoot(mCurrentScript, mOverrideEye);

			Vector3 hitPoint = GetHitPoint(target);

			float scaleVal1 = Mathf.Tan(Mathf.Asin(weaponStats.minimumDispersion));
			float scaleVal2 = Mathf.Tan(Mathf.Asin(weaponStats.maximumDispersion));
			float scaleVal3 = Mathf.Tan(Mathf.Asin(BaseWeaponScript.DebugHelper.GetCurrentDispersion(mCurrentScript)));
			float lengthScale = Vector3.Distance(target.position, hitPoint);

			Matrix4x4 dotScaleMin = Matrix4x4.Scale(new Vector3(scaleVal1, scaleVal1, 1.0f));
			Matrix4x4 dotScaleMax = Matrix4x4.Scale(new Vector3(scaleVal2, scaleVal2, 1.0f));
			Matrix4x4 dotScaleCur = Matrix4x4.Scale(new Vector3(scaleVal3, scaleVal3, 1.0f));
			Matrix4x4 totalScale = Matrix4x4.Scale(Vector3.one * lengthScale);
			Matrix4x4 rotation = Matrix4x4.Rotate(target.rotation);
			Matrix4x4 translation = Matrix4x4.Translate(target.position);

			Graphics.DrawMesh(mConeMesh, translation * rotation * totalScale * dotScaleMin, mMinMat, 0);
			Graphics.DrawMesh(mConeMesh, translation * rotation * totalScale * dotScaleMax, mMaxMat, 0);
			Graphics.DrawMesh(mConeMesh, translation * rotation * totalScale * dotScaleCur, mCurMat, 0);

			DrawLines(scaleVal1, target, 0);
			DrawLines(scaleVal2, target, 1);
			DrawLines(scaleVal3, target, 2);
		}

		private void DrawLines(float scaleVal, Transform target, int whichRenderer)
		{
			var points = new List<Vector3>();
			for (float theta = 0; theta < 2 * Mathf.PI + 0.2f; theta += 0.1f)
			{
				Vector3 basePoint = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0.0f);
				basePoint *= scaleVal;

				Matrix4x4 translation1 = Matrix4x4.Translate(new Vector3(0.0f, 0.0f, 1.0f));
				Matrix4x4 bringInScale = Matrix4x4.Scale(Vector3.one * 0.5f);
				Matrix4x4 targetTransform = target.localToWorldMatrix;
				basePoint = (targetTransform * bringInScale * translation1).MultiplyPoint(basePoint);

				points.Add(basePoint);
			}

			mLineRenderers[whichRenderer].positionCount = points.Count;
			mLineRenderers[whichRenderer].SetPositions(points.ToArray());
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
