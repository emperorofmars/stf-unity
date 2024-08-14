using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFResourceApplicationConverter
	{
		string ConvertPropertyPath(ISTFApplicationConvertState State, Object Resource, string STFProperty);
		void Convert(ISTFApplicationConvertState State, Object Resource);
	}
}
