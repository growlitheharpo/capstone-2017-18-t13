using FiringSquad.Data;

namespace FiringSquad.Gameplay
{
	public interface IWeaponBearer : ICharacter
	{
		IWeapon weapon { get; }
		WeaponDefaultsData defaultParts { get; }
	}
}
