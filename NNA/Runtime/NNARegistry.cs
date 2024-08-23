
using System.Collections.Generic;
using nna.processors;

namespace nna
{
	public static class NNARegistry
	{
		public const string DefaultContext = "default";

		// Type -> Context -> IProcessor
		// A `null` Context is the default context applicable to all context's, unless a IProcessor for a specific context is registered.
		public static readonly Dictionary<string, Dictionary<string, IProcessor>> DefaultProcessors = new Dictionary<string, Dictionary<string, IProcessor>>() {
			{TwistBone._Type, new Dictionary<string, IProcessor> {{DefaultContext, new TwistBone()}}},
			{HumanoidMapping._Type, new Dictionary<string, IProcessor> {{DefaultContext, new HumanoidMapping()}}},
		};
		private static Dictionary<string, Dictionary<string, IProcessor>> RegisteredProcessors = new Dictionary<string, Dictionary<string, IProcessor>>();
		
		public static Dictionary<string, Dictionary<string, IProcessor>> Processors { get {
			var ret = new Dictionary<string, Dictionary<string, IProcessor>>();
			foreach(var entry in DefaultProcessors)
			{
				ret.Add(entry.Key, new Dictionary<string, IProcessor>(entry.Value));
			}
			foreach(var entry in RegisteredProcessors)
			{
				foreach(var contextEntry in entry.Value)
				{
					MergeEntryIntoProcessorDict(ret, entry.Key, contextEntry.Key, contextEntry.Value);
				}
			}
			return ret;
		}}

		public static Dictionary<string, IProcessor> GetProcessors(string Context = DefaultContext)
		{
			var ret = new Dictionary<string, IProcessor>();
			foreach(var entry in Processors)
			{
				if(entry.Value.ContainsKey(Context))
				{
					ret.Add(entry.Key, entry.Value[Context]);
				}
				else if(entry.Value.ContainsKey(DefaultContext))
				{
					ret.Add(entry.Key, entry.Value[DefaultContext]);
				}
			}
			return ret;
		}

		public static List<string> GetAvaliableContexts()
		{
			var ret = new List<string>();
			foreach(var entry in Processors)
			{
				foreach(var contextEntry in entry.Value)
				{
					if(!ret.Contains(contextEntry.Key)) ret.Add(contextEntry.Key);
				}
			}
			return ret;
		}

		public static void RegisterProcessor(IProcessor Processor, string Type, string Context = DefaultContext)
		{
			MergeEntryIntoProcessorDict(RegisteredProcessors, Type, Context, Processor);
		}

		private static void MergeEntryIntoProcessorDict(Dictionary<string, Dictionary<string, IProcessor>> Dict, string Type, string Context, IProcessor Processor)
		{
			if(Dict.ContainsKey(Type))
			{
				var existing = Dict[Type];
				if(existing.ContainsKey(Context))
				{
					existing[Context] = Processor;
				}
				else
				{
					existing.Add(Context, Processor);
				}
			}
			else
			{
				Dict.Add(Type, new Dictionary<string, IProcessor> {{Context, Processor}});
			}
		}
	}
}
