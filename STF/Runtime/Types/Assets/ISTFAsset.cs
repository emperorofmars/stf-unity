
using System;
using UnityEngine;
using STF_Util;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Unity.Collections;
using STF.Util;
using STF.Serialisation;

namespace STF.Types
{
	public abstract class ISTFAsset : MonoBehaviour, ISTFType
	{
		public abstract string Type { get; }
		public string Id { get => _Id; set => _Id = value; }
		[Id] public string _Id = System.Guid.NewGuid().ToString();
		public string STFName { get => _STFName; set => _STFName = value; }
		public string _STFName;

		public string Version;
		public string Author;
		public string URL;
		public string License;
		public string LicenseLink;
		public Texture2D Preview;

		public string OriginalFileName;
		public bool Degraded = false;

		[SerializeField, ReadOnly] public STFResourceMeta ImportMeta = new(); // TODO: actually build this metaobject on import

		[Serializable] public class AppliedAddonMeta { public string AddonId; public STFResourceMeta AddonMeta = new(); }
		[ReadOnly] public List<AppliedAddonMeta> AppliedAddonMetas = new();

		public void ParseDefaultValuesFromJson(STFImportState State, JObject JsonAsset, RefDeserializer Rf)
		{
			Id = (string)JsonAsset["id"];
			STFName = (string)JsonAsset["name"];
			Version = (string)JsonAsset["version"];
			Author = (string)JsonAsset["author"];
			URL = (string)JsonAsset["url"];
			License = (string)JsonAsset["license"];
			LicenseLink = (string)JsonAsset["license_link"];

			if(JsonAsset["preview"] != null)
			{
				Preview = (Texture2D)State.Resources[Rf.ResourceRef(JsonAsset["preview"])].Resource;
			}
		}

		public (JObject, RefSerializer) SerializeDefaultValuesToJson(STFExportState State)
		{
			var ret = new JObject
			{
				{"id", Id},
				{"type", Type},
				{"name", STFName},
				{"version", Version},
				{"author", Author},
				{"url", URL},
				{"license", License},
				{"license_link", LicenseLink}
			};
			var rf = new RefSerializer(ret);

			if(Preview) ret.Add("preview", rf.ResourceRef(ExportUtil.SerializeResource(State, Preview)));

			return (ret, rf);
		}
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
