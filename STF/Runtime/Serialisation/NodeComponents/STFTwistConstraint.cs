using System;
using System.Linq;
using System.Threading.Tasks;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using Newtonsoft.Json.Linq;
using STF.ApplicationConversion;
using UnityEngine;
using UnityEngine.Animations;

namespace STF.Serialisation
{
	public class STFTwistConstraint : ISTFNodeComponent
	{
		public const string _TYPE = "STF.constraint.twist";
		public override string Type => _TYPE;
		public GameObject Target;
		public string TargetId;
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
			if(c.Target == null && (c.TargetId == null || c.TargetId.Length == 0)) c.TargetId = c.transform.parent?.GetComponents<ISTFNode>().OrderByDescending(c => c.PrefabHirarchy).FirstOrDefault()?.Id;
			State.AddTask(new Task(() => {
				ret.Add("target", rf.NodeRef(c.Target != null ? c.Target.GetComponent<ISTFNode>().Id : c.TargetId));
			}));
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
			c.TargetId = Json.ContainsKey("target") ? rf.NodeRef(Json["target"]) : null;
			c.Target = State.Nodes.ContainsKey(c.TargetId) ? State.Nodes[c.TargetId] : null;

			State.AddNodeComponent(c, Id);
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
				sourceTransform = stfComponent.Target != null ? stfComponent.Target.transform : Component.transform.parent.parent,
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