
using System;
using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFSecondStageConverter
	{
		void convert(GameObject node, GameObject root, Component originalComponent);
	}
}
