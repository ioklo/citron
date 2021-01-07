using System;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Phase3
    {
        Analyzer analyzer;
        Phase3Root root;

        bool bGlobalScope;

        public Phase3(Analyzer analyzer, Phase3Root root)
        {
            this.analyzer = analyzer;
            this.root = root;

            bGlobalScope = true;
        }

        void VisitFunc(Phase3Func func)
        {

        }
        
        //TVarDecl VisitVarDecl<TVarDecl>(S.VarDecl varDecl, VarDeclVisitor<TVarDecl> varDeclVisitor)
        //{

        //}

        // Phase3에서는 함수의 Stmt위주로 순회하게 된다
        // TopLevel, GlobalFunc, MemberFunc이다
        public void Run()
        {
            foreach(var stmt in root.GetTopLevelStmts())
                VisitStmt(stmt);

            foreach (var func in root.GetAllFuncs())
                VisitFunc(func);
        }
    }
}