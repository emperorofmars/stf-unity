
namespace STF.Serialisation
{
	public struct STFKeywords
	{
		public struct ObjectType
		{
			public static string Asset = "asset";
			public static string Nodes = "nodes";
			public static string NodeComponents = "node_components";
			public static string Resources = "resources";
			public static string ResourceComponents = "resource_components";
			public static string Buffers = "buffers";
		}
		public struct ReferenceType
		{
		}
		public struct Keys
		{
			public static string Id = "id";
			public static string Type = "type";
			public static string Name = "name";
			public static string References = "references";
			public static string Asset = ObjectType.Asset;
			public static string Node = ObjectType.Nodes;
			public static string NodeComponent = ObjectType.NodeComponents;
			public static string Resource = ObjectType.Resources;
			public static string ResourceComponent = ObjectType.ResourceComponents;
			public static string Buffer = ObjectType.Buffers;
		}
	}
}
