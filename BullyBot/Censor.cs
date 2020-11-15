using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BullyBot
{
	public class Censor
	{
		public IList<string> CensoredWords { get; private set; }

		public Censor(IEnumerable<string> censoredWords)
		{
			if (censoredWords == null)
				throw new ArgumentNullException(nameof(censoredWords));
			CensoredWords = new List<string>(censoredWords);
		}

		public bool CensorText(string text)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			string input = text;
			foreach (string censoredWord in CensoredWords)
			{
				string regexPattern = ToRegexPattern(censoredWord);
				input = Regex.Replace(input, regexPattern, new MatchEvaluator(StarCensoredMatch), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			}
			return input != text;
		}

		private static string StarCensoredMatch(Match m)
		{
			return new string('*', m.Captures[0].Value.Length);
		}

		private string ToRegexPattern(string wildcardSearch)
		{
			string str = Regex.Escape(wildcardSearch).Replace("\\*", ".*?").Replace("\\?", ".");
			if (str.StartsWith(".*?"))
				str = "(^\\b)*?" + str.Substring(3);
			return "\\b" + str + "\\b";
		}
	}
}
