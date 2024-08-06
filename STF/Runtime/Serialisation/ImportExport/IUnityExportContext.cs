
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public interface IUnityExportContext
	{
		T LoadMeta<T>(UnityEngine.Object Resource) where T: ISTFResource;
		(byte[] Data, T Meta, string Path) LoadAsset<T>(UnityEngine.Object Resource) where T: ISTFResource;
	}
}
