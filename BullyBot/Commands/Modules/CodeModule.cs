using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using Discord.Commands;
using Discord;
using System.IO;
using System.Runtime.Loader;

namespace BullyBot.Modules
{
    public class CodeModule : ModuleBase<BullyBotCommandContext>
    {
        public CodeHelperService CodeHelper { get; set; }

        [Command("execute")]
        [RequireOwner]
        public async Task ExecuteAsync([Remainder] string code)
        {

            string validCode = CodeHelper.CreateValidCode(code);

            (UnloadableAssemblyLoadContext alc, string errors) = CodeHelper.CompileAndLoadAssembly(validCode);

            if (errors != "")
            {
                await ReplyAsync(errors);
                return;
            }

            CodeHelper.ExecuteCodeAndUnloadAssembly(alc, Context);


        }


    }
}
