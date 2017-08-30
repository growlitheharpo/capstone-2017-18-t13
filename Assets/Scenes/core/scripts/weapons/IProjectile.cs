using UnityEngine;

public interface IProjectile
{
	ICharacter hitCharacter { get; }
	ICharacter sourceCharacter { get; }
	IWeapon sourceWeapon { get; }

	Collision contactInformation { get; }

	void Create(IWeapon weapon); // get modifiers from weapon here
	bool HasMadeContact();

	void DoHit();
}
