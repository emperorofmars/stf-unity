
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;
using MTF;
using System.Drawing;

namespace STF.Serialisation
{
	// Temporary fix for bitmap bullshit
	public class MTFPropertyValueImportState : MTF.IPropertyValueImportState
	{
		ISTFImportState State;
		public MTFPropertyValueImportState(ISTFImportState State)
		{
			this.State = State;
		}

		public UnityEngine.Object GetResource(string Id)
		{
			var r = State.Resources[Id];
			if(r is ISTFResource resource)
			{
				return State.LoadResource(resource);
			}
			return r;
		}
	}
	public class MTFMaterialConvertState : MTF.IMaterialConvertState
	{
		ISTFImportState State;
		public MTFMaterialConvertState(ISTFImportState State)
		{
			this.State = State;
		}
		public Bitmap LoadTextureAsBitmap(Texture2D Texture)
		{
			var texturePropertyPath = AssetDatabase.GetAssetPath(Texture);
			return new Bitmap(texturePropertyPath);
		}

		public Texture2D SaveBitmapTexture(Bitmap Bitmap, string Name)
		{
			var savePath = Path.Combine(State.TargetLocation, STFConstants.ResourceDirectoryName, Path.GetFileNameWithoutExtension(Name));
			Bitmap.Save(savePath + ".png", System.Drawing.Imaging.ImageFormat.Png);
			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath + ".png");
		}
	}

	public class STFImportState : ISTFImportState
	{
		STFImportContext _Context;
		public STFImportContext Context {get =>_Context;}
		string _TargetLocation;
		public string TargetLocation {get =>_TargetLocation;}
		string _MainAssetId;
		public string MainAssetId {get => _MainAssetId;}
		JObject _JsonRoot;
		public JObject JsonRoot {get => _JsonRoot;}
		Dictionary<string, STFAsset> _Assets = new Dictionary<string, STFAsset>();
		public Dictionary<string, STFAsset> Assets {get => _Assets;}
		Dictionary<string, UnityEngine.Object> _Resources = new Dictionary<string, UnityEngine.Object>();
		public Dictionary<string, UnityEngine.Object> Resources {get => _Resources;}
		Dictionary<string, byte[]> _Buffers = new Dictionary<string, byte[]>();
		public Dictionary<string, byte[]> Buffers {get => _Buffers;}

		public IPropertyValueImportState MTFPropertyValueImportState => new MTFPropertyValueImportState(this);

		public IMaterialConvertState MTFMaterialConvertState => new MTFMaterialConvertState(this);

		// stuff to delete before the import finishes
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		public List<Task> Tasks = new List<Task>();

		public STFImportState(STFImportContext Context, string TargetLocation, JObject JsonRoot)
		{
			this._Context = Context;
			this._TargetLocation = TargetLocation;
			this._JsonRoot = JsonRoot;
			this._MainAssetId = (string)JsonRoot["main"];
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public void AddResource(UnityEngine.Object Resource, string Id)
		{
			Resources.Add(Id, Resource);
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.Trash.Add(Trash);
		}

		public void SaveResource(UnityEngine.Object Resource, string FileExtension, string Id)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + Id + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AddResource(Resource, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(UnityEngine.Object Resource, string FileExtension, T Meta, string Id) where T: UnityEngine.Object, ISTFResource
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			Meta.Resource = Resource;
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: UnityEngine.Object, ISTFResource
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + ".Prefab");
			var saved = PrefabUtility.SaveAsPrefabAsset(Resource, location);
			Meta.Resource = saved;
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResource<T>(byte[] Resource, string FileExtension, T Meta, string Id) where T: UnityEngine.Object, ISTFResource
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + FileExtension);
			File.WriteAllBytes(location, Resource);
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		public void SaveResourceBelongingToId(UnityEngine.Object Resource, string FileExtension, string OwnerId)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + OwnerId + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
		}

		public Object LoadResource(ISTFResource Resource)
		{
			if(Resource.Resource != null) return Resource.Resource;
			else if(Resource.ResourceLocation != null && Resource.ResourceLocation.Length > 0) return AssetDatabase.LoadAssetAtPath<Object>(Resource.ResourceLocation);
			throw new System.Exception("Error retrieving resource: " + Resource);
		}
		
		public UnityEngine.Object Instantiate(UnityEngine.Object Resource)
		{
			return PrefabUtility.InstantiatePrefab(Resource);
		}
		
		public void SaveAsset(GameObject Root, string Name, bool Main = false)
		{
			var path = Main ? Path.Combine(TargetLocation, Name + ".Prefab") : Path.Combine(TargetLocation, STFConstants.SecondaryAssetsDirectoryName, Name + ".Prefab");
			PrefabUtility.SaveAsPrefabAsset(Root, path);
		}
	}
}

#endif
