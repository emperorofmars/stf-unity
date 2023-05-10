
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFAssetExporter
	{
		void Convert(ISTFExporter state);
		string GetId(ISTFExporter state);
		JToken SerializeToJson(ISTFExporter state);
	}

	public interface ISTFAsset
	{
		string getId();
		string GetSTFAssetType();
		Type GetUnityAssetType();
        UnityEngine.Object GetAsset();
	}
	
	public interface ISTFAssetImporter
	{
		ISTFAsset ParseFromJson(ISTFImporter state, JToken jsonAsset, string id, JObject jsonRoot);
	}
}
