
using System;
using System.Collections.Generic;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFArmatureNodeInfo : MonoBehaviour
	{
		public string ArmatureId = Guid.NewGuid().ToString();
		public string ArmatureName;
		public GameObject Root;
		public List<GameObject> Bones = new List<GameObject>();
	}
}
