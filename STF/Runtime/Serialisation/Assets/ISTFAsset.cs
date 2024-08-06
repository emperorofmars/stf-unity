
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

		public string OriginalFileName;

		[SerializeField]
		public STFResourceMeta ImportMeta = new STFResourceMeta();

		[Serializable] public class AppliedAddonMeta { public string AddonId; public STFResourceMeta AddonMeta = new STFResourceMeta(); }
		public List<AppliedAddonMeta> AppliedAddonIds = new List<AppliedAddonMeta>();
	}
	
	public interface ISTFAssetExporter
	{
		JObject SerializeToJson(STFExportState State, ISTFAsset Asset);
	}
	
	public interface ISTFAssetImporter
	{
		ISTFAsset ParseFromJson(STFImportState State, JObject JsonAsset);
	}
}
