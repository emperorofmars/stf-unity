
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class RefSerializer
	{
		JObject References = new JObject();
		public RefSerializer(JObject Json)
		{
			Json.Add(STFKeywords.Keys.References, References);
		}
		public RefSerializer() {}

		public int Ref(string ObjectType, string Id)
		{
			if(References.ContainsKey(ObjectType))
			{
				var node_arr = (JArray)References[ObjectType];
				lock (node_arr)
				{
					node_arr.FirstOrDefault(e => (string)e == Id);
					for(int i = 0; i < node_arr.Count; i++) if((string)node_arr[i] == Id) return i; // dont add the same id twice
					node_arr.Add(Id);
					return node_arr.Count - 1;
				}
			}
			else
			{
				lock (References)
				{
					References.Add(ObjectType, new JArray(Id));
					return 0;
				}
			}
		}

		public int NodeRef(string Id) { return Ref(STFKeywords.ObjectType.Nodes, Id); }
		public int NodeComponentRef(string Id) { return Ref(STFKeywords.ObjectType.NodeComponents, Id); }
		public int ResourceRef(string Id) { return Ref(STFKeywords.ObjectType.Resources, Id); }
		public int ResourceComponentRef(string Id) { return Ref(STFKeywords.ObjectType.ResourceComponents, Id); }
		public int BufferRef(string Id) { return Ref(STFKeywords.ObjectType.Buffers, Id); }

		public JObject GetReferences()
		{
			return new JObject { {STFKeywords.Keys.References, References} };
		}

		public void MergeInto(JObject Json)
		{
			Json.Add(STFKeywords.Keys.References, References);
		}
	}
}