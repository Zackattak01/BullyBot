using System.Runtime.Loader;
using System.Reflection;

namespace BullyBot
{
    public class UnloadableAssemblyLoadContext : AssemblyLoadContext
	{
		public UnloadableAssemblyLoadContext() : base(isCollectible: true)
		{

		}

		protected override Assembly Load(AssemblyName assemblyName)
		{
			return null;
		}
	}
}
