using System.Collections.Generic;

namespace STF.Util
{
	static class CollectionUtil
	{
		public static Dictionary<A, B> Combine<A, B>(Dictionary<A, B> Base, Dictionary<A, B> ToBeMerged)
		{
			var ret = new Dictionary<A, B>(Base);
			foreach(var e in ToBeMerged)
			{
				if(ret.ContainsKey(e.Key)) ret[e.Key] = e.Value;
				else ret.Add(e.Key, e.Value);
			}
			return ret;
		}
		public static List<A> Combine<A>(List<A> Base, List<A> ToBeMerged)
		{
			var ret = new List<A>(Base);
			foreach(var e in ToBeMerged)
			{
				if(!ret.Contains(e)) ret.Add(e);
			}
			return ret;
		}
	}
}