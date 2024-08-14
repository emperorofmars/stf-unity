
using System;
using System.Linq;

namespace MTF.Editor.Util
{
	// from https://stackoverflow.com/questions/857705/get-all-derived-types-of-a-type
	public static class ReflectionUtils
	{
		public static Type[] GetAllSubclasses(Type Superclass)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
					// alternative: .GetExportedTypes()
					.SelectMany(domainAssembly => domainAssembly.GetTypes())
					.Where(type => Superclass.IsAssignableFrom(type) && !type.IsAbstract
					// alternative: => type.IsSubclassOf(type)
					// alternative: && type != type
					// alternative: && ! type.IsAbstract
					).ToArray();
		}
	}
}