
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace nna.processors
{
	public class TwistBone : IProcessor
	{
		public const string _Type = "c-twist";
		public string Type => _Type;

		public void Process(NNAContext Context, GameObject Target, GameObject NNANode, JObject Json)
		{
			var converted = Target.AddComponent<RotationConstraint>();
			
			var weight = (float)ParseUtil.GetMulkikeyOrDefault(Json, new JValue(0.5f), "w", "weight");
			GameObject sourceGo;
			if(ParseUtil.HasMulkikey(Json, "tp", "target"))
			{
				sourceGo = ParseUtil.ResolvePath(Context.Root.transform, Target.transform, (string)ParseUtil.GetMulkikey(Json, "tp", "target"));
			}
			else
			{
				sourceGo = Target.transform.parent.parent.gameObject;
			}

			converted.weight = weight;
			converted.rotationAxis = Axis.Y;

			var source = new ConstraintSource {
				weight = 1,
				sourceTransform = sourceGo.transform,
			};
			converted.AddSource(source);

			Quaternion rotationOffset = Quaternion.Inverse(source.sourceTransform.rotation) * converted.transform.rotation;
			converted.rotationOffset = rotationOffset.eulerAngles;

			converted.locked = true;
			converted.constraintActive = true;
		}
	}
}