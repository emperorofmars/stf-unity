
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace STF.Util
{
	public class RefDeserializer
	{
		readonly JObject References;
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

		public string NodeRef(int Idx) => Ref(STFKeywords.ObjectType.Nodes, Idx);
		public string NodeComponentRef(int Idx) => Ref(STFKeywords.ObjectType.NodeComponents, Idx);
		public string ResourceRef(int Idx) => Ref(STFKeywords.ObjectType.Resources, Idx);
		public string ResourceComponentRef(int Idx) => Ref(STFKeywords.ObjectType.ResourceComponents, Idx);
		public string BufferRef(int Idx) => Ref(STFKeywords.ObjectType.Buffers, Idx);

		public string NodeRef(JToken Idx) => Idx != null ? Ref(STFKeywords.ObjectType.Nodes, (int)Idx) : null;
		public string NodeComponentRef(JToken Idx) => Idx != null ? Ref(STFKeywords.ObjectType.NodeComponents, (int)Idx) : null;
		public string ResourceRef(JToken Idx) => Idx != null ? Ref(STFKeywords.ObjectType.Resources, (int)Idx) : null;
		public string ResourceComponentRef(JToken Idx) => Idx != null ? Ref(STFKeywords.ObjectType.ResourceComponents, (int)Idx) : null;
		public string BufferRef(JToken Idx) => Idx != null ? Ref(STFKeywords.ObjectType.Buffers, (int)Idx) : null;

		public List<string> Refs(string ObjectType) { return References[ObjectType]?.ToObject<List<string>>(); }
		public List<string> NodeRefs() { return Refs(STFKeywords.ObjectType.Nodes); }
		public List<string> NodeComponentRefs() { return Refs(STFKeywords.ObjectType.NodeComponents); }
		public List<string> ResourceRefs() { return Refs(STFKeywords.ObjectType.Resources); }
		public List<string> ResourceComponentRefs() { return Refs(STFKeywords.ObjectType.ResourceComponents); }
		public List<string> BufferRefs() { return Refs(STFKeywords.ObjectType.Buffers); }
	}
}