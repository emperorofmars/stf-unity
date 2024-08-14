
using UnityEngine;

namespace STF.Types
{
	public abstract class ISTFBuffer : ScriptableObject
	{
		public string Id;
		public abstract byte[] GetData();
	}
}
