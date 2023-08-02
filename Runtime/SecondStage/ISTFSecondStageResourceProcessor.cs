
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFSecondStageResourceProcessor
	{
		UnityEngine.Object Convert(GameObject root, UnityEngine.Object resource, ISTFSecondStageContext context);
	}
}
