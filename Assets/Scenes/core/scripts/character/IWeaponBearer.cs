using FiringSquad.Data;

namespace FiringSquad.Gameplay
{
	public interface IWeaponBearer : ICharacter
	{
		IWeapon weapon { get; }
		WeaponPartCollection defaultParts { get; }
	}
}
