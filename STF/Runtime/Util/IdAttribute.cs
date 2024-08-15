using UnityEngine;

namespace STF.Util
{
	public class IdAttribute : PropertyAttribute
	{
		public bool editable = false;
		public string editingId;
	}
}