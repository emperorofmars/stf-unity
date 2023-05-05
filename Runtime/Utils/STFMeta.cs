using System;
using System.Collections.Generic;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	public class STFMeta : ScriptableObject
	{
		public string mainAsset;
		public Dictionary<Component, string> componentIds = new Dictionary<Component, string>();
	}
}
