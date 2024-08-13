
using System;
using System.Collections.Generic;
using STF.Serialisation;

namespace STF.Util
{
	[Serializable]
	public class STFResourceMeta
	{
		[Serializable] public class Entry { public UnityEngine.Object UnityObject; public ISTFResource STFResource; }
		List<Entry> Entries = new List<Entry>();

		public bool ContainsKey(UnityEngine.Object Key)
		{
			return Entries.Find(entry => entry.UnityObject == Key) != null;
		}

		public ISTFResource this[UnityEngine.Object Key]
		{
			get => Entries.Find(entry => entry.UnityObject == Key).STFResource;
		}

		public void Add(UnityEngine.Object UnityObject, ISTFResource STFResource)
		{
			if(ContainsKey(UnityObject)) throw new Exception("Key already set!");
			Entries.Add(new Entry { UnityObject = UnityObject, STFResource = STFResource });
		}
	}
}
