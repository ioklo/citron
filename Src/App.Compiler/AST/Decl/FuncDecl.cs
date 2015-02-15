using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class FuncDecl : IDecl
    {
        public string ReturnTypeName { get; private set; }
        public string Name { get; private set; }
        public List<TypeAndName> Parameters { get; private set; }
        public BlockStmt Body { get; set; }

        public FuncDecl(string retTypeName, string name)
        {
            ReturnTypeName = retTypeName;
            Name = name;
            Parameters = new List<TypeAndName>();
        }
    }
}
