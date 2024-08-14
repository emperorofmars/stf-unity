
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using STF.Tools;
using STF.Types;
using UnityEditor;
using UnityEngine;

namespace STF.Serialisation
{
	public static class ExportEditor
	{
		public static void ExportDialog(ISTFAsset asset, bool DebugExport = false)
		{
			ExportDialog(asset.gameObject, DebugExport);
		}
		public static void ExportDialog(GameObject root, bool DebugExport = false)
		{
			var exportSTFAsset = root.GetComponent<ISTFAsset>();
			var defaultExportFilaName = exportSTFAsset != null && !string.IsNullOrWhiteSpace(exportSTFAsset.OriginalFileName) ? exportSTFAsset.OriginalFileName : root.name;
			
			var path = EditorUtility.SaveFilePanel("STF Export", "Assets", defaultExportFilaName + ".stf", "stf");
			if(path != null && path.Length > 0) {
				ExportEditor.SerializeAsSTFBinary(root, path, DebugExport);
			}
		}

		public static void SerializeAsSTFBinary(ISTFAsset asset, string ExportPath, bool DebugExport = false)
		{
			SerializeAsSTFBinary(asset.gameObject, ExportPath, DebugExport);
		}

		public static void SerializeAsSTFBinary(GameObject root, string ExportPath, bool DebugExport = false)
		{
			var trash = new List<GameObject>();
			try
			{
				var exportInstance = UnityEngine.Object.Instantiate(root);
				exportInstance.name = root.name;
				trash.Add(exportInstance);

				var (CreatedGos, ResourceMeta) = STFSetup.SetupStandaloneAssetInplace(exportInstance);
				trash.AddRange(CreatedGos);

				var unityContext = new EditorUnityExportContext(ExportPath);
				var (stfFile, json) = Exporter.Export(unityContext, exportInstance.GetComponent<ISTFAsset>(), ResourceMeta);

				File.WriteAllBytes(ExportPath, stfFile.CreateBinaryFromBuffers());
				if(DebugExport) File.WriteAllText(ExportPath + ".json", json.ToString(Formatting.Indented));
			}
			catch(Exception e)
			{
				Debug.LogError(e);
			}
			finally
			{
				foreach(var trashObject in trash)
				{
					if(trashObject != null)
					{
						UnityEngine.Object.DestroyImmediate(trashObject);
					}
				}
			}
			return;
		}
	}
}

#endif
