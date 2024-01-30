
using STF.Serialisation;
using UnityEngine;

namespace STF
{
	public static class STFAddonApplier
	{
		public static GameObject Apply(ISTFAsset Base, STFAddonAsset Addon, bool InPlace = false)
		{
			GameObject ret = InPlace ? Base.gameObject : UnityEngine.Object.Instantiate(Base.gameObject);
			ret.name = Base.name;
			
			return ret;
		}
	}
}
