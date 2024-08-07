
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace STF.Serialisation
{
	public class RefDeserializer
	{
		JObject References;
		public RefDeserializer(JObject Json)
		{
			References = (JObject)Json[STFKeywords.Keys.References];
		}

		public string Ref(string ObjectType, int Idx)
		{
			if(References.ContainsKey(ObjectType))
			{
				return (string)((JArray)References[ObjectType])[Idx];
			}
			else throw new Exception("Invalid Reference Index!");
		}

		public string NodeRef(int Idx) { return Ref(STFKeywords.ObjectType.Nodes, Idx); }
		public string NodeComponentRef(int Idx) { return Ref(STFKeywords.ObjectType.NodeComponents, Idx); }
		public string ResourceRef(int Idx) { return Ref(STFKeywords.ObjectType.Resources, Idx); }
		public string ResourceComponentRef(int Idx) { return Ref(STFKeywords.ObjectType.ResourceComponents, Idx); }
		public string BufferRef(int Idx) { return Ref(STFKeywords.ObjectType.Buffers, Idx); }

		public string NodeRef(JToken Idx) { return Ref(STFKeywords.ObjectType.Nodes, (int)Idx); }
		public string NodeComponentRef(JToken Idx) { return Ref(STFKeywords.ObjectType.NodeComponents, (int)Idx); }
		public string ResourceRef(JToken Idx) { return Ref(STFKeywords.ObjectType.Resources, (int)Idx); }
		public string ResourceComponentRef(JToken Idx) { return Ref(STFKeywords.ObjectType.ResourceComponents, (int)Idx); }
		public string BufferRef(JToken Idx) { return Ref(STFKeywords.ObjectType.Buffers, (int)Idx); }

		public List<string> Refs(string ObjectType) { return References[ObjectType].ToObject<List<string>>(); }
		public List<string> NodeRefs() { return Refs(STFKeywords.ObjectType.Nodes); }
		public List<string> NodeComponentRefs() { return Refs(STFKeywords.ObjectType.NodeComponents); }
		public List<string> ResourceRefs() { return Refs(STFKeywords.ObjectType.Resources); }
		public List<string> ResourceComponentRefs() { return Refs(STFKeywords.ObjectType.ResourceComponents); }
		public List<string> BufferRefs() { return Refs(STFKeywords.ObjectType.Buffers); }
	}
}