#if UNITY_EDITOR
#if AVA_UNIVRM_FOUND

using AVA.Types;
using STF.ApplicationConversion;
using UnityEngine;
using VRM;

namespace AVA.ApplicationConversion
{
	public class AVA_VRM_FacialTrackingSimpleConverter : ISTFNodeComponentApplicationConverter
	{

		private void createBlendshapeClip(string visemeName, BlendShapePreset blendshapePreset, AVAFacialTrackingSimple c, VRMBlendShapeProxy vrmBlendshapeProxy, ISTFApplicationConvertState State)
		{
			BlendShapeClip clip = new BlendShapeClip();
			clip.name = "VRM_Clip_" + visemeName;
			clip.BlendShapeName = c.Mappings.Find(m => m.VisemeName == visemeName)?.BlendshapeName;
			clip.Preset = blendshapePreset;
			var bindingsList = new BlendShapeBinding[1];
			BlendShapeBinding binding = new BlendShapeBinding();
			Mesh mesh = c.TargetMeshInstance.NodeComponent.GetComponent<SkinnedMeshRenderer>() != null ? c.TargetMeshInstance.NodeComponent.GetComponent<SkinnedMeshRenderer>().sharedMesh : c.TargetMeshInstance.NodeComponent.GetComponent<MeshFilter>().sharedMesh;
			binding.Index = mesh.GetBlendShapeIndex(clip.BlendShapeName);
			binding.RelativePath = STF.Util.Utils.getPath(c.transform, c.TargetMeshInstance.NodeComponent.transform)?.Substring(1);
			binding.Weight = 100;
			bindingsList[0] = binding;
			clip.Values = bindingsList;
			vrmBlendshapeProxy.BlendShapeAvatar.Clips.Add(clip);
			State.SaveGeneratedResource(clip, "Asset");
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

		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			return;
		}

		public string ConvertPropertyPath(ISTFApplicationConvertState State, Component Component, string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var c = (AVAFacialTrackingSimple)Component;
			VRMBlendShapeProxy vrmBlendshapeProxy = State.RelMat.GetExtended<VRMBlendShapeProxy>(Component);
			if(vrmBlendshapeProxy == null) vrmBlendshapeProxy = Component.gameObject.AddComponent<VRMBlendShapeProxy>();

			if(vrmBlendshapeProxy.BlendShapeAvatar == null)
			{
				BlendShapeAvatar vrmBlendShapeAvatar = new BlendShapeAvatar();
				vrmBlendShapeAvatar.name = "VRM_BlendshapeAvatar";
				vrmBlendshapeProxy.BlendShapeAvatar = vrmBlendShapeAvatar;
				State.SaveGeneratedResource(vrmBlendShapeAvatar, "Asset");
			}

			createBlendshapeClip("aa", BlendShapePreset.A, c, vrmBlendshapeProxy, State);
			createBlendshapeClip("e", BlendShapePreset.E, c, vrmBlendshapeProxy, State);
			createBlendshapeClip("ih", BlendShapePreset.I, c, vrmBlendshapeProxy, State);
			createBlendshapeClip("oh", BlendShapePreset.O, c, vrmBlendshapeProxy, State);
			createBlendshapeClip("ou", BlendShapePreset.U, c, vrmBlendshapeProxy, State);

			createBlendshapeClip("blink", BlendShapePreset.Blink, c, vrmBlendshapeProxy, State);

			State.RelMat.AddConverted(Component, vrmBlendshapeProxy);
		}
	}
}

#endif
#endif
