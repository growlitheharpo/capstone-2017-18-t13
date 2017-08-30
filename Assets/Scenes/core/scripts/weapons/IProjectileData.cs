using System.Collections.Generic;

public interface IProjectileData
{
	float baseSpeed { get; }
	float baseDamage { get; }
	IEnumerable<IWeaponEffect> baseEffects { get; }
}
