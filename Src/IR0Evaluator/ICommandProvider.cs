using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Citron
{
    public interface IIR0CommandProvider
    {
        Task ExecuteAsync(string cmdText);
    }
}
