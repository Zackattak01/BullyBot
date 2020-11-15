using System.Threading.Tasks;

namespace BullyBot
{
	internal class Program
	{
		public static Task Main(string[] args)
		{
			return Startup.RunAsync(args);
		}
	}
}
