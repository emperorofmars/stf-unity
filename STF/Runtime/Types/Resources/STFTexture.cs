
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using STF.Util;
using STF.Serialisation;
using System.Threading.Tasks;
using UnityEngine.Experimental.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace STF.Types
{
	public class STFTexture : ISTFResource
	{
		public const string _TYPE = "STF.texture";
		public override string Type => _TYPE;

		public string TextureType = "color"; // linear | normal
		public Vector2Int TextureSize;
		//public string OriginalBufferId;
		public ISTFBuffer Buffer;
	}

	public class STFTexture2dExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(STFExportState State, UnityEngine.Object Resource, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(STFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			var texture = (Texture2D)Resource;
			var (arrayBuffer, meta, fileName) = State.UnityContext.LoadAsset<STFTexture>(texture);

			var ret = new JObject {
				{ "type", STFTexture._TYPE },
				{ "name", !string.IsNullOrWhiteSpace(meta?.STFName) ? meta.STFName : Path.GetFileNameWithoutExtension(fileName) },
				{ "image_format", Path.GetExtension(fileName).Remove(0, 1) },
				{ "texture_width", meta?.TextureSize != null ? meta.TextureSize.x : texture.width },
				{ "texture_height", meta?.TextureSize != null ? meta.TextureSize.y : texture.height },
			};
			var rf = new RefSerializer(ret);

#if UNITY_EDITOR
			TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
			switch(textureImporter.textureType)
			{
				case TextureImporterType.NormalMap: ret.Add("texture_type", "normal"); break;
				default: ret.Add("texture_type", "color"); break;
			}
#else
			if(meta != null && meta.TextureType != null && meta.TextureType.Length >= 0) ret.Add("texture_type", meta.TextureType);
			else ret.Add("texture_type", texture.graphicsFormat.ToString().ToLower().EndsWith("unorm") ? "linear" : "color");
#endif

			if(meta != null && meta.Buffer != null) ret.Add("buffer", rf.BufferRef(State.AddBuffer(meta.Buffer.GetData(), meta.Buffer.Id)));
			else ret.Add("buffer", rf.BufferRef(State.AddBuffer(arrayBuffer)));

			// serialize resource components
			ret.Add("components", ExportUtil.SerializeResourceComponents(State, meta));

			if(meta.Fallback.IsRef) ret.Add("fallback", rf.ResourceRef(ExportUtil.SerializeResource(State, meta.Fallback.Ref)));

			return State.AddResource(Resource, ret, meta ? meta.Id : Guid.NewGuid().ToString());
		}
	}

	public class STFTextureImporter : ISTFResourceImporter
	{
		public string ConvertPropertyPath(STFImportState State, UnityEngine.Object Resource, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(STFImportState State, JObject Json, string Id)
		{
			var meta = ScriptableObject.CreateInstance<STFTexture>();
			meta.Id = Id;
			meta.STFName = (string)Json["name"];
			meta.name = meta.STFName + "_" + Id;
			meta.TextureType = (string)Json["texture_type"];

			var rf = new RefDeserializer(Json);
			State.AddTask(new Task(() => meta.Fallback = State.GetResourceReference(rf.ResourceRef(Json["fallback"]))));

			if(Json["texture_width"] != null && Json["texture_height"] != null) meta.TextureSize = new Vector2Int((int)Json["texture_width"], (int)Json["texture_height"]);

			var bufferId = rf.BufferRef(Json["buffer"]);
			meta.Buffer = ScriptableObject.CreateInstance<STFBuffer>();
			
			(UnityEngine.Object textureResource, ISTFBuffer textureBuffer) = State.UnityContext.SaveGeneratedResource(State.Buffers[bufferId], meta.name, (string)Json["image_format"], bufferId);
			meta.Resource = textureResource;
			meta.Buffer = textureBuffer;
			State.AddResource(meta);
			ImportUtil.ParseResourceComponents(State, meta, Json);
			return;
		}
	}
}
