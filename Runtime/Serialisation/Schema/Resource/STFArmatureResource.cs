

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
			public List<string> children = new List<string>();
		}
		public string id;
		public string armatureName;
		public string rootId;
		public Vector3 armaturePosition;
		public Quaternion armatureRotation;
		public Vector3 armatureScale;
		public List<Bone> bones = new List<Bone>();

		public STFArmature armatureTransforms;
	}
}
