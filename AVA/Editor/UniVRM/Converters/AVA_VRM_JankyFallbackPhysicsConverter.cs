#if UNITY_EDITOR
#if AVA_UNIVRM_FOUND

using System.Threading.Tasks;
using AVA.Types;
using STF.ApplicationConversion;
using UnityEngine;
using VRM;

namespace AVA.ApplicationConversion
{
	public class AVA_VRM_JankyFallbackPhysicsConverter : ISTFNodeComponentApplicationConverter
	{
		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var c = (AVAJankyFallbackPhysics)Component;

			State.AddTask(new Task(() => {
				var secondary = State.Root.transform.Find("VRM_secondary");
				var springbone = secondary.gameObject.AddComponent<VRMSpringBone>();

				springbone.RootBones.Add(c.target.Node.transform);
				springbone.m_dragForce = c.pull;
				springbone.m_stiffnessForce = c.stiffness;
				
				State.RelMat.AddConverted(Component, springbone);
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
