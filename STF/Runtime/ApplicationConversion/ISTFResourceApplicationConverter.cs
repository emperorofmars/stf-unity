using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFResourceApplicationConverter
	{
		string ConvertPropertyPath(ISTFApplicationConvertState State, UnityEngine.Object Resource, string STFProperty);
		void Convert(ISTFApplicationConvertState State, UnityEngine.Object Resource);
	}
}