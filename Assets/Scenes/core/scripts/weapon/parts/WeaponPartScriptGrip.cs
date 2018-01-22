namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptGrip : WeaponPartScript
	{
		/// <inheritdoc />
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Grip; } }
	}
}
