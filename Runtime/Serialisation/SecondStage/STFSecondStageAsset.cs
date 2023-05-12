
using System;
using UnityEngine;

namespace stf.serialisation
{
	public class STFSecondStageAsset : ISTFAsset
	{
		ISTFImporter state;
		public string id;
		public string name;
		public GameObject rootNode;
		public string type;

		public STFSecondStageAsset(GameObject rootNode, string id, string name)
		{
			this.name = name;
			this.rootNode = rootNode;
			this.id = id;
			this.type = "unity";
		}

		public STFSecondStageAsset(GameObject rootNode, string id, string name, string type)
		{
			this.name = name;
			this.rootNode = rootNode;
			this.id = id;
			this.type = type;
		}

		public UnityEngine.Object GetAsset()
		{
			return rootNode;
		}

		public string GetSTFAssetName()
		{
			return name;
		}

		public string GetSTFAssetType()
		{
			return type;
		}

		public Type GetUnityAssetType()
		{
			return typeof(GameObject);
		}

		public string getId()
		{
			return id;
		}
	}
}
