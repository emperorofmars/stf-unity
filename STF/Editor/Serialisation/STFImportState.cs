
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public class STFImportState : ISTFImportState
	{
		string TargetLocation;

		public STFImportState(STFImportContext Context, string TargetLocation, JObject JsonRoot)
		{
			this.Context = Context;
			this.TargetLocation = TargetLocation;
			this.JsonRoot = JsonRoot;
			this.AssetId = (string)JsonRoot["asset"]["id"];
		}

		override public void AddNode(GameObject Node, string Id)
		{
			Nodes.Add(Id, Node);
			AddTrash(Node);
		}

		override public void SaveSubResource(Object Component, Object Resource)
		{
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			Debug.Assert(assetPath != null);
			AssetDatabase.AddObjectToAsset(Component, assetPath);
			AssetDatabase.ImportAsset(assetPath);
			AssetDatabase.Refresh();
		}

		override public void SaveResource(Object Resource, string FileExtension, string Id)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + Id + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AddResource(Resource, Id);
			AssetDatabase.Refresh();
		}
		override public void SaveResource<T>(Object Resource, string FileExtension, T Meta, string Id)
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
		override public void SaveResource<T>(GameObject Resource, T Meta, string Id)
		{
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + ".Prefab");
			var saved = PrefabUtility.SaveAsPrefabAsset(Resource, location);
			Meta.Resource = saved;
			Meta.ResourceLocation = location;
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			AddResource(Meta, Id);
			AssetDatabase.Refresh();
		}
		override public void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Meta.Name + "_" + Id + FileExtension);
			AssetDatabase.CreateAsset(Meta, Path.ChangeExtension(location, "Asset"));
			File.WriteAllBytes(location, Resource);
			Meta.ResourceLocation = location;
			AssetDatabase.Refresh();
			Meta.Resource = AssetDatabase.LoadAssetAtPath<R>(location);
			AddResource(Meta, Id);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		override public T SaveAndLoadResource<T>(byte[] Resource, string Name, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Name + FileExtension);
			File.WriteAllBytes(location, Resource);
			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath<T>(location);
		}
		override public void SaveResourceBelongingToId(Object Resource, string FileExtension, string OwnerId)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + "_" + OwnerId + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
		}
		override public void SaveGeneratedResource(Object Resource, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			var location = Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName, Resource.name + FileExtension);
			AssetDatabase.CreateAsset(Resource, location);
			AssetDatabase.Refresh();
		}
		
		override public Object Instantiate(Object Resource)
		{
			if(Resource is GameObject) return PrefabUtility.InstantiatePrefab(Resource);
			else return Object.Instantiate(Resource);
		}
	}
}

#endif
