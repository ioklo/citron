using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class VarDecl : IREPLStmt, IFileUnitDecl, IStmt
    {
        public TypeIdentifier Type { get; private set; }
        public IReadOnlyList<NameAndExp> NameAndExps { get; private set; }

        public VarDecl(TypeIdentifier typeID, IEnumerable<NameAndExp> nameAndExps )
        {
            Type = typeID;
            NameAndExps = new List<NameAndExp>(nameAndExps);
        }

        public void Visit(IREPLStmtVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Visit(IFileUnitDeclVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Visit(IStmtVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Visit(IFuncStmtVisitor visitor)
        {
            visitor.Visit(this);
        }

        public Result Visit<Result>(IStmtVisitor<Result> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
