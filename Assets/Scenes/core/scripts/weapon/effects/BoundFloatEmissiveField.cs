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
		[SerializeField] private string mEmissiveProperty = "_EmissionColor";
		[SerializeField] private Color mStartColor = Color.white;
		[SerializeField] private Color mEndColor = Color.white;

		/// Private variables
		private Material mMaterialInstance;

		/// <inheritdoc />
		protected override void Start()
		{
			mMaterialInstance = GetComponent<MeshRenderer>().material;
			base.Start();
		}

		/// <inheritdoc />
		protected override void HandlePropertyChanged()
		{
			Color val = Color.Lerp(mEndColor, mStartColor, property.value);
			mMaterialInstance.SetColor(mEmissiveProperty, val);
		}
	}
}
