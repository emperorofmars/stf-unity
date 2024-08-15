
#if UNITY_EDITOR
#if AVA_VRCSDK3_FOUND

using System.Linq;
using System.Threading.Tasks;
using AVA.Types;
using STF.ApplicationConversion;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace AVA.ApplicationConversion
{
	public class AVA_VRC_EyeBoneLimitsSimpleConverter : ISTFNodeComponentApplicationConverter
	{
		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			// nothing to convert
		}

		public string ConvertPropertyPath(ISTFApplicationConvertState State, Component Resource, string STFProperty)
		{
			throw new System.NotImplementedException();
		}
		
		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var c = (AVAEyeBoneLimitsSimple)Component;
			State.AddTask(new Task(() => {
				AVAAvatar avaAvatar = State.RelMat.GetExtended<AVAAvatar>(Component);
				var humanoid = avaAvatar.TryGetHumanoidDefinition();
				
				var avatar = (VRCAvatarDescriptor)State.RelMat.GetConverted(avaAvatar).FirstOrDefault(component => component.GetType() == typeof(VRCAvatarDescriptor));

				avatar.enableEyeLook = true;
				avatar.customEyeLookSettings.leftEye = humanoid?.Mappings.Find(m => m.humanoidName == "EyeLeft")?.bone.transform;
				avatar.customEyeLookSettings.rightEye = humanoid?.Mappings.Find(m => m.humanoidName == "EyeRight")?.bone.transform;

				avatar.customEyeLookSettings.eyesLookingUp = new VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations
						{left = Quaternion.Euler(-c.up, 0f, 0f), right = Quaternion.Euler(-c.up, 0f, 0f), linked = true};
				avatar.customEyeLookSettings.eyesLookingDown = new VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations
						{left = Quaternion.Euler(c.down, 0f, 0f), right = Quaternion.Euler(c.down, 0f, 0f), linked = true};
				avatar.customEyeLookSettings.eyesLookingLeft = new VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations
						{left = Quaternion.Euler(0f, -c.outer, 0f), right = Quaternion.Euler(0f, -c.inner, 0f), linked = false};
				avatar.customEyeLookSettings.eyesLookingRight = new VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations
						{left = Quaternion.Euler(0f, c.inner, 0f), right = Quaternion.Euler(0f, c.outer, 0f), linked = false};
				
				State.RelMat.AddConverted(Component, avatar);
			}));
		}
	}
}

#endif
#endif
