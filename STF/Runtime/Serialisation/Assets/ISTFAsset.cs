
using System;
using UnityEngine;
using STF_Util;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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

		public string ImportPath;
		public List<string> AppliedAddonIds;
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
