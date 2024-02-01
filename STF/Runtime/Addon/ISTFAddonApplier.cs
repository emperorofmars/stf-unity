using UnityEngine;

namespace STF.Addon
{
	public interface ISTFAddonApplier
	{
		public void Apply(GameObject Target, Component SourceComponent);
	}
}