using System;
using System.Collections.Generic;
using stf.serialisation;
using UnityEngine;

namespace stf
{
	public class STFMeta : ScriptableObject
	{
		
		[System.Serializable]
		public class ResourceInfo
		{
			public bool external;
			public UnityEngine.Object resource;
			public string assetPath;
			public string originalFormat;
			public string uuid;
			public string name;
		}
		
		public string mainAsset;
		public List<ResourceInfo> resourceInfo = new List<ResourceInfo>();
	}
}
