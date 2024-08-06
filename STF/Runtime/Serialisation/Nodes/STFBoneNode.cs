
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFBoneNode : ISTFNode
	{
		public const string _TYPE = "STF.bone";
		public override string Type => _TYPE;
	}

	// These need to exist for the property path translation
	public class DontInvokeNodeExporter : ASTFNodeExporter
	{
		public override string SerializeToJson(ISTFExportState State, GameObject Go)
		{
			throw new System.Exception($"This method should never be invoked for type: {STFBoneNode._TYPE}");
		}
	}

	public class DontInvokeNodeImporter : ASTFNodeImporter
	{
		public override GameObject ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id)
		{
			throw new System.Exception($"This method should never be invoked for type: {STFBoneNode._TYPE}");
		}
	}
}
