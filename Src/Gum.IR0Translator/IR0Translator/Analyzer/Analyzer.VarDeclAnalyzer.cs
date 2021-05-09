using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Collections;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        partial struct VarDeclElemAnalyzer
        {
            GlobalContext globalContext;
            StmtAndExpAnalyzer stmtAndExpAnalyzer;

            public VarDeclElemAnalyzer(GlobalContext globalContext, CallableContext callableContext, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.stmtAndExpAnalyzer = new StmtAndExpAnalyzer(globalContext, callableContext, localContext);
            }
            
            [AutoConstructor]
            public partial struct VarDeclElementCoreResult
            {
                public R.VarDeclElement Elem { get; }
                public TypeValue TypeValue { get; }
            }

            public VarDeclElementCoreResult AnalyzeVarDeclElement(S.VarDeclElement elem, TypeValue declType)
            {
                if (elem.InitExp == null)
                {
                    // var x; 체크
                    if (declType is VarTypeValue)
                        globalContext.AddFatalError(A0101_VarDecl_CantInferVarType, elem);

                    var rtype = declType.GetRPath();
                    return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, null), declType);
                }
                else
                {
                    // var 처리
                    if (declType is VarTypeValue)
                    {
                        var initExpResult = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.InitExp, ResolveHint.None);
                        var rtype = initExpResult.TypeValue.GetRPath();
                        return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, initExpResult.Result), initExpResult.TypeValue);
                    }
                    else
                    {
                        var initExpResult = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.InitExp, ResolveHint.Make(declType));

                        if (!globalContext.IsAssignable(declType, initExpResult.TypeValue))
                            globalContext.AddFatalError(A0102_VarDecl_MismatchBetweenDeclTypeAndInitExpType, elem);

                        var rtype = declType.GetRPath();
                        return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, initExpResult.Result), declType);
                    }
                }
            }
        }
    }
}
