
using System;
using UnityEngine;
using STF_Util;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Unity.Collections;
using STF.Util;
using STF.Types;

namespace STF.Serialisation
{
	public abstract class ISTFAsset : MonoBehaviour, ISTFType
	{
		public abstract string Type { get; }
		public string Id { get => _Id; set => _Id = value; }
		[Id] public string _Id = System.Guid.NewGuid().ToString();
		public string Name { get => _Name; set => _Name = value; }
		public string _Name;

		public string Version;
		public string Author;
		public string URL;
		public string License;
		public string LicenseLink;
		public Texture2D Preview;

		public string OriginalFileName;
		public bool Degraded = false;

		[SerializeField, ReadOnly] public STFResourceMeta ImportMeta = new STFResourceMeta(); // TODO: actually build this metaobject on import

		[Serializable] public class AppliedAddonMeta { public string AddonId; public STFResourceMeta AddonMeta = new STFResourceMeta(); }
		[ReadOnly] public List<AppliedAddonMeta> AppliedAddonMetas = new List<AppliedAddonMeta>();
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
