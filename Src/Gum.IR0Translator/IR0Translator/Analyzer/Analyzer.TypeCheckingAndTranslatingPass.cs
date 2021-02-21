using Gum.Infra;
using System;

using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        class TypeCheckingAndTranslatingPass : ISyntaxScriptVisitor
        {
            private Analyzer analyzer;

            public TypeCheckingAndTranslatingPass(Analyzer analyzer)
            {
                this.analyzer = analyzer;
            }

            public void VisitGlobalFuncDecl(S.FuncDecl funcDecl)
            {
                analyzer.AnalyzeFuncDecl(funcDecl);
            }

            public void VisitTopLevelStmt(S.Stmt stmt)
            {
                // do nothing
            }

            public void VisitTypeDecl(S.TypeDecl typeDecl)
            {
                analyzer.AnalyzeTypeDecl(typeDecl);
            }
        }

        void AnalyzeStructDecl(S.StructDecl structDecl)
        {
            throw new NotImplementedException();
        }
        
        void AnalyzeEnumDecl(S.EnumDecl enumDecl)
        {
            throw new NotImplementedException();
        }

        void AnalyzeTypeDecl(S.TypeDecl typeDecl)
        {
            switch(typeDecl)
            {
                case S.StructDecl structDecl:
                    AnalyzeStructDecl(structDecl);
                    break;

                case S.EnumDecl enumDecl:
                    AnalyzeEnumDecl(enumDecl);
                    break;

                default:
                    throw new UnreachableCodeException();
            }
        }
        
    }
}
