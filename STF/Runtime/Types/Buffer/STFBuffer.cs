
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace STF.Types
{
	public class STFBuffer : ISTFBuffer
	{
		[HideInInspector] public byte[] Data;

		public override byte[] GetData()
		{
			return Data;
		}
	}
}
