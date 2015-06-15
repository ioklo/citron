using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.AbstractSyntax
{
    //public class ClassDecl : IREPLStmt
    //{
    //    // 클래스 이름
    //    public TypeIdentifier Name { get; private set; }

    //    // 함수과 프로퍼티들
    //    public IReadOnlyList<ClassFuncDecl> FuncDecls { get; private set; }

    //    // 변수들
    //    public IReadOnlyList<ClassVarDecl> VarDecls { get; private set; }

    //    // 베이스 타입 (Interface 포함)
    //    public IReadOnlyList<TypeIdentifier> BaseTypes { get; private set; }

    //    public ClassDecl(TypeIdentifier name, IEnumerable<ClassFuncDecl> funcDecls, IEnumerable<ClassVarDecl> varDecls, IEnumerable<TypeIdentifier> baseTypes)
    //    {
    //        Name = name;
    //        FuncDecls = new List<ClassFuncDecl>(funcDecls);
    //        VarDecls = new List<ClassVarDecl>(varDecls);
    //        BaseTypes = new List<TypeIdentifier>(baseTypes);
    //    }

    //    public void Visit(IREPLStmtVisitor visitor)
    //    {
    //        visitor.Visit(this);
    //    }
    //}
}
