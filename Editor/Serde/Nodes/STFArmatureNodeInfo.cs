
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using STF.Util;
using UnityEditor;
using UnityEngine;

namespace STF.Serde
{
	public class STFArmatureNodeInfo : MonoBehaviour
	{
		public string ArmatureId;
		public string ArmatureName;
		public GameObject Root;
		public List<GameObject> Bones = new List<GameObject>();
	}
}

#endif
