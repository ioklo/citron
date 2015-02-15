using System.Collections.Generic;

namespace Gum.App.Compiler.AST
{
    public class Program
    {
        public List<IDecl> Decls { get; private set; }

        public Program()
        {
            Decls = new List<IDecl>();
        }
    }    
}