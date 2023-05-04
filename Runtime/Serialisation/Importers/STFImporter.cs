
using UnityEngine;
using System.Collections.Generic;
using stf.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;

namespace stf.serialisation
{
	public class STFImporter : ISTFImporter
	{
		private STFImportContext context;
		public string mainAssetId;
		public Dictionary<string, ISTFAsset> assets = new Dictionary<string, ISTFAsset>();
		public Dictionary<string, string> assetNodes = new Dictionary<string, string>();
		public Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
		public Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();
		public Dictionary<string, byte[]> buffers = new Dictionary<string, byte[]>();
		private List<Task> tasks = new List<Task>();
		private List<Task> componentTasks = new List<Task>();

		public STFImporter(JObject jsonRoot)
		{
			this.context = STFRegistry.GetDefaultImportContext();
			parse(jsonRoot);
		}

		public STFImporter(byte[] byteArray)
		{
			this.context = STFRegistry.GetDefaultImportContext();
			parse(byteArray);
		}

		public STFImporter(JObject jsonRoot, STFImportContext context)
		{
			this.context = context;
			if(this.context == null) STFRegistry.GetDefaultImportContext();
			parse(jsonRoot);
		}

		public void AddTask(Task task)
		{
			tasks.Add(task);
		}

		public GameObject GetNode(string id)
		{
			return nodes[id];
		}

		public Dictionary<string, ISTFAsset> GetAssets()
		{
			return assets;
		}

		public UnityEngine.Object GetResource(string id)
		{
			return resources[id];
		}

		public List<UnityEngine.Object> GetResources()
		{
			return resources.Values.ToList();
		}

		public byte[] GetBuffer(string id)
		{
			return buffers[id];
		}

		public void AddNode(string id, GameObject go)
		{
			this.nodes.Add(id, go);
		}

		public void parse(JObject jsonRoot)
		{
			foreach(var jsonAsset in ((JObject)jsonRoot["assets"]))
			{
				if(!context.AssetImporters.ContainsKey((string)jsonAsset.Value["type"]))
					throw new Exception("Assettype '" + (string)jsonAsset.Value["type"] + "' is not supported");

				var asset = context.AssetImporters[(string)jsonAsset.Value["type"]].ParseFromJson(this, jsonAsset.Value, jsonAsset.Key, jsonRoot);
				assets.Add(jsonAsset.Key, asset);
			}
			foreach(var jsonResource in ((JObject)jsonRoot["resources"]))
			{
				if((string)jsonResource.Value["type"] != null && context.ResourceImporters.ContainsKey((string)jsonResource.Value["type"]))
				{
					var resourceImporter = context.ResourceImporters[(string)jsonResource.Value["type"]];
					var resource = resourceImporter.parseFromJson(this, jsonResource.Value, jsonResource.Key);
					resources.Add(jsonResource.Key, resource);
				}
				// unrecognized resource
			}
			foreach(var jsonNode in ((JObject)jsonRoot["nodes"]))
			{
				var go = nodes[jsonNode.Key];

				var uuidComponent = go.AddComponent<STFUUID>();
				uuidComponent.id = jsonNode.Key;

				if((JObject)jsonNode.Value["components"] != null)
				{
					componentTasks.Add(new Task(() => {
						foreach(var jsonComponent in (JObject)jsonNode.Value["components"])
						{
							if((string)jsonComponent.Value["type"] != null && context.ComponentImporters.ContainsKey((string)jsonComponent.Value["type"]))
							{
								var componentImporter = context.ComponentImporters[(string)jsonComponent.Value["type"]];
								componentImporter.parseFromJson(this, jsonComponent.Value, jsonComponent.Key, go);
							}
							else
							{
								var unrecognizedComponent = (STFUnrecognizedComponent)go.AddComponent<STFUnrecognizedComponent>();
								unrecognizedComponent.id = jsonComponent.Key;
								unrecognizedComponent.parseFromJson(this, jsonComponent.Value);
							}
						}
					}));
				}
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
			}
			tasks.Clear();
			foreach(var task in componentTasks)
			{
				task.RunSynchronously();
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
			}
		}

		public void parse(byte[] byteArray)
		{
			var offset = 0;

			// Magic
			int magicLen = Encoding.UTF8.GetBytes(STFExporter._MAGIC).Length;
			var magicUtf8 = new byte[magicLen];
			Buffer.BlockCopy(byteArray, 0, magicUtf8, 0, magicUtf8.Length * sizeof(byte));
			offset += magicUtf8.Length * sizeof(byte);

			var magic = Encoding.UTF8.GetString(magicUtf8);
			if(magic != STFExporter._MAGIC)
				throw new Exception("Not an STF file, invalid magic number.");
			
			// Version
			int version = BitConverter.ToInt32(byteArray, offset);
			offset += sizeof(int);
			
			// Header Length
			int headerLen = BitConverter.ToInt32(byteArray, offset);
			offset += sizeof(int);

			// Buffer Lengths
			var bufferLengths = new int[headerLen / sizeof(int)];
			for(int i = 0; i < headerLen / sizeof(int); i++)
			{
				bufferLengths[i] = BitConverter.ToInt32(byteArray, offset);
				offset += sizeof(int);
			}

			// Validity Check
			if(bufferLengths.Length < 1)
				throw new Exception("Invalid file: At least one buffer needed.");
			var totalLengthCheck = offset;
			foreach(var l in bufferLengths) totalLengthCheck += l;
			if(totalLengthCheck != byteArray.Length)
				throw new Exception("Invalid file: Size of buffers doesn't line up with total file size. ( calculated: " + totalLengthCheck + " | actual: " + byteArray.Length + " )");

			// First buffer, the Json definition
			var json = Encoding.UTF8.GetString(byteArray, offset, bufferLengths[0]);
			offset += bufferLengths[0];

			JObject jsonRoot = JObject.Parse(json);
			if(((JArray)jsonRoot["buffers"]).Count() != bufferLengths.Count() - 1)
				throw new Exception("Invalid file: Mismatching number of defined buffers and actual buffer count.");

			for(int i = 1; i < bufferLengths.Count(); i++)
			{
				var buffer = new byte[bufferLengths[i]];
				Buffer.BlockCopy(byteArray, offset, buffer, 0, bufferLengths[i]);
				offset += bufferLengths[i];
				buffers.Add((string)jsonRoot["buffers"][i - 1], buffer);
			}

			parse(jsonRoot);
		}
	}
}