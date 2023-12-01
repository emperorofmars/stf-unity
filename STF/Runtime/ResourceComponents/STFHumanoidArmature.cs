using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	[System.Serializable]
	public class STFHumanoidArmature : ISTFResourceComponent
	{
		public const string _TYPE = "STF.armature.humanoid";
		public override string Type => _TYPE;
	}
	
	public class STFHumanoidArmatureExporter : ISTFResourceComponentExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new System.NotImplementedException();
		}

		public (string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, ISTFResourceComponent Component)
		{
			var ret = new JObject {
				{"type", STFHumanoidArmature._TYPE},
			};
			return (Component.Id, ret);
		}
	}
	
	public class STFHumanoidArmatureImporter : ISTFResourceComponentImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id, ISTFResource Resource)
		{
			var ret = new STFHumanoidArmature {
				Id = Id,
			};
			Resource.Components.Add(ret);
		}
	}
}