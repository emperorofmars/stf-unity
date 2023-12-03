using UnityEngine;

namespace STF_Util
{
	public class IdAttribute : PropertyAttribute
	{
		public bool editable = false;
		public string editingId;
	}
}