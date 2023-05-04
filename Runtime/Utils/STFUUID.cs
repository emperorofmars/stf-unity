using System;
using System.Collections.Generic;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	public class STFUUID : MonoBehaviour
	{
		public String id;
		public Dictionary<Component, string> componentIds = new Dictionary<Component, string>();
	}
}
