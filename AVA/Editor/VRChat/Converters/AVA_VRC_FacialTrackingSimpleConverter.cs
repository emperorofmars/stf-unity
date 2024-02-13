
#if UNITY_EDITOR
#if AVA_VRCSDK3_FOUND

using System.Threading.Tasks;
using AVA.Serialisation;
using STF.ApplicationConversion;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace AVA.ApplicationConversion
{
	public class AVA_VRC_FacialTrackingSimpleConverter : ISTFNodeComponentApplicationConverter
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
			var c = (AVAFacialTrackingSimple)Component;
			State.AddTask(new Task(() => {
				AVAAvatar avaAvatar = State.RelMat.GetExtended<AVAAvatar>(Component);

				var avatar = (VRCAvatarDescriptor)State.RelMat.GetConverted(avaAvatar).Find(c => c is VRCAvatarDescriptor);

				var smr = c.TargetMeshInstance.GetComponent<SkinnedMeshRenderer>();

				avatar.VisemeSkinnedMesh = smr;
				avatar.lipSync = VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
				avatar.VisemeBlendShapes = new string[15];
				avatar.VisemeBlendShapes[0] = c.Mappings.Find(m => m.VisemeName == "sil")?.BlendshapeName;
				avatar.VisemeBlendShapes[1] = c.Mappings.Find(m => m.VisemeName == "pp")?.BlendshapeName;
				avatar.VisemeBlendShapes[2] = c.Mappings.Find(m => m.VisemeName == "ff")?.BlendshapeName;
				avatar.VisemeBlendShapes[3] = c.Mappings.Find(m => m.VisemeName == "th")?.BlendshapeName;
				avatar.VisemeBlendShapes[4] = c.Mappings.Find(m => m.VisemeName == "dd")?.BlendshapeName;
				avatar.VisemeBlendShapes[5] = c.Mappings.Find(m => m.VisemeName == "kk")?.BlendshapeName;
				avatar.VisemeBlendShapes[6] = c.Mappings.Find(m => m.VisemeName == "ch")?.BlendshapeName;
				avatar.VisemeBlendShapes[7] = c.Mappings.Find(m => m.VisemeName == "ss")?.BlendshapeName;
				avatar.VisemeBlendShapes[8] = c.Mappings.Find(m => m.VisemeName == "nn")?.BlendshapeName;
				avatar.VisemeBlendShapes[9] = c.Mappings.Find(m => m.VisemeName == "rr")?.BlendshapeName;
				avatar.VisemeBlendShapes[10] = c.Mappings.Find(m => m.VisemeName == "aa")?.BlendshapeName;
				avatar.VisemeBlendShapes[11] = c.Mappings.Find(m => m.VisemeName == "e")?.BlendshapeName;
				avatar.VisemeBlendShapes[12] = c.Mappings.Find(m => m.VisemeName == "ih")?.BlendshapeName;
				avatar.VisemeBlendShapes[13] = c.Mappings.Find(m => m.VisemeName == "oh")?.BlendshapeName;
				avatar.VisemeBlendShapes[14] = c.Mappings.Find(m => m.VisemeName == "ou")?.BlendshapeName;

				if(c.Mappings.Find(m => m.VisemeName == "blink") != null)
				{
					avatar.customEyeLookSettings.eyelidType = VRCAvatarDescriptor.EyelidType.Blendshapes;
					avatar.customEyeLookSettings.eyelidsSkinnedMesh = smr;
					avatar.customEyeLookSettings.eyelidsBlendshapes = new int[3];
					avatar.customEyeLookSettings.eyelidsBlendshapes[0] = GetBlendshapeIndex(smr.sharedMesh, c.Mappings.Find(m => m.VisemeName == "blink")?.BlendshapeName);
					avatar.customEyeLookSettings.eyelidsBlendshapes[1] = GetBlendshapeIndex(smr.sharedMesh, c.Mappings.Find(m => m.VisemeName == "look_up")?.BlendshapeName);
					avatar.customEyeLookSettings.eyelidsBlendshapes[2] = GetBlendshapeIndex(smr.sharedMesh, c.Mappings.Find(m => m.VisemeName == "look_down")?.BlendshapeName);
				}

				State.RelMat.AddConverted(Component, avatar);
			}));
		}

		public int GetBlendshapeIndex(Mesh mesh, string name)
		{
		
			for(int i = 0; i < mesh.blendShapeCount; i++)
			{
				var bName = mesh.GetBlendShapeName(i);
				if(bName == name) return i;
			}
			return -1;
		}
	}
}

#endif
#endif
