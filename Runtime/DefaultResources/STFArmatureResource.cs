

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
			public Vector3 localPosition;
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

		/*public Transform ConstructTransformHirarchy()
		{
			var transforms = new Dictionary<string, Transform>();
			foreach(var bone in bones)
			{
				var go = new GameObject();
				
			}
			return transforms[rootId];
		}*/
	}
}
