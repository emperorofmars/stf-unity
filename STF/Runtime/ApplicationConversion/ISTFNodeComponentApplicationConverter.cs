using System.Collections.Generic;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFNodeComponentApplicationConverter
	{
		void ConvertResources(ISTFApplicationConvertState State, Component Component);
		void Convert(ISTFApplicationConvertState State, Component Component);
	}
}