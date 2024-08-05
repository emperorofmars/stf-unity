
using Newtonsoft.Json.Linq;

namespace STF.Serialisation
{
	public class RefUtil
	{
		public static JObject CreateNodeReference(string Id)
		{
			return new JObject
			{
				"type", "STF.ref_node",
				"target", Id,
			};
		}
		public static JObject CreateNodeComponentReference(string NodeId, string ComponentId)
		{
			return new JObject
			{
				"type", "STF.ref_node_component",
				"node", NodeId,
				"target", ComponentId,
			};
		}
		public static JObject CreateResourceReference(string Id)
		{
			return new JObject
			{
				"type", "STF.ref_resource",
				"target", Id,
			};
		}
		public static JObject CreateResourceComponentReference(string ResourceId, string ComponentId)
		{
			return new JObject
			{
				"type", "STF.ref_resource_component",
				"resource", ResourceId,
				"target", ComponentId,
			};
		}
	}
}
