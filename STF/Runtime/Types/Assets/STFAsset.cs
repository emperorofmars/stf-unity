
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using STF.Serialisation;
using STF.Util;

namespace STF.Types
{
	public class STFAsset : ISTFAsset
	{
		public const string _TYPE = "STF.asset";
		public override string Type => _TYPE;
		public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = new();
	}

	public class STFAssetExporter : ISTFAssetExporter
	{
		public JObject SerializeToJson(STFExportState State, ISTFAsset Asset)
		{
			var (ret, rf) = Asset.SerializeDefaultValuesToJson(State);
			ret.Add("root_node", rf.NodeRef(ExportUtil.SerializeNode(State, Asset.gameObject)));
			return ret;
		}
	}
	
	public class STFAssetImporter : ISTFAssetImporter
	{
		public ISTFAsset ParseFromJson(STFImportState State, JObject JsonAsset)
		{
			GameObject rootGo = null;
			try
			{
				var rf = new RefDeserializer(JsonAsset);
				rootGo = ImportUtil.ParseNode(State, rf.NodeRef(JsonAsset["root_node"]));
				
				var asset = rootGo.AddComponent<STFAsset>();
				asset.ParseDefaultValuesFromJson(State, JsonAsset, rf);
				return asset;
			}
			catch(Exception e)
			{
				if(rootGo != null) UnityEngine.Object.DestroyImmediate(rootGo);
				throw new Exception("Error during Asset import: ", e);
			}
		}
	}
}
