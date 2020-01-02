using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Infra
{
    public interface IErrorCollector
    {
        void Add(object obj, string msg);
        bool HasError { get; }
    }
}
