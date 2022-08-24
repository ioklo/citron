using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Citron.Log;

using S = Citron.Syntax;
using M = Citron.Module;
using Citron.Symbol;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        class FatalException : Exception
        {
        }
        
        // TypeExpInfo를 Syntax 트리에 추가한다
        public static void Evaluate(
            M.Name internalModuleName,
            S.Script script,
            ImmutableArray<ModuleDeclSymbol> referenceModules,
            ILogger logger)
        {
            var moduleSkel = SkeletonCollector.Collect(script);
            var localContext = new LocalContext();
            var globalContext = new GlobalContext(internalModuleName, referenceModules, moduleSkel, logger);

            var declVisitor = new DeclVisitor(localContext, globalContext);
            var stmtVisitor = new StmtVisitor(localContext, globalContext);

            try
            {

                foreach (var elem in script.Elements)
                {
                    switch (elem)
                    {
                        case S.TypeDeclScriptElement typeDeclElem:
                            declVisitor.VisitTypeDecl(typeDeclElem.TypeDecl);
                            break;

                        case S.GlobalFuncDeclScriptElement funcDeclElem:
                            declVisitor.VisitGlobalFuncDecl(funcDeclElem.FuncDecl);
                            break;

                        case S.StmtScriptElement stmtDeclElem:
                            stmtVisitor.VisitStmt(stmtDeclElem.Stmt);
                            break;
                    }
                }
            }
            catch(FatalException)
            {

            }

            if (logger.HasError)
            {
                // TODO: 검토
                throw new InvalidOperationException();
            }            
        }
    }
}
