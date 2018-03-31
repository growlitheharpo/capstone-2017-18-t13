using KeatsLib.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	/// <inheritdoc />
	public class BoundTeamColorPercentField : BoundUIElement<float>
	{
		/// Inspector variables
		[SerializeField] private float mEmptyValue;
		[SerializeField] private float mFullValue = 1.0f;
		[SerializeField] private bool mUseHardCutoff;
		[SerializeField] private float mCutoffValue;
		[SerializeField] private Color mEmptyColor;

		/// Private variables
		private UIImage mImage;
		private Color mFullColor;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		protected override void Start()
		{
			mImage = GetComponent<UIImage>();
			mFullColor = mImage.color;

			EventManager.LocalGUI.OnLocalPlayerAssignedTeam += OnLocalPlayerAssignedTeam;
			base.Start();
		}

		/// <inheritdoc />
		protected override void OnDestroy()
		{
			base.OnDestroy();
			EventManager.LocalGUI.OnLocalPlayerAssignedTeam -= OnLocalPlayerAssignedTeam;
		}

		/// <summary>
		/// EVENT HANDLER: LocalGUI.OnLocalPlayerAssignedTeam
		/// </summary>
		private void OnLocalPlayerAssignedTeam(CltPlayer player)
		{
			mFullColor = player.teamColor;
		}

		/// <inheritdoc />
		protected override void HandlePropertyChanged()
		{
			if (mUseHardCutoff)
				mImage.color = property.value < mCutoffValue ? mEmptyColor : mFullColor;
			else
			{
				float val = property.value.Rescale(mEmptyValue, mFullValue);
				mImage.color = Color.Lerp(mEmptyColor, mFullColor, val);
			}
		}
	}
}
