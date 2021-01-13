using System;
using System.Collections.Generic;
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
        public Script? Run()
        {
            var topLevelStmts = new List<Stmt>();

            foreach (var syntaxStmt in root.GetTopLevelStmts())
            {
                var stmtResult = VisitStmt(syntaxStmt);
                topLevelStmts.Add(stmtResult.Stmt);
            }

            foreach (var func in root.GetAllFuncs())
            {
                VisitFunc(func);
            }

            return new Script(,, topLevelStmts);
        }
    }
}