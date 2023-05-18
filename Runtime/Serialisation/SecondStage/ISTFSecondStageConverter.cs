
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	public class STFSecondStageContext
	{
		public STFRelationshipMatrix RelMat;
		public List<Task> Tasks = new List<Task>();
	}

	public interface ISTFSecondStageConverter
	{
		void convert(Component component, GameObject root, List<UnityEngine.Object> resources, STFSecondStageContext context);
	}
}
