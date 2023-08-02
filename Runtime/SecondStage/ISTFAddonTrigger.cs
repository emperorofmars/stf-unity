using System.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFAddonTrigger
	{
		void Apply(Component triggerComponent, GameObject root);
	}
}