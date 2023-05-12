using System.Collections.Generic;

namespace stf.serialisation
{
	public class STFRelationshipMatrix
	{
		Dictionary<string, List<string>> Extends = new Dictionary<string, List<string>>();
		Dictionary<string, bool> IsOverridden = new Dictionary<string, bool>();
		Dictionary<string, List<string>> Overrides = new Dictionary<string, List<string>>();
		Dictionary<string, bool> TargetMatch = new Dictionary<string, bool>();

	}
}