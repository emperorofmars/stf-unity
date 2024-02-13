#if UNITY_EDITOR
#if AVA_UNIVRM_FOUND

using System.Collections.Generic;
using System.Threading.Tasks;
using AVA.Serialisation;
using STF.ApplicationConversion;
using UniHumanoid;
using UnityEngine;
using VRM;

namespace ava.Converters
{
	public class AVAEyeBoneLimitsSimpleVRMConverter : ISTFNodeComponentApplicationConverter
	{
		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var c = (AVAEyeBoneLimitsSimple)Component;
			State.AddTask(new Task(() => {
				AVAAvatar avaAvatar = State.RelMat.GetExtended<AVAAvatar>(Component);
				var humanoid = avaAvatar.TryGetHumanoidDefinition();

				var vrmLookat = Component.gameObject.AddComponent<VRMLookAtBoneApplyer>();
				vrmLookat.LeftEye.Transform = humanoid.Mappings.Find(p => p.humanoidName == "EyeLeft")?.bone?.transform;
				vrmLookat.RightEye.Transform = humanoid.Mappings.Find(p => p.humanoidName == "EyeRight")?.bone?.transform;
				
				// vrmLookat.VerticalDown = // i cant be arsed to look at how this nonesense is defined
				// etc
				
				State.RelMat.AddConverted(Component, vrmLookat);
			}));
		}

		public string ConvertPropertyPath(ISTFApplicationConvertState State, Component Component, string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			return;
		}
	}
}

#endif
#endif
