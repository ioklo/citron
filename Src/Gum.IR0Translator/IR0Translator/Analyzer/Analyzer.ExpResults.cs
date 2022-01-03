using Gum.Infra;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Analysis;
using S = Gum.Syntax;
using System;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        abstract record ExpResult
        {
            public record Namespace : ExpResult;
            public record Type(TypeSymbol TypeSymbol) : ExpResult;
            //public record Funcs(ItemValueOuter Outer, ImmutableArray<IModuleFuncDecl> FuncInfos, ImmutableArray<TypeSymbol> TypeArgs, R.Loc? Instance) : ExpResult;
            public record GlobalFuncs : ExpResult
            {
                // decls와 constructors는 크기가 같아야 한다
                ImmutableArray<GlobalFuncDeclSymbol> decls;
                ImmutableArray<Func<ImmutableArray<TypeSymbol>, GlobalFuncSymbol>> constructors;

                ImmutableArray<TypeSymbol> typeArgsForMatch; // partial typeArgs

                public (GlobalFuncSymbol Func, ImmutableArray<R.Argument> RArgs)
                    Match(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext, ImmutableArray<S.Argument> sargs, ISyntaxNode nodeForErrorReport)
                {
                    var parameterInfos = ImmutableArray.CreateRange(decls, decl => decl.GetParameters());

                    // outer가 없으므로 outerTypeEnv는 None이다
                    var result = FuncMatcher.Match(globalContext, callableContext, localContext, TypeEnv.None, parameterInfos, sargs, typeArgsForMatch);

                    switch(result)
                    {
                        case FuncMatchIndexResult.MultipleCandidates:
                            globalContext.AddFatalError(A0901_CallExp_MultipleCandidates, nodeForErrorReport);
                            throw new UnreachableCodeException();

                        case FuncMatchIndexResult.NotFound:
                            globalContext.AddFatalError(A0906_CallExp_NotFound, nodeForErrorReport);
                            throw new UnreachableCodeException();

                        case FuncMatchIndexResult.Success successResult:                            
                            var globalFunc = constructors[successResult.Index](successResult.TypeArgs);
                            return (globalFunc, successResult.Args);

                        default:
                            throw new UnreachableCodeException();
                    }

                }
            }

            public record ClassMemberFuncs
            {
                ImmutableArray<ClassMemberFuncDeclSymbol> decls;
                ImmutableArray<Func<ImmutableArray<TypeSymbol>, ClassMemberFuncSymbol>> constructors;
                ImmutableArray<ITypeSymbolNode> typeArgsForMatch;

                public (ClassMemberFuncSymbol Func, ImmutableArray<R.Argument> RArgs) 
                    Match(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext, ImmutableArray<Argument> sargs)
                {
                    // outer가 없으므로 outerTypeEnv는 None이다
                    var result = FuncMatcher.Match(globalContext, callableContext, localContext, TypeEnv.None, decls, sargs, typeArgsForMatch);

                    switch (result)
                    {
                        case FuncMatchIndexResult.MultipleCandidates:
                            globalContext.AddFatalError(A0901_CallExp_MultipleCandidates, nodeForErrorReport);
                            throw new UnreachableCodeException();

                        case FuncMatchIndexResult.NotFound:
                            globalContext.AddFatalError(A0906_CallExp_NotFound, nodeForErrorReport);
                            throw new UnreachableCodeException();

                        case FuncMatchIndexResult.Success successResult:
                            var globalFunc = constructors[successResult.Index](successResult.TypeArgs);
                            return (globalFunc, successResult.Args);

                        default:
                            throw new UnreachableCodeException();
                    }
                }
            }

            public record EnumElem(EnumElemSymbol EnumElemSymbol) : ExpResult;
            public record Exp(R.Exp Result, TypeSymbol TypeSymbol) : ExpResult;
            public record Loc(R.Loc Result, TypeSymbol TypeSymbol) : ExpResult;
        }
    }
}
