#if UNITY_EDITOR
#if AVA_UNIVRM_FOUND

using System.Collections.Generic;
using System.Threading.Tasks;
using AVA.Serialisation;
using STF.ApplicationConversion;
using UnityEngine;
using VRM;

namespace ava.Converters
{
	public class AVAJankyFallbackPhysicsVRMConverter : ISTFNodeComponentApplicationConverter
	{
		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			var c = (AVAJankyFallbackPhysics)Component;

			State.AddTask(new Task(() => {
				var secondary = State.Root.transform.Find("VRM_secondary");
				var springbone = secondary.gameObject.AddComponent<VRMSpringBone>();

				springbone.RootBones.Add(c.target.transform);
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
