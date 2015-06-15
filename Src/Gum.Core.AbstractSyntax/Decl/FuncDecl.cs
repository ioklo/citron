using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class FuncDecl : IREPLStmt, IFileUnitDecl
    {
        // 리턴 타입
        public TypeIdentifier ReturnType { get; private set; }

        // 함수  이름
        public VarIdentifier Name { get; private set; }

        // 파라미터
        public IReadOnlyList<FuncParameter> Parameters { get; private set; }

        // 함수 바디
        public IReadOnlyList<IFuncStmt> BodyStmts { get; private set; }

        public void Visit(IREPLStmtVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Visit(IFileUnitDeclVisitor visitor)
        {
            visitor.Visit(this);
        }

        public FuncDecl(TypeIdentifier returnType, VarIdentifier name, IEnumerable<FuncParameter> parameters, IReadOnlyList<IFuncStmt> bodyStmts)
        {
            ReturnType = returnType;
            Name = name;
            Parameters = new List<FuncParameter>(parameters);
            BodyStmts = new List<IFuncStmt>(bodyStmts);
        }
    }
}
