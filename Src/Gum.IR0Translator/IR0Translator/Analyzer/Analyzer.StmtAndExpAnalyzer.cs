using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;
using Pretune;
using Gum.Infra;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {
        partial struct StmtAndExpAnalyzer 
        {
            GlobalContext globalContext;
            CallableContext callableContext;
            LocalContext localContext;

            public StmtAndExpAnalyzer(GlobalContext globalContext, CallableContext callableContext, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.callableContext = callableContext;
                this.localContext = localContext;
            }
            
            ImmutableArray<TypeValue> GetTypeValues(ImmutableArray<S.TypeExp> typeExps)
            {
                var builder = ImmutableArray.CreateBuilder<TypeValue>(typeExps.Length);

                foreach (var typeExp in typeExps)
                {
                    var typeValue = globalContext.GetTypeValueByTypeExp(typeExp);
                    builder.Add(typeValue);
                }

                return builder.MoveToImmutable();
            }

            StmtAndExpAnalyzer NewAnalyzer()
            {
                var newLocalContext = localContext.NewLocalContext();
                return new StmtAndExpAnalyzer(globalContext, callableContext, newLocalContext);
            }            

            StmtAndExpAnalyzer NewAnalyzerWithLoop()
            {
                var newLocalContext = localContext.NewLocalContextWithLoop();
                return new StmtAndExpAnalyzer(globalContext, callableContext, newLocalContext);
            }

            StmtAndExpAnalyzer CloneAnalyzer()
            {
                var cloneContext = CloneContext.Make();
                var clonedGlobalContext = cloneContext.GetClone(globalContext);
                var clonedCallableContext = cloneContext.GetClone(callableContext);
                var clonedLocalContext = cloneContext.GetClone(localContext);

                return new StmtAndExpAnalyzer(clonedGlobalContext, clonedCallableContext, clonedLocalContext);
            }

            void UpdateAnalyzer(GlobalContext srcGlobalContext, CallableContext srcCallableContext, LocalContext srcLocalContext)
            {
                var updateContext = UpdateContext.Make();

                updateContext.Update(globalContext, srcGlobalContext);
                updateContext.Update(callableContext, srcCallableContext);
                updateContext.Update(localContext, srcLocalContext);
            }
            
            R.LocalVarDecl AnalyzeLocalVarDecl(S.VarDecl varDecl)
            {
                var varDeclAnalyzer = new VarDeclElemAnalyzer(globalContext, callableContext, localContext);

                var declType = globalContext.GetTypeValueByTypeExp(varDecl.Type);

                var relems = new List<R.VarDeclElement>();
                foreach (var elem in varDecl.Elems)
                {
                    if (localContext.DoesLocalVarNameExistInScope(elem.VarName))
                        globalContext.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem);

                    var result = varDeclAnalyzer.AnalyzeVarDeclElement(elem, declType);

                    localContext.AddLocalVarInfo(elem.VarName, result.TypeValue);
                    relems.Add(result.Elem);
                }

                return new R.LocalVarDecl(relems.ToImmutableArray());
            }
        }
    }
}