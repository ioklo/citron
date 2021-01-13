using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0
{
    // 
    class Phase0
    {
        S.Script script;
        TypeSkeletonCollector collector;
        Phase1Factory phase1Factory;

        public Phase0(S.Script script, TypeSkeletonCollector collector, Phase1Factory phase1Factory)
        {
            this.script = script;
            this.collector = collector;
            this.phase1Factory = phase1Factory;
        }

        void VisitEnumDecl(S.EnumDecl enumDecl)
        {
            collector.ExecInNewEnumScope(enumDecl, () =>
            {
                // var enumElemNames = enumDecl.Elems.Select(elem => elem.Name);
            });            
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            collector.ExecInNewStructScope(structDecl, () => {
                foreach (var elem in structDecl.Elems)
                {
                    switch (elem)
                    {
                        case S.StructDecl.TypeDeclElement typeElem:
                            VisitTypeDecl(typeElem.TypeDecl);
                            break;

                        case S.StructDecl.FuncDeclElement funcElem:
                            VisitFuncDecl(funcElem.FuncDecl);
                            break;
                    }
                }               
            });            
        }

        void VisitFuncDecl(S.FuncDecl funcDecl)
        {
            collector.AddFunc(funcDecl);
        }

        void VisitVarDecl(S.VarDecl varDecl)
        {
            for (int i = 0; i < varDecl.Elems.Length; i++)
                collector.AddVar(varDecl.Elems[i].VarName, varDecl, i);
        }

        void VisitTypeDecl(S.TypeDecl typeDecl)
        {
            switch (typeDecl)
            {
                case S.EnumDecl enumDecl: VisitEnumDecl(enumDecl); return;
                case S.StructDecl structDecl: VisitStructDecl(structDecl); return;
                default: throw new InvalidOperationException();
            }
        }

        void VisitScript(S.Script script)
        {
            foreach (var scriptElem in script.Elements)
            {
                switch (scriptElem)
                {
                    case S.Script.TypeDeclElement typeDeclElem:
                        VisitTypeDecl(typeDeclElem.TypeDecl);
                        return;
                        

                    case S.Script.GlobalFuncDeclElement funcElem:
                        VisitFuncDecl(funcElem.FuncDecl);
                        return;

                    case S.Script.StmtElement stmtElem when stmtElem.Stmt is S.VarDeclStmt varDeclStmt:
                        VisitVarDecl(varDeclStmt.VarDecl);
                        return;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public Phase1 Run()
        {
            VisitScript(script);

            var skelRepo = new SkeletonRepository(collector.GetGlobalSkeletons());
            return phase1Factory.Make(skelRepo);
        }
    }
}
