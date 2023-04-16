using System;
using System.Diagnostics;
using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using Citron.Syntax;
using R = Citron.IR0;
using static Citron.Analysis.BodyMisc;
using Pretune;
using System.Runtime.Serialization;

namespace Citron.Analysis;

[AutoConstructor]
partial class BodyContext : IMutable<BodyContext>
{
    static Name thisName = new Name.Normal("this");

    ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols;
    SymbolFactory symbolFactory;
    
    ScopeContext? outerScopeContext;  // 람다라면, 람다를 선언한 Scope
    IFuncDeclSymbol funcDeclSymbol;   // decl space
    
    bool bSeqFunc;

    bool bSetReturn;
    FuncReturn? funcReturn;

    // 람다 관련
    ImmutableArray<LambdaDeclSymbol> lambdaDs;

    // 아래 둘은 크기가 같다
    ImmutableArray<LambdaMemberVarDeclSymbol> lambdaMemberVars;
    ImmutableArray<R.Argument> lambdaMemberVarInitArgs; 

    public BodyContext(ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols, SymbolFactory symbolFactory, 
        ScopeContext? outerScopeContext, IFuncDeclSymbol funcDeclSymbol, bool bSeqFunc, bool bSetReturn, FuncReturn? funcReturn)
    {
        this.moduleDeclSymbols = moduleDeclSymbols;
        this.symbolFactory = symbolFactory;
        
        this.outerScopeContext = outerScopeContext;
        this.funcDeclSymbol = funcDeclSymbol;
        this.bSeqFunc = bSeqFunc;

        Debug.Assert(!(!bSetReturn && funcReturn != null));
        this.bSetReturn = bSetReturn;
        this.funcReturn = funcReturn;
    }
    
    // 시퀀스 함수인가
    public bool IsSeqFunc()
    {
        return bSeqFunc;
    }

    BodyContext IMutable<BodyContext>.Clone(CloneContext context)
    {
        var newOuterScopeContext = outerScopeContext != null ? context.GetClone(outerScopeContext) : null;

        return new BodyContext(
            moduleDeclSymbols,
            symbolFactory,
            newOuterScopeContext,
            funcDeclSymbol,
            bSeqFunc,
            bSetReturn,
            funcReturn,
            lambdaDs,
            lambdaMemberVars,
            lambdaMemberVarInitArgs
        );
    }

    void IMutable<BodyContext>.Update(BodyContext src, UpdateContext context)
    {
        this.moduleDeclSymbols = src.moduleDeclSymbols;
        this.symbolFactory = src.symbolFactory;

        // outerScopeContext를 직접 변경할 일이 없으므로
        // 둘다 null이거나, 둘다 null이 아니라고 가정한다
        if (src.outerScopeContext != null)
        {   
            Debug.Assert(outerScopeContext != null);
            context.Update(outerScopeContext, src.outerScopeContext);
        }
        else
        {
            Debug.Assert(outerScopeContext == null);
        }
        this.funcDeclSymbol = src.funcDeclSymbol;
        this.bSeqFunc = src.bSeqFunc;
        this.bSetReturn = src.bSetReturn;
        this.funcReturn = src.funcReturn;
        this.lambdaDs = src.lambdaDs;
        this.lambdaMemberVars = src.lambdaMemberVars;
        this.lambdaMemberVarInitArgs = src.lambdaMemberVarInitArgs;
    }

    public IFuncDeclSymbol GetFuncDeclSymbol()
    {
        return funcDeclSymbol;
    }

    public bool CanAccess(ISymbolNode node)
    {
        return funcDeclSymbol.CanAccess(node.GetDeclSymbolNode());
    }

    internal IFuncDeclSymbol GetOutermostFuncDeclSymbol()
    {
        if (outerScopeContext != null)
            return outerScopeContext.GetOutermostFuncDeclSymbol();

        return funcDeclSymbol;
    }

    public IType MakeType(TypeExp typeExp)
    {   
        return TypeMakerByTypeExp.MakeType(moduleDeclSymbols.AsEnumerable(), symbolFactory, funcDeclSymbol, typeExp);
    }

    record struct IdentifierResolver(Name name, ImmutableArray<IType> typeArgs, BodyContext bodyContext)
    {   
        Candidates<IntermediateExp> candidates = new Candidates<IntermediateExp>();

        void TryQueryMember(IDeclSymbolNode curOuterNode)
        {
            // 1. 타입 인자에서 찾기 (타입 인자는 declSymbol이 없으므로 리턴값은 Symbol이어야 한다)
            // T => X<>의 TypeVar T
            if (typeArgs.Length == 0)
            {
                var outerTypeVarResult = QueryTypeVar(name, curOuterNode);
                if (outerTypeVarResult != null)
                    candidates.Add(outerTypeVarResult);
            }

            // class X<T> { class Y<U> {
            //     void F<V, W>(W w) { }
            //     void F<T>() { }
            // }
            //
            // F<int> 는 둘다 지칭 가능하므로 
            // F<int> => (X<T>.Y<U>, [(F, 2, [W]), (F, 1, [])], [int])
            var outerPath = curOuterNode.GetDeclSymbolId().Path;
            foreach (var module in bodyContext.moduleDeclSymbols)
            {
                var outerDeclSymbol = module.GetDeclSymbol(outerPath);
                if (outerDeclSymbol == null) continue;

                var outerSymbol = outerDeclSymbol.MakeOpenSymbol(bodyContext.symbolFactory);
                var symbolQueryResult = outerSymbol.QueryMember(name, typeArgs.Length);

                if (symbolQueryResult == null) 
                    continue;

                else if (symbolQueryResult is SymbolQueryResult.MultipleCandidatesError multipleCandidatesError) // 에러가 났으면 무시하지 말고 리턴
                {
                    var builder = ImmutableArray.CreateBuilder<ExpResult>(multipleCandidatesError.Results.Length);
                    foreach (var result in multipleCandidatesError.Results)
                    {
                        var expResult = SymbolQueryResultExpResultTranslator.Translate(result, typeArgs); // NOTICE: 여기서 exception이 발생할 수 있다
                        builder.Add(expResult);
                    }

                    throw new IdentifierResolverMultipleCandidatesException(builder.MoveToImmutable());
                }
                else // 에러가 없는 경우
                {
                    candidates.Clear();
                    var candidate = SymbolQueryResultExpResultTranslator.Translate(symbolQueryResult, typeArgs);
                    candidates.Add(candidate);
                    return;
                }
            }
        }

        // funcDeclSymbol은 람다나 함수.        
        void TryQueryTypeVar(Name name, IFuncDeclSymbol funcDeclSymbol)
        {
            var typeVarResult = QueryTypeVar(name, funcDeclSymbol);
            if (typeVarResult != null) candidates.Add(typeVarResult);
        }

        void TryQueryLambdaMemberVar(Name name, int typeArgCount)
        {
            // 0. lambdaMemberVar를 찾는다
            if (typeArgCount != 0) return;

            foreach (var memberVar in bodyContext.lambdaMemberVars)
            {
                if (memberVar.GetName().Equals(name))
                {
                    var memberVarSymbol = memberVar.MakeOpenSymbol(bodyContext.symbolFactory) as LambdaMemberVarSymbol;
                    Debug.Assert(memberVarSymbol != null);
                    candidates.Add(new ExpResult.LambdaMemberVar(memberVarSymbol));
                }
            }
        }

        static IntermediateExp? QueryTypeVar(Name name, IDeclSymbolNode curOuterNode)
        {
            int typeParamCount = curOuterNode.GetTypeParamCount();
            for (int i = 0; i < typeParamCount; i++)
            {
                var typeParam = curOuterNode.GetTypeParam(i);
                if (typeParam.Equals(name))
                {
                    int baseTypeParamIndex = curOuterNode.GetOuterDeclNode()?.GetTotalTypeParamCount() ?? 0;
                    var typeVarType = new TypeVarType(baseTypeParamIndex + i, typeParam);
                    return new IntermediateExp.TypeVar(typeVarType);
                    // 같은 이름이 있을수 없으므로 바로 종료
                }
            }

            return null;
        }

        void TryQueryThis(Name name, ImmutableArray<IType> typeArgs)
        {
            // this검색, local변수 this를 만들게 되면 그것보다 뒤에 있다
            if (typeArgs.Length == 0 && name.Equals(thisName))
            {
                var funcOuter = bodyContext.funcDeclSymbol.GetOuterDeclNode();
                switch (funcOuter)
                {
                    case ClassDeclSymbol classDeclSymbol:
                        candidates.Add(new ExpResult.ThisVar(((ITypeSymbol)classDeclSymbol).MakeType()));
                        break;

                    case StructDeclSymbol structDeclSymbol:
                        candidates.Add(new ExpResult.ThisVar(((ITypeSymbol)structDeclSymbol).MakeType()));
                        break;

                    default:
                        break;
                }
            }
        }

        // 즉시 체인을 빠져나갈땐 exception, 찾지 못해서 다음으로 제어를 넘길땐 null리턴
        // ExpResult.NotFound를 쓰지 않는다
        public IntermediateExp? Resolve() 
        {   
            TryQueryThis(name, typeArgs);
            TryQueryLambdaMemberVar(name, typeArgs.Length);
            TryQueryTypeVar(name, bodyContext.funcDeclSymbol);

            int count = candidates.GetCount();
            if (count == 1) return candidates.GetAt(0);
            if (1 < count)
            {
                var builder = ImmutableArray.CreateBuilder<ExpResult>(count);
                for (int i = 0; i < count; i++)
                    builder.Add(candidates.GetAt(i));

                throw new IdentifierResolverMultipleCandidatesException(builder.MoveToImmutable());
            }

            // 못찾았으면 아래로 계속 진행
            Debug.Assert(count == 0);
            
            // 2. outerScopeContext가 있으면 거기에서, 아니라면 bodyContext의 outer를 찾아본다
            if (bodyContext.outerScopeContext != null)
            {
                var result = bodyContext.outerScopeContext.ResolveIdentifier(name, typeArgs);

                // 람다 멤버에 없었으므로 (TryQueryLambdaMemberVar) 람다에 추가한다
                // 로컬과 람다 멤버, this만 감싸는 대상이다
                switch (result)
                {
                    case ExpResult.LocalVar localResult:
                        {
                            var initExp = new R.LoadExp(new R.LocalVarLoc(localResult.Name), localResult.Type);
                            Debug.Assert(initExp != null);

                            var initArg = new R.Argument.Normal(initExp);
                            var symbol = bodyContext.StageLambdaMemberVar(localResult.Type, localResult.Name, initArg);
                            return new ExpResult.LambdaMemberVar(symbol);
                        }

                    case ExpResult.LambdaMemberVar lambdaMemberResult:
                        {
                            var initExp = new R.LoadExp(new R.LambdaMemberVarLoc(lambdaMemberResult.Symbol), lambdaMemberResult.Symbol.GetDeclType());
                            Debug.Assert(initExp != null);

                            var initArg = new R.Argument.Normal(initExp);
                            var symbol = bodyContext.StageLambdaMemberVar(lambdaMemberResult.Symbol.GetDeclType(), lambdaMemberResult.Symbol.GetName(), initArg);
                            return new ExpResult.LambdaMemberVar(symbol);
                        }

                    case ExpResult.ThisVar thisResult:
                        {
                            // TODO: 워닝, struct의 this는 복사가 일어납니다. 원본과 다를 수 있습니다. ref this로 명시적으로 지정해주세요(?)
                            if (thisResult.Type is StructType)
                                throw new NotImplementedException();

                            var initExp = new R.LoadExp(new R.ThisLoc(), thisResult.Type);
                            Debug.Assert(initExp != null);

                            var initArg = new R.Argument.Normal(initExp);
                            var symbol = bodyContext.StageLambdaMemberVar(thisResult.Type, thisName, initArg);
                            return new ExpResult.LambdaMemberVar(symbol);
                        }

                    // 나머지는 그대로 리턴
                    default:
                        return result;
                }
            }

            // 3. 아니라면, funcDecl의 outer에서 찾기 시작한다
            var curOuterNode = bodyContext.funcDeclSymbol.GetOuterDeclNode()!; // 전역함수도 모듈에 속하기 때문에 null이 아니다
            while (curOuterNode != null)
            {
                TryQueryMember(curOuterNode);

                count = candidates.GetCount();
                if (count == 1) return candidates.GetAt(0);
                else if (1 < count)
                {
                    var builder = ImmutableArray.CreateBuilder<ExpResult>(count);
                    for (int i = 0; i < count; i++)
                        builder.Add(candidates.GetAt(i));

                    throw new IdentifierResolverMultipleCandidatesException(builder.MoveToImmutable());
                }

                // not found인 경우 계속 진행
                Debug.Assert(count == 0);

                curOuterNode = curOuterNode.GetOuterDeclNode()!;
                candidates.Clear(); // 재사용
            }

            return null;
        }
    }


    // 람다일 수도 있고, 함수일 수도 있다
    public IntermediateExp? ResolveIdentifier(Name name, ImmutableArray<IType> typeArgs)
    {
        // 0. 인자 먼저 (ScopeContext에서 이미 검색했으니 스킵)
        // 1. 함수(람다 등) 멤버변수
        // 1. 함수(글로벌, 클래스 멤버, 구초제 멤버) 타입인자
        // 1. 'this' (클래스, 구조체에만 존재)

        return new IdentifierResolver(name, typeArgs, this).Resolve();
    }    
    
    public LambdaMemberVarSymbol StageLambdaMemberVar(IType type, Name name, R.Argument initArg)
    {
        var lambdaDeclSymbol = funcDeclSymbol as LambdaDeclSymbol;
        Debug.Assert(lambdaDeclSymbol != null);

        var memberVarDeclSymbol = new LambdaMemberVarDeclSymbol(lambdaDeclSymbol, type, name);
        lambdaMemberVars = lambdaMemberVars.Add(memberVarDeclSymbol);
        lambdaMemberVarInitArgs = lambdaMemberVarInitArgs.Add(initArg);

        return (LambdaMemberVarSymbol)memberVarDeclSymbol.MakeOpenSymbol(symbolFactory);
    }
    
    // 여기서 인자로 들어온 ret는 null이면 아직 모른다는 뜻 (constructor라는 뜻이 아님)
    public (BodyContext, LambdaSymbol) MakeLambdaBodyContext(ScopeContext outerScopeContext, FuncReturn? ret, ImmutableArray<FuncParameter> parameters)
    {
        var lambdaD = new LambdaDeclSymbol(funcDeclSymbol, new Name.Anonymous(lambdaDs.Length), parameters);
        lambdaDs = lambdaDs.Add(lambdaD); // staging

        BodyContext newBodyContext = (ret != null) 
            ? new BodyContext(moduleDeclSymbols, symbolFactory, outerScopeContext, lambdaD, bSeqFunc: false, bSetReturn: true, funcReturn: ret.Value)
            : new BodyContext(moduleDeclSymbols, symbolFactory, outerScopeContext, lambdaD, bSeqFunc: false, bSetReturn: false, funcReturn: null);

        var lambda = (LambdaSymbol)lambdaD.MakeOpenSymbol(symbolFactory);
        return (newBodyContext, lambda);
    }

    public void CommitLambdasToDeclSymbolTree()
    {
        foreach (var lambda in lambdaDs)
            funcDeclSymbol.AddLambda(lambda);

        lambdaDs = default;
    }

    public ImmutableArray<R.Argument> MakeLambdaArgs()
    {
        return lambdaMemberVarInitArgs;
    }

    // 리턴값 관련 
    public bool IsSetReturn()
    {
        return bSetReturn;
    }

    // constructor라면 null
    public FuncReturn? GetReturn()
    {
        Debug.Assert(bSetReturn);
        return funcReturn;
    }

    public void SetReturn(IType retType)
    {
        bSetReturn = true;
        funcReturn = new FuncReturn(retType);
    }
}

class IdentifierResolverMultipleCandidatesException : Exception
{
    public ImmutableArray<ExpResult> Candidates { get; }
    public IdentifierResolverMultipleCandidatesException(ImmutableArray<ExpResult> candidates)
    {
        Candidates = candidates;
    }
}