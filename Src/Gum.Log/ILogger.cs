using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Log
{
    public interface ILogger : IMutable<ILogger>
    {   
        void Add(ILog log);
        bool HasError { get; }
    }
}
