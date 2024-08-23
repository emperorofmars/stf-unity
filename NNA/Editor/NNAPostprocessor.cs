
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using nna.jank;

namespace nna
{
	class NNAPostprocessor : AssetPostprocessor
	{
		void OnPostprocessModel(GameObject Root)
		{
			var nnaContext = new NNAContext(Root, assetImporter.userData);
			NNAConverter.Convert(nnaContext);
			foreach(var newObj in nnaContext.GetNewObjects())
			{
				context.AddObjectToAsset(newObj.Name, newObj.NewObject);
			}
		}
	}
}

#endif
