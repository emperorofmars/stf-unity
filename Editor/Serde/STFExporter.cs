
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
using STF.IdComponents;
using UnityEditor;

namespace STF.Serde
{
	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class STFExporter
	{
		private STFExportState state;

		public STFExporter(string TargetLocation, string path)
		{
			try
			{
				this.state = new STFExportState(TargetLocation);
			}
			catch(Exception e)
			{
				foreach(var node in state.Nodes.Keys)
				{
					if(node != null)
					{
						UnityEngine.Object.DestroyImmediate(node);
					}
				}
				throw new Exception("Error during STF import: ", e);
			}
			finally
			{
				foreach(var trashObject in state.Trash)
				{
					if(trashObject != null)
					{
						UnityEngine.Object.DestroyImmediate(trashObject);
					}
				}
			}
		}
	}
}

#endif
