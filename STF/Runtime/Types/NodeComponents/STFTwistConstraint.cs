using System;
using Newtonsoft.Json.Linq;
using STF.ApplicationConversion;
using STF.Serialisation;
using STF.Util;
using UnityEngine;
using UnityEngine.Animations;

namespace STF.Types
{
	public class STFTwistConstraint : ISTFNodeComponent
	{
		public const string _TYPE = "STF.constraint.twist";
		public override string Type => _TYPE;
		public NodeReference Source = new();
		public float Weight = 0.5f;
	}

	public class STFTwistConstraintExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public override (string, JObject) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (STFTwistConstraint)Component;
			var ret = new JObject {
				{"type", STFTwistConstraint._TYPE},
				{"weight", c.Weight}
			};
			var rf = new RefSerializer(ret);

			ret.Add("source", rf.NodeRef(c.Source.Id));

			SerializeRelationships(c, ret);
			return (c.Id, ret);
		}
	}

	public class STFTwistConstraintImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public override void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<STFTwistConstraint>();
			ParseRelationships(Json, c);
			var rf = new RefDeserializer(Json);

			c.Id = Id;
			c.Weight = (float)Json["weight"];
			c.Source = Json.ContainsKey("source") ? State.GetNodeReference(rf.NodeRef(Json["source"])) : new NodeReference(Utils.GetNodeComponent(Go.transform.parent?.parent?.gameObject));

			State.AddNodeComponent(c);
		}
	}
	

	public class STFTwistConstraintConverter : ISTFNodeComponentApplicationConverter
	{
		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			// nothing to convert
		}

		public string ConvertPropertyPath(ISTFApplicationConvertState State, Component Resource, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var stfComponent = (STFTwistConstraint)Component;
			var converted = Component.gameObject.AddComponent<RotationConstraint>();

			converted.weight = stfComponent.Weight;
			converted.rotationAxis = Axis.Y;

			var source = new ConstraintSource {
				weight = 1,
				sourceTransform = stfComponent.Source.Node != null ? stfComponent.Source.Node.transform : Component.transform.parent.parent,
			};
			converted.AddSource(source);

			Quaternion rotationOffset = Quaternion.Inverse(source.sourceTransform.rotation) * converted.transform.rotation;
			converted.rotationOffset = rotationOffset.eulerAngles;

			converted.locked = true;
			converted.constraintActive = true;
			
			State.RelMat.AddConverted(Component, converted);
		}
	}
}