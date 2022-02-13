using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0Evaluator
{
    public interface IIR0CommandProvider
    {
        Task ExecuteAsync(string cmdText);
    }
}
