using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hit_indicator_selfdestruct : MonoBehaviour
{

	void Delete ()
	{
		Destroy(this.gameObject);
	}
}
