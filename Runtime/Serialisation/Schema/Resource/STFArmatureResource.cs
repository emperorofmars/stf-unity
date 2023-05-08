

using System;
using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	public class STFArmatureResource : ScriptableObject
	{
		[Serializable]
		public class Bone
		{
			public string id;
			public string name;
			public Vector3 loacalPosition;
			public Quaternion localRotation;
			public Vector3 localScale;
			public List<string> children;
		}
		public string id;
		public string armatureName;
		public string rootId;
		public Vector3 rootPosition;
		public Quaternion rootRotation;
		public Vector3 rootScale;
		public List<Bone> bones;
	}
}
