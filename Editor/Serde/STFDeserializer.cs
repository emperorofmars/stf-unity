
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
using STF.IdComponents;

namespace STF.Serde
{
	public class STFImportState
	{

		public string targetLocation = "STF_Imports";
		public string filename;
		public string mainAssetId;

		// id -> asset
		public Dictionary<string, STFAsset> assets = new Dictionary<string, STFAsset>();

		// id -> node
		public Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();

		// id -> resource
		public Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();

		// id -> component
		public Dictionary<string, Component> components = new Dictionary<string, Component>();

		// id -> buffer
		public Dictionary<string, byte[]> buffers = new Dictionary<string, byte[]>();

		// stuff to delete before the import finishes
		private List<UnityEngine.Object> trash = new List<UnityEngine.Object>();
		private List<Task> tasks = new List<Task>();
	}

	// The main star for import!
	// Parses the binary file, extracts the JSON and parses it based on the provided importers from the STFImportContext.
	public class STFDeserializer
	{

	}
}

#endif
