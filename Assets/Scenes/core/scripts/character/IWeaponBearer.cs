using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;

namespace FiringSquad.Gameplay
{
	public interface IWeaponBearer : ICharacter
	{
		IWeapon weapon { get; }
		WeaponPartCollection defaultParts { get; }

		void PlayFireAnimation();
		void BindWeaponToBearer(IModifiableWeapon wep, bool bindUI = false);
	}
}
