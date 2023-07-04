
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFSecondStageResourceProcessor
	{
		void convert(GameObject root, List<UnityEngine.Object> resources, STFSecondStageContext context);
	}
}
