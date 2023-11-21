
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using STF.Inspectors;

namespace STF.Serde
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

#endif
