
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace STF.Serialisation
{
	// from https://stackoverflow.com/questions/857705/get-all-derived-types-of-a-type
	public static class ReflectionUtils
	{
		public static Type[] GetAllSubclasses(Type Superclass)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
					// alternative: .GetExportedTypes()
					.SelectMany(domainAssembly => domainAssembly.GetTypes())
					.Where(type => Superclass.IsAssignableFrom(type)
					// alternative: => type.IsSubclassOf(typeof(B))
					// alternative: && type != typeof(B)
					// alternative: && ! type.IsAbstract
					).ToArray();
		}
	}
}