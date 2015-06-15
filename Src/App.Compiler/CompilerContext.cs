using Gum.Core.AbstractSyntax;
using Gum.Core.IL;
using Gum.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler
{
    public class CompilerContext
    {
        internal void PushLocalScope()
        {
            throw new NotImplementedException();
        }

        internal void PopLocalScope()
        {
            throw new NotImplementedException();
        }

        internal int AddLocal(string typeName, string varName)
        {
            throw new NotImplementedException();
        }

        internal int AddLocal(string typeName)
        {
            throw new NotImplementedException();
        }

        internal bool GetLocalIndex(out int localIndex, string p)
        {
            throw new NotImplementedException();
        }

        internal bool GetGlobalIndex(out int globalIndex, string p)
        {
            throw new NotImplementedException();
        }

        internal string GetLocalType(int result1)
        {
            throw new NotImplementedException();
        }

        internal string GetGlobalType(int globalIndex)
        {
            throw new NotImplementedException();
        }

        internal FuncValue GetBinOpFunc(BinaryExpKind kind, string type1, string type2)
        {
            throw new NotImplementedException();
        }

        internal FuncValue GetUnOpFunc(UnaryExpKind kind, string type)
        {
            throw new NotImplementedException();
        }

        internal TypeValue GetTypeValue(string typeArgName)
        {
            throw new NotImplementedException();
        }

        internal FuncValue GetFuncValue(string type, string funcName)
        {
            throw new NotImplementedException();
        }
    }
}
