using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Infra
{
    public interface IError
    {
        string Message { get; }
    }

    public interface IErrorCollector : ICloneable<IErrorCollector>
    {
        void Add(IError code);
        bool HasError { get; }
    }
}
