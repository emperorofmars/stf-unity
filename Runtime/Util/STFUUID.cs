using System;
using System.Collections.Generic;
using stf.serialisation;
using UnityEngine;

namespace stf
{
	public class STFUUID : MonoBehaviour
	{
		[Serializable]
		public class ComponentIdMapping
		{
			public string id;
			public Component component;
		}
		public string id;
		public string boneId;
		public List<ComponentIdMapping> componentIds = new List<ComponentIdMapping>();

		public string GetIdByComponent(Component component)
		{
			var m = componentIds.Find(c => c.component == component);
			return m?.id;
		}

		public Component GetComponentById(string id)
		{
			var m = componentIds.Find(c => c.id == id);
			return m?.component;
		}
	}
}
