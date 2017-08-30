public interface IWeaponModifier
{
	IWeaponModifierData modifierData { get; }
	IWeapon attachedWeapon { get; }
	bool activated { get; }

	bool Toggle();
}
