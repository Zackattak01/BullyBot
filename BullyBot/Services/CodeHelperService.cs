using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using ConfigurableServices;

namespace BullyBot
{
    public class CodeHelperService : ConfigurableService
    {

        [ConfigureFromKey("BegginningBoilerplate")]
        public string BegginningBoilerplate { get; private set; }

        [ConfigureFromKey("EndingBoilerplate")]
        public string EndingBoilerplate { get; private set; }

        public CodeHelperService(IConfigService configService)
         : base(configService)
        {
        }

        public string CreateValidCode(string invalidCode)
        {
            return BegginningBoilerplate + invalidCode + EndingBoilerplate;

        }

        public (UnloadableAssemblyLoadContext, string) CompileAndLoadAssembly(string code)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            string fileName = "exec.dll";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));

            var compilation = CSharpCompilation.Create(fileName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)))
                .AddSyntaxTrees(tree);


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
        public void ExecuteCodeAndUnloadAssembly(UnloadableAssemblyLoadContext alc, params object[] args)
        {
            Assembly asm = alc.Assemblies.First();

            Type execType = asm.GetType("BullyBot.Execute");

            object execObj = execType.GetConstructors().First().Invoke(args);

            execType.GetMethod("Run").Invoke(execObj, null);


            alc.Unload();



        }


    }
}
