
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using STF.Util;
using STF.Serialisation;

namespace STF.Types
{
	public class STFBuffer : ScriptableObject
	{
		public string Id;
		[HideInInspector] public byte[] Data;
	}
}