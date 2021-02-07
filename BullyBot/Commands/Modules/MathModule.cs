using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MathEngine;
using MathEngine.SigFig;

namespace BullyBot.Modules
{

    public class MathModule : ModuleBase<BullyBotCommandContext>
    {
        [Command("eval")]
        public async Task EvalAsync([Remainder] string str)
        {
            //foreach (var str in param)
            //{
            //	Console.WriteLine(str);
            //}
            SigFigCalculator calc = new SigFigCalculator();
            SigFigCalculatorResult result = calc.Evaluate(str);

            if (result.Success)
            {
                await ReplyAsync("Your expression was evaluated:\n\n" +
                    "Unrounded (might be screwed up) = " + result.Value +
                    "\nRounded: " + result.RoundedValue);
            }
            else
            {
                await ReplyAsync("Sorry your expression could not be evaluated:\n\n" +
                    $"`{result.ErrorMessage}`");
            }
        }
    }
}
