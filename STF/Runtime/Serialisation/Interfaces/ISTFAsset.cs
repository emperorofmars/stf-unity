
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public interface ISTFAssetImportState
	{
		STFImportContext Context {get;}
		STFAssetInfo AssetInfo {get;}
		JObject JsonRoot {get;}
		// id -> asset
		Dictionary<string, STFAsset> Assets {get;}

		// id -> resource
		Dictionary<string, UnityEngine.Object> Resources  {get;}

		// id -> node
		Dictionary<string, GameObject> Nodes {get;}

		// id -> component
		Dictionary<string, Component> Components {get;}

		void AddTask(Task task);
		void AddNode(GameObject Node, string Id);
		void AddComponent(Component Node, string Id);
		void AddTrash(UnityEngine.Object Trash);
		UnityEngine.Object Instantiate(UnityEngine.Object Resource);
	}
	
	public interface ISTFAssetExporter
	{
		string SerializeToJson(ISTFExportState State, STFAsset Asset);
	}
	
	public interface ISTFAssetImporter
	{
		void ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id);
	}
}

#endif
