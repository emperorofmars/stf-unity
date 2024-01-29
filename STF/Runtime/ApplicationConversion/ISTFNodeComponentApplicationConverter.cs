using System.Collections.Generic;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFNodeComponentApplicationConverter
	{
		void ConvertResources(ISTFApplicationConvertState State, Component Component);
		string ConvertPropertyPath(ISTFApplicationConvertState State, Component Resource, string STFProperty);
		void Convert(ISTFApplicationConvertState State, Component Component);
	}
}