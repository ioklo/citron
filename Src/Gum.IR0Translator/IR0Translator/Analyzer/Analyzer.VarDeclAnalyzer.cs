﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Collections;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;
using Gum.Infra;

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

            public VarDeclElementCoreResult AnalyzeVarDeclElement(S.VarDeclElement elem, bool bRef, TypeValue declType)
            {
                if (elem.Initializer == null)
                {
                    // ref로 시작했으면 initializer가 꼭 있어야 합니다
                    if (bRef)
                        globalContext.AddFatalError(A0106_VarDecl_RefDeclNeedInitializer, elem);

                    // var x; 체크
                    if (declType is VarTypeValue)
                        globalContext.AddFatalError(A0101_VarDecl_CantInferVarType, elem);

                    var rtype = declType.GetRPath();
                    return new VarDeclElementCoreResult(new R.VarDeclElement.Normal(rtype, elem.VarName, null), declType);
                }
                else
                {
                    // var 처리
                    if (declType is VarTypeValue)
                    {
                        // ref var는 에러
                        if (bRef)
                            globalContext.AddFatalError(A0107_VarDecl_DontAllowVarWithRef, elem);

                        // var x = ref exp
                        if (elem.Initializer.Value.IsRef)
                        {
                            // ref 라면 
                            var initExpResult = stmtAndExpAnalyzer.AnalyzeExp(elem.Initializer.Value.Exp, ResolveHint.None);

                            // location만 허용
                            switch(initExpResult)
                            {
                                case ExpResult.Loc locResult:
                                    var relem = new R.VarDeclElement.Ref(elem.VarName, locResult.Result);
                                    return new VarDeclElementCoreResult(relem, locResult.TypeValue);

                                default:
                                    globalContext.AddFatalError(A0108_VarDecl_RefNeedLocation, elem);
                                    throw new UnreachableCodeException();
                            }
                        }
                        else
                        {
                            var initExpResult = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.Initializer.Value.Exp, ResolveHint.None);
                            var rtype = initExpResult.TypeValue.GetRPath();
                            return new VarDeclElementCoreResult(new R.VarDeclElement.Normal(rtype, elem.VarName, initExpResult.Result), initExpResult.TypeValue);
                        }
                    }
                    else
                    {
                        var initExpResult = stmtAndExpAnalyzer.AnalyzeExp_Exp(elem.InitExp, ResolveHint.Make(declType));
                        var castExpResult = stmtAndExpAnalyzer.CastExp_Exp(initExpResult, declType, elem.InitExp);

                        var rtype = declType.GetRPath();
                        return new VarDeclElementCoreResult(new R.VarDeclElement(rtype, elem.VarName, castExpResult.Result), declType);
                    }
                }
            }
        }
    }
}
