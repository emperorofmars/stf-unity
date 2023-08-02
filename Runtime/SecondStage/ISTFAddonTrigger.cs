using System.Linq;
using UnityEngine;

namespace stf.serialisation
{
	// Can be implemented for components in order to run code during the STFAddonApplier ApplyAddon function.

	public interface ISTFAddonTrigger
	{
		void Apply(Component triggerComponent, GameObject root);
	}
}