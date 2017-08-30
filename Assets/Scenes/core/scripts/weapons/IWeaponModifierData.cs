public interface IWeaponModifierData
{
	Modifier.Float spreadModifier { get; }
	Modifier.Float fireRateModifier { get; }
	Modifier.Float damageModifier { get; }
	Modifier.Float recoilModifier { get; }

	Modifier.Array<IWeaponEffect> additiveEffects { get; }

	IProjectile projectileOverride { get; }
}
