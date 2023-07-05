
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFSecondStageResourceProcessor
	{
		UnityEngine.Object convert(GameObject root, UnityEngine.Object resource, STFSecondStageContext context);
	}
}
