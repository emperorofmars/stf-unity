
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

		public STFSecondStageAsset(GameObject rootNode, string id, string name)
		{
			this.rootNode = rootNode;
			this.id = id;
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
			return "asset";
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
