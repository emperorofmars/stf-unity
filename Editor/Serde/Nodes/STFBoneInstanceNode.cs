
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serde
{
	public class STFBoneInstanceNode : ASTFNode
	{
		public static string _TYPE => "STF.bone_instance";
		public override string Type => _TYPE;
		public string BoneId;
	}
}

#endif
