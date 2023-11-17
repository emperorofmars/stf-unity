
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
using STF.IdComponents;

namespace STF.Tools
{
	public class STFImportInfo : ScriptableObject
	{
		public List<STFAssetInfo> assets = new List<STFAssetInfo>();
	}
}

#endif
