using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using Input = UnityEngine.Input;

namespace FiringSquad.Debug
{
	/// <summary>
	/// Draw a debug visualizer to represent the current dispersion.
	/// </summary>
	public class WeaponDebugVisualizer : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private Mesh mConeMesh;
		[SerializeField] private Material mWireframe;

		/// Private variables
		private Material mMinMat, mCurMat, mMaxMat;
		private LineRenderer[] mLineRenderers;
		private BaseWeaponScript mCurrentScript;
		private bool mEnabled;

		private void Start()
		{
			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.F5, INPUT_ToggleState, InputLevel.None);

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

		/// <summary>
		/// INPUT HANDLER: Toggle whether or not the visualizer is active.
		/// </summary>
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
				mCurrentScript = target.GetComponentInParent<BaseWeaponScript>();

			if (mCurrentScript == null)
				mCurrentScript = FindObjectOfType<BaseWeaponScript>();
#else
			mCurrentScript = FindObjectOfType<BaseWeaponScript>();
#endif
		}

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			// I'm so sorry for all of this.

			// Clear the positions of all our line renderers.
			foreach (LineRenderer l in mLineRenderers)
				l.SetPositions(new Vector3[] { });

			if (mCurrentScript == null)
				return;

			// Grab the weapon's data.
			WeaponData weaponStats = mCurrentScript.currentData;
			float currentDispersion = mCurrentScript.GetCurrentDispersionFactor(true);
			Transform target = mCurrentScript.aimRoot;

			// Grab the end hit point (for the 3D view cone)
			Vector3 hitPoint = GetHitPoint(target);

			// Determine the scales of our circles.
			float scaleVal1 = Mathf.Tan(Mathf.Asin(weaponStats.minimumDispersion));
			float scaleVal2 = Mathf.Tan(Mathf.Asin(weaponStats.maximumDispersion));
			float scaleVal3 = Mathf.Tan(Mathf.Asin(currentDispersion));
			float lengthScale = Vector3.Distance(target.position, hitPoint);

			// Do a bunch of matrix math for scaling the 3D cone.
			Matrix4x4 dotScaleMin = Matrix4x4.Scale(new Vector3(scaleVal1, scaleVal1, 1.0f));
			Matrix4x4 dotScaleMax = Matrix4x4.Scale(new Vector3(scaleVal2, scaleVal2, 1.0f));
			Matrix4x4 dotScaleCur = Matrix4x4.Scale(new Vector3(scaleVal3, scaleVal3, 1.0f));
			Matrix4x4 totalScale = Matrix4x4.Scale(Vector3.one * lengthScale);
			Matrix4x4 rotation = Matrix4x4.Rotate(target.rotation);
			Matrix4x4 translation = Matrix4x4.Translate(target.position);

			// Draw the 3D cones.
			Graphics.DrawMesh(mConeMesh, translation * rotation * totalScale * dotScaleMin, mMinMat, 0);
			Graphics.DrawMesh(mConeMesh, translation * rotation * totalScale * dotScaleMax, mMaxMat, 0);
			Graphics.DrawMesh(mConeMesh, translation * rotation * totalScale * dotScaleCur, mCurMat, 0);

			// Update the line renderers.
			DrawLines(scaleVal1, target, 0);
			DrawLines(scaleVal2, target, 1);
			DrawLines(scaleVal3, target, 2);
		}

		/// <summary>
		/// Updates the line renderer to have a screen-space radius of the given scale.
		/// </summary>
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

		/// <summary>
		/// Get the world position of the weapon's current hit point.
		/// </summary>
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
