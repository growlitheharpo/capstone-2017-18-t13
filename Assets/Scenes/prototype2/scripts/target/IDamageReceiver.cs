using UnityEngine;

namespace Prototype2
{
	public interface IDamageReceiver
	{
		void ApplyDamage(float amount, Vector3 point);
	}
}
