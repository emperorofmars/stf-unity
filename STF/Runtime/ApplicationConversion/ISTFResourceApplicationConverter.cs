using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFResourceApplicationConverter
	{
		void Convert(ISTFApplicationConvertState State, UnityEngine.Object Component);
	}
}