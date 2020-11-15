using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BullyBot.Interactive
{
	public interface ICriteria<in T>
	{
		public Task<bool> JudgeAsync(T parameter);
	}
}
