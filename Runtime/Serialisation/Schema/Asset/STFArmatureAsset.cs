
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace stf.serialisation
{
	public class STFArmatureAsset
	{
		public string id;
		public Transform armatureRootTransform;
		public Transform rootBone;
		public Transform[] bones;

		public float calculateArmatureDeviation(Mesh mesh, Transform meshTransform)
		{
			float diffPosition = 0;
			float diffRotation = 0;
			float diffScale = 0;
			for(int i = 0; i < bones.Length; i++)
			{
				var armatureBindpose = bones[i].worldToLocalMatrix * armatureRootTransform.localToWorldMatrix;
				var meshBindpose = mesh.bindposes[i];

				diffPosition += ((Vector3)armatureBindpose.GetColumn(3) - (Vector3)meshBindpose.GetColumn(3)).magnitude;
				diffRotation += (armatureBindpose.rotation.eulerAngles - meshBindpose.rotation.eulerAngles).magnitude;
				float tmpScale = (armatureBindpose.lossyScale - meshBindpose.lossyScale).magnitude - 0.02f;
				diffScale += Math.Max(tmpScale, 0);
			}

			return diffPosition + diffRotation + diffScale;
		}
	}
}
