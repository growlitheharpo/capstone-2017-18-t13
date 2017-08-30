public interface IWeaponEffect
{
	bool overridesWeaponFire { get; }
	void ApplyAffectFire(IWeapon sourceWeapon);
	void ApplyEffectHit(IProjectile sourceProjectile);
}
