
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace STF.Serialisation
{
	public interface ISTFNode
	{
		string Type {get;}
		string NodeId {get; set;}
		string Origin {get; set;}
	}
	public abstract class ASTFNode : MonoBehaviour, ISTFNode
	{
		public abstract string Type { get; }
		public string _NodeId = Guid.NewGuid().ToString();
		public string NodeId {get => _NodeId; set => _NodeId = value;}
		public string _Origin;
		public string Origin {get => _Origin; set => _Origin = value;}
	}

	public interface ISTFNodeExporter
	{
		string SerializeToJson(ISTFExportState State, GameObject Go);
	}

	public interface ISTFNodeImporter
	{
		GameObject ParseFromJson(ISTFAssetImportState State, JObject JsonAsset, string Id);
	}
}
