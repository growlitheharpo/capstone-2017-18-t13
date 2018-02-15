using FiringSquad.Gameplay.UI;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// Utiltiy component that binds the emissive color of the target mesh renderer to a provided float.
	/// </summary>
	public class BoundFloatEmissiveField : BoundUIElement<float>
	{
		/// Inspector variables
		[SerializeField] private MeshRenderer mTargetMeshRenderer;
		[SerializeField] private string mEmissiveProperty = "_EmissionColor";
		[SerializeField] private Color mStartColor;
		[SerializeField] private Color mEndColor;

		/// <inheritdoc />
		protected override void HandlePropertyChanged()
		{
			
		}
	}
}
