using System.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFAddonTrigger
	{
		void apply(UnityEngine.Object o);
	}
}