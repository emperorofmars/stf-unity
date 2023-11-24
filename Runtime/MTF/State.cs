using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	public interface IImportState
	{
		UnityEngine.Object GetResource(string Id);
	}

	public interface IExportState
	{
		string AddResource(UnityEngine.Object Resource);
	}
}