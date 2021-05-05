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
        [AutoConstructor]
        partial struct GlobalVarDeclResult
        {
            public ImmutableArray<R.VarDeclElement> Elems { get; }
        }

        [AutoConstructor]
        partial struct LocalVarDeclResult
        {
            public R.LocalVarDecl VarDecl { get; }
        }

        partial struct VarDeclAnalyzer
        {
            GlobalContext globalContext;
            LocalContext localContext;

            public GlobalVarDeclResult AnalyzeGlobalVarDecl(S.VarDecl varDecl)
            {
                var declType = globalContext.GetTypeValueByTypeExp(varDecl.Type);

                var elems = new List<R.VarDeclElement>();
                foreach (var elem in varDecl.Elems)
                {
                    var result = AnalyzeInternalGlobalVarDeclElement(elem, declType);
                    elems.Add(result.VarDeclElement);
                }

                return new GlobalVarDeclResult(elems.ToImmutableArray());
            }

            public LocalVarDeclResult AnalyzeLocalVarDecl(S.VarDecl varDecl)
            {
                var declType = globalContext.GetTypeValueByTypeExp(varDecl.Type);

                var elems = new List<R.VarDeclElement>();
                foreach (var elem in varDecl.Elems)
                {
                    var result = AnalyzeLocalVarDeclElement(elem, declType);
                    elems.Add(result.VarDeclElement);
                }

                return new LocalVarDeclResult(new R.LocalVarDecl(elems.ToImmutableArray()));
            }

            VarDeclElementResult AnalyzeLocalVarDeclElement(S.VarDeclElement elem, TypeValue declType)
            {
                var name = elem.VarName;

                if (globalContext.DoesLocalVarNameExistInScope(name))
                    globalContext.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem);

                var result = AnalyzeVarDeclElementCore(elem, declType);

                globalContext.AddLocalVarInfo(name, result.TypeValue);
                return new VarDeclElementResult(result.Elem);
            }

            [AutoConstructor]
            partial struct VarDeclElementResult
            {
                public R.VarDeclElement VarDeclElement { get; }
            }

            VarDeclElementResult AnalyzeInternalGlobalVarDeclElement(S.VarDeclElement elem, TypeValue declType)
            {
                var name = elem.VarName;

                if (globalContext.DoesInternalGlobalVarNameExist(name))
                    globalContext.AddFatalError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);

                var result = AnalyzeVarDeclElementCore(elem, declType);

                globalContext.AddInternalGlobalVarInfo(name, result.TypeValue);
                return new VarDeclElementResult(result.Elem);
            }

            [AutoConstructor]
            partial struct VarDeclElementCoreResult
            {
                public R.VarDeclElement Elem { get; }
                public TypeValue TypeValue { get; }
            }

            VarDeclElementCoreResult AnalyzeVarDeclElementCore(S.VarDeclElement elem, TypeValue declType)
            {
                if (elem.InitExp == null)
                {
                    // var x; 체크
                    if (declType is VarTypeValue)
                        globalContext.AddFatalError(A0101_VarDecl_CantInferVarType, elem);

                    var rtype = declType.GetRType();
                    return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, null), declType);
                }
                else
                {
                    // var 처리
                    if (declType is VarTypeValue)
                    {
                        var initExpResult = AnalyzeExp_Exp(elem.InitExp, ResolveHint.None);
                        var rtype = initExpResult.TypeValue.GetRType();
                        return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, initExpResult.Exp), initExpResult.TypeValue);
                    }
                    else
                    {
                        var initExpResult = AnalyzeExp_Exp(elem.InitExp, ResolveHint.Make(declType));

                        if (!globalContext.IsAssignable(declType, initExpResult.TypeValue))
                            globalContext.AddFatalError(A0102_VarDecl_MismatchBetweenDeclTypeAndInitExpType, elem);

                        var rtype = declType.GetRType();
                        return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, initExpResult.Exp), declType);
                    }
                }
            }
        }
    }
}
