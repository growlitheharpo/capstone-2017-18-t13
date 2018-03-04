using UnityEngine;

namespace FiringSquad.Debug
{
	/// <summary>
	/// Utility class that directly draws the current framerate to the top left of the screen.
	/// </summary>
	public class FPSDisplay : MonoBehaviour
	{
		private float mDeltaTime;
		
		/// <summary>
		/// Unity's Update function
		/// </summary>
		private void Update()
		{
			mDeltaTime += (Time.unscaledDeltaTime - mDeltaTime) * 0.1f;
		}

		/// <summary>
		/// Unity's OnGUI function
		/// </summary>
		private void OnGUI()
		{
			Rect rect = new Rect(0, 0, Screen.width, Screen.height * 2.0f / 100);

			GUIStyle style = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft,
				fontSize = Screen.height * 2 / 100,
				normal = { textColor = new Color(0.2f, 0.5f, 0.8f, 1.0f) }
			};

			float msec = mDeltaTime * 1000.0f;
			float fps = 1.0f / mDeltaTime;

			string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
			GUI.Label(rect, text, style);
		}
	}
}
