using System.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFAddonTrigger
	{
		void apply(Component triggerComponent, GameObject root);
	}
}