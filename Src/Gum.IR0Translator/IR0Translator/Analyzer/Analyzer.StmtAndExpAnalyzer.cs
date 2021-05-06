﻿using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;
using Pretune;

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
                var globalContext = this.globalContext;
                return ImmutableArray.CreateRange(typeExps, typeExp => globalContext.GetTypeValueByTypeExp(typeExp));
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

            void CheckParamTypes(S.ISyntaxNode nodeForErrorReport, ImmutableArray<TypeValue> parameters, ImmutableArray<TypeValue> args)
            {
                bool bFatal = false;

                if (parameters.Length != args.Length)
                {
                    globalContext.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport);
                    bFatal = true;
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!globalContext.IsAssignable(parameters[i], args[i]))
                    {
                        globalContext.AddError(A0402_Parameter_MismatchBetweenParamTypeAndArgType, nodeForErrorReport);
                        bFatal = true;
                    }
                }

                if (bFatal)
                    throw new FatalAnalyzeException();
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