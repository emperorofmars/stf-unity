using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFNodeComponentApplicationConverter
	{
		void Convert(ISTFApplicationConvertState State, Component Component);
	}
}