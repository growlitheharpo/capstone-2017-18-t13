using System.Collections.Generic;

public interface IWeapon
{
	ICharacter bearer { get; }

	IEnumerable<IWeaponModifier> GetModifiers(bool onlyActive);
	IProjectile Fire();
}
