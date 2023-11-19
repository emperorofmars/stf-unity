
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serde
{
	public class STFBoneNode : ASTFNode
	{
		public override string Type => "STF.bone";
	}
}

#endif
