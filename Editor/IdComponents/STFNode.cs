
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace STF.IdComponents
{
	public class STFNode : MonoBehaviour
	{
		public string nodeId = Guid.NewGuid().ToString();
		public string type = "STF.Node";
		public string origin;
	}
}

#endif
