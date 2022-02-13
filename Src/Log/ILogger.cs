using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citron.Log
{
    public interface ILogger : IMutable<ILogger>
    {   
        void Add(ILog log);
        bool HasError { get; }
    }
}
