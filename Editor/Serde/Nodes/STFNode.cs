
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace STF.Serde
{
	public class STFNode : MonoBehaviour
	{
		public string NodeId = Guid.NewGuid().ToString();
		public string Type = "STF.Node";
		public string Origin;
	}
}

#endif
