
using System.Collections.Generic;

namespace stf.serialisation
{
	public interface ISTFComponent
	{
		string id {get; set;}
		List<string> extends {get; set;}
		List<string> overrides {get; set;}
		List<string> targets {get; set;}
	}
}
