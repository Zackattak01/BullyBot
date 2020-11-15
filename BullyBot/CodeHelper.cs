﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BullyBot
{
	public static class CodeHelper
	{
		//const string begginningBoilerplate =
		//	"using System;" +
		//	"using System.Threading.Tasks;" +
		//	"using Discord.Commands;" +
		//	"using Discord;" +
		//	"namespace BullyBot {" +
		//	"public static class Execute{" +
		//	"public static async Task Run(SocketCommandContext Context){";

		private readonly static string begginningBoilerplate = File.ReadAllText("begginningBoilerplate.cs");

		const string endingBoilerplate =
			"}}}";


		public static string CreateValidCode(string invalidCode)
		{
			return begginningBoilerplate + invalidCode + endingBoilerplate;
		}

		public static (UnloadableAssemblyLoadContext, string) CompileAndLoadAssembly(string code)
		{
			//AppDomain dom = AppDomain.CreateDomain("execute");
			
			
			var tree = SyntaxFactory.ParseSyntaxTree(code);
			string fileName = "exec.dll";

			var systemRefLocation = typeof(object).GetTypeInfo().Assembly.Location;
			var tasksLocation = typeof(System.Threading.Tasks.Task).GetTypeInfo().Assembly.Location;
			var discordCoreLocation = typeof(Discord.IChannel).GetTypeInfo().Assembly.Location;
			var discordRestLocation = typeof(Discord.Rest.RestUserMessage).GetTypeInfo().Assembly.Location;
			var discordSocketLocation = typeof(Discord.WebSocket.SocketUserMessage).GetTypeInfo().Assembly.Location;
			var discordCommandLocation = typeof(Discord.Commands.SocketCommandContext).GetTypeInfo().Assembly.Location;


			
			var systemReference = MetadataReference.CreateFromFile(systemRefLocation);
			var netStandardRef = MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51").Location);
			var systemRuntimeRef = MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location);
			var discordCoreReference = MetadataReference.CreateFromFile(discordCoreLocation);
			var discordRestReference = MetadataReference.CreateFromFile(discordRestLocation);
			var discordSocketReference = MetadataReference.CreateFromFile(discordSocketLocation);
			var discordCommandReference = MetadataReference.CreateFromFile(discordCommandLocation);

			var compilation = CSharpCompilation.Create(fileName)
				.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
				.AddReferences(systemReference, netStandardRef, systemRuntimeRef, discordCoreReference, discordRestReference, discordSocketReference, discordCommandReference)
				.AddSyntaxTrees(tree);

			

			//string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
			var stream = new MemoryStream();
			EmitResult compilationResult = compilation.Emit(stream);

			if (compilationResult.Success)
			{
				stream.Seek(0, SeekOrigin.Begin);

				var alc = new UnloadableAssemblyLoadContext();
				alc.LoadFromStream(stream);

				return (alc, "");
			}
			else
			{
				string issues = "";
				foreach (var codeIssue in compilationResult.Diagnostics)
				{
					string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, " +
						$"Location: { codeIssue.Location.GetLineSpan()}, " +
						$"Severity: { codeIssue.Severity}" + "\n";

					issues += issue;
				}

				return (null, issues);
			}
		}

		//method assumes that the Assembly has a "Run" method under the Execute class which is in the BullyBot namespace.
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void ExecuteCodeAndUnloadAssembly(UnloadableAssemblyLoadContext alc, params object[] args)
		{
			Assembly asm = alc.Assemblies.First();

			Type execType = asm.GetType("BullyBot.Execute");

			object execObj = execType.GetConstructors().First().Invoke(args);
	
			execType.GetMethod("Run").Invoke(execObj, null);

	
			alc.Unload();



		}


	}
}
