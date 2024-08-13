
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using STF_Util;
using STF.Types;

namespace STF.Serialisation
{
	public abstract class ISTFNode : MonoBehaviour, ISTFType
	{
		public abstract string Type { get; }
		public string Id { get => _Id; set => _Id = value; }
		[Id] public string _Id = System.Guid.NewGuid().ToString();
		public string Name { get => _Name; set => _Name = value; }
		public string _Name;
		
		public int _PrefabHirarchy = 0;
		public int PrefabHirarchy {get => _PrefabHirarchy; set => _PrefabHirarchy = value;}
		public string _Origin;
		public string Origin {get => _Origin; set => _Origin = value;}
	}

	public interface ISTFNodeExporter
	{
		string ConvertPropertyPath(string UnityProperty);
		string SerializeToJson(STFExportState State, GameObject Go);
	}

	public interface ISTFNodeImporter
	{
		string ConvertPropertyPath(string STFProperty);
		GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id);
	}

	public abstract class ASTFNodeExporter : ISTFNodeExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			if(UnityProperty.StartsWith("m_LocalPosition"))
			{
				if(UnityProperty.EndsWith("x")) return "translation.x";
				if(UnityProperty.EndsWith("y")) return "translation.y";
				if(UnityProperty.EndsWith("z")) return "translation.z";
			}
			else if(UnityProperty.StartsWith("m_LocalRotation"))
			{
				if(UnityProperty.EndsWith("x")) return "rotation.x";
				if(UnityProperty.EndsWith("y")) return "rotation.y";
				if(UnityProperty.EndsWith("z")) return "rotation.z";
				if(UnityProperty.EndsWith("w")) return "rotation.w";
			}
			else if(UnityProperty.StartsWith("m_LocalScale"))
			{
				if(UnityProperty.EndsWith("x")) return "scale.x";
				if(UnityProperty.EndsWith("y")) return "scale.y";
				if(UnityProperty.EndsWith("z")) return "scale.z";
			}
			throw new Exception("Unrecognized animation property: " + UnityProperty);
		}
		public abstract string SerializeToJson(STFExportState State, GameObject Go);
	}

	public abstract class ASTFNodeImporter : ISTFNodeImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			if(STFProperty.StartsWith("translation"))
			{
				if(STFProperty.EndsWith("x")) return "m_LocalPosition.x";
				if(STFProperty.EndsWith("y")) return "m_LocalPosition.y";
				if(STFProperty.EndsWith("z")) return "m_LocalPosition.z";
			}
			else if(STFProperty.StartsWith("rotation"))
			{
				if(STFProperty.EndsWith("x")) return "m_LocalRotation.x";
				if(STFProperty.EndsWith("y")) return "m_LocalRotation.y";
				if(STFProperty.EndsWith("z")) return "m_LocalRotation.z";
				if(STFProperty.EndsWith("w")) return "m_LocalRotation.w";
			}
			else if(STFProperty.StartsWith("scale"))
			{
				if(STFProperty.EndsWith("x")) return "m_LocalScale.x";
				if(STFProperty.EndsWith("y")) return "m_LocalScale.y";
				if(STFProperty.EndsWith("z")) return "m_LocalScale.z";
			}
			throw new Exception("Unrecognized animation property: " + STFProperty);
		}
		public abstract GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id);
	}
}
