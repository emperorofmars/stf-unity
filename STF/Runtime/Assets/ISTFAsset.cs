
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public abstract class ISTFAsset : MonoBehaviour
	{
		public string _Id = Guid.NewGuid().ToString();
		public string Id {get => _Id; set => _Id = value;}
		public abstract string Type { get; }
		public string Name { get; set; }
		public string Version { get; set; }
		public string Author { get; set; }
		public string URL { get; set; }
		public string License { get; set; }
		public string LicenseLink { get; set; }
		public Texture2D Preview { get; set; }
	}
	
	public interface ISTFAssetImportState
	{
		string AssetId {get;}

		STFImportContext Context {get;}
		//ISTFAsset Asset {get;}
		JObject JsonRoot {get;}
		// id -> asset
		//Dictionary<string, STFAsset> Assets {get;}

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
		string SerializeToJson(ISTFExportState State, ISTFAsset Asset);
	}
	
	public interface ISTFAssetImporter
	{
		void ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id);
	}
}

#endif
