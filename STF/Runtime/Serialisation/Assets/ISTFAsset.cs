
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using STF_Util;

namespace STF.Serialisation
{
	public abstract class ISTFAsset : MonoBehaviour
	{
		[Id] public string _Id = Guid.NewGuid().ToString();
		public string Id {get => _Id; set => _Id = value;}
		public abstract string Type { get; }
		public string Name;
		public string Version;
		public string Author;
		public string URL;
		public string License;
		public string LicenseLink;
		public Texture2D Preview;
	}
}

#endif
