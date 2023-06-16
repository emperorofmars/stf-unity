
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFAsset
	{
		string getId();
		string GetSTFAssetType();
		Type GetUnityAssetType();
		string GetSTFAssetName();
        UnityEngine.Object GetAsset();
		bool isNodeInAsset(string id);
	}

	public interface ISTFAssetExporter
	{
		void Convert(ISTFExporter state);
		string GetId(ISTFExporter state);
		JToken SerializeToJson(ISTFExporter state);
	}
	
	public interface ISTFAssetImporter
	{
		ISTFAsset ParseFromJson(ISTFImporter state, JToken jsonAsset, string id, JObject jsonRoot);
	}
}
