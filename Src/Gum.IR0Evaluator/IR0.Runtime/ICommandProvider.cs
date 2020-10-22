using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0.Runtime
{
    public interface ICommandProvider
    {
        Task ExecuteAsync(string cmdText);
    }
}
