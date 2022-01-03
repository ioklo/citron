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
using Gum.Analysis;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {
        partial struct StmtAndExpAnalyzer 
        {
            GlobalContext globalContext;
            ICallableContext callableContext;
            LocalContext localContext;

            public StmtAndExpAnalyzer(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.callableContext = callableContext;
                this.localContext = localContext;
            }
            
            ImmutableArray<TypeSymbol> GetTypeValues(ImmutableArray<S.TypeExp> typeExps)
            {
                var builder = ImmutableArray.CreateBuilder<TypeSymbol>(typeExps.Length);

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

            // var x = 3, y = ref i; 라면 
            R.LocalVarDecl AnalyzeLocalVarDecl(S.VarDecl varDecl)
            {
                var varDeclAnalyzer = new VarDeclElemAnalyzer(globalContext, callableContext, localContext);
                var declType = globalContext.GetTypeValueByTypeExp(varDecl.Type);

                var relems = new List<R.VarDeclElement>();
                foreach (var elem in varDecl.Elems)
                {
                    if (localContext.DoesLocalVarNameExistInScope(elem.VarName))
                        globalContext.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem);

                    var result = varDeclAnalyzer.AnalyzeVarDeclElement(bLocal: true, elem, varDecl.IsRef, declType);

                    // varDecl.IsRef는 syntax에서 체크한 것이므로, syntax에서 ref가 아니더라도 ref일 수 있으므로 result.Elem으로 검사를 해야한다.
                    localContext.AddLocalVarInfo(result.Elem is R.VarDeclElement.Ref, result.TypeValue, elem.VarName);
                    relems.Add(result.Elem);
                }

                return new R.LocalVarDecl(relems.ToImmutableArray());
            }
        }
    }
}