using System;
using System.Diagnostics;
using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using Citron.Syntax;
using R = Citron.IR0;
using static Citron.Analysis.BodyMisc;

namespace Citron.Analysis;

class BodyContext : IMutable<BodyContext>
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
    ImmutableArray<LambdaDeclSymbol> lambdas;

    // 아래 둘은 크기가 같다
    ImmutableArray<LambdaMemberVarDeclSymbol> lambdaMemberVars;
    ImmutableArray<R.Argument> lambdaMemberVarInitArgs; 

    public BodyContext(ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols, SymbolFactory symbolFactory, 
        ScopeContext? outerScopeContext, IFuncDeclSymbol funcDeclSymbol, bool bSeqFunc, bool bSetReturn, FuncReturn? funcReturn)
    {
        Debug.Assert(!bSetReturn && funcReturn == null);

        this.moduleDeclSymbols = moduleDeclSymbols;
        this.symbolFactory = symbolFactory;
        
        this.outerScopeContext = outerScopeContext;
        this.funcDeclSymbol = funcDeclSymbol;
        this.bSeqFunc = bSeqFunc;
    }
    
    // 시퀀스 함수인가
    public bool IsSeqFunc()
    {
        return bSeqFunc;
    }

    BodyContext IMutable<BodyContext>.Clone(CloneContext context)
    {
        throw new NotImplementedException();
    }

    void IMutable<BodyContext>.Update(BodyContext src, UpdateContext context)
    {
        throw new NotImplementedException();
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
        return new TypeMakerByTypeExp(moduleDeclSymbols.AsEnumerable(), symbolFactory, funcDeclSymbol).MakeType(typeExp);
    }

    record struct IdentifierResolver(Name name, ImmutableArray<IType> typeArgs, BodyContext bodyContext)
    {   
        Candidates<ExpResult> candidates = new Candidates<ExpResult>();

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

                if (symbolQueryResult is SymbolQueryResult.Valid)
                {
                    var candidate = symbolQueryResult.MakeExpResult(typeArgs);
                    candidates.Add(candidate);
                }
                else if (symbolQueryResult is SymbolQueryResult.NotFound) continue;
                else if (symbolQueryResult is SymbolQueryResult.Error) // 에러가 났으면 무시하지 말고 리턴
                {
                    candidates.Clear();
                    candidates.Add(symbolQueryResult.MakeExpResult(typeArgs));
                    return;
                }
                else
                    throw new UnreachableCodeException();
            }
        }

        // funcDeclSymbol은 람다나 함수.        
        void TryQueryTypeVar(Name name, IFuncDeclSymbol funcDeclSymbol, Candidates<ExpResult> candidates)
        {
            var typeVarResult = QueryTypeVar(name, funcDeclSymbol);
            if (typeVarResult != null) candidates.Add(typeVarResult);
        }

        void TryQueryLambdaMemberVar(Name name, int typeArgCount, Candidates<ExpResult> candidates)
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

        static ExpResult.TypeVar? QueryTypeVar(Name name, IDeclSymbolNode curOuterNode)
        {
            int typeParamCount = curOuterNode.GetTypeParamCount();
            for (int i = 0; i < typeParamCount; i++)
            {
                var typeParam = curOuterNode.GetTypeParam(i);
                if (typeParam.Equals(name))
                {
                    int baseTypeParamIndex = curOuterNode.GetOuterDeclNode()?.GetTotalTypeParamCount() ?? 0;
                    var typeVarType = new TypeVarType(baseTypeParamIndex + i, typeParam);
                    return new ExpResult.TypeVar(typeVarType);
                    // 같은 이름이 있을수 없으므로 바로 종료
                }
            }

            return null;
        }

        void TryQueryThis(Name name, ImmutableArray<IType> typeArgs, Candidates<ExpResult> candidates)
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

        public ExpResult Resolve()
        {
            var candidates = new Candidates<ExpResult>();
            TryQueryThis(name, typeArgs, candidates);
            TryQueryLambdaMemberVar(name, typeArgs.Length, candidates);
            TryQueryTypeVar(name, bodyContext.funcDeclSymbol, candidates);

            var uniqueResult = candidates.GetUniqueResult();
            if (uniqueResult.IsFound(out var expResult))
                return expResult;
            else if (uniqueResult.IsMultipleError())
                return ExpResults.MultipleCandiates;
            else
                Debug.Assert(uniqueResult.IsNotFound()); // not found인 경우 계속 진행

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
                            var initExp = localResult.MakeIR0Exp();
                            Debug.Assert(initExp != null);

                            var initArg = new R.Argument.Normal(initExp);
                            var symbol = bodyContext.StageLambdaMemberVar(localResult.Type, localResult.Name, initArg);
                            return new ExpResult.LambdaMemberVar(symbol);
                        }

                    case ExpResult.LambdaMemberVar lambdaMemberResult:
                        {
                            var initExp = lambdaMemberResult.MakeIR0Exp();
                            Debug.Assert(initExp != null);

                            var initArg = new R.Argument.Normal(initExp);
                            var symbol = bodyContext.StageLambdaMemberVar(lambdaMemberResult.MemberVar.GetDeclType(), lambdaMemberResult.MemberVar.GetName(), initArg);
                            return new ExpResult.LambdaMemberVar(symbol);
                        }

                    case ExpResult.ThisVar thisResult:
                        {
                            // TODO: 워닝, struct의 this는 복사가 일어납니다. 원본과 다를 수 있습니다. ref this로 명시적으로 지정해주세요(?)
                            if (thisResult.Type is StructType)
                                throw new NotImplementedException();

                            var initExp = thisResult.MakeIR0Exp();
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

                uniqueResult = candidates.GetUniqueResult();
                if (uniqueResult.IsFound(out expResult))
                    return expResult;
                else if (uniqueResult.IsMultipleError())
                    return ExpResults.MultipleCandiates;

                // not found인 경우 계속 진행
                Debug.Assert(uniqueResult.IsNotFound());
                curOuterNode = curOuterNode.GetOuterDeclNode()!;
                candidates.Clear(); // 재사용
            }

            return ExpResults.NotFound;
        }
    }


    // 람다일 수도 있고, 함수일 수도 있다
    public ExpResult ResolveIdentifier(Name name, ImmutableArray<IType> typeArgs)
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
        var lambda = new LambdaDeclSymbol(funcDeclSymbol, new Name.Anonymous(lambdas.Length), parameters);
        lambdas = lambdas.Add(lambda); // staging

        BodyContext newBodyContext = (ret != null) 
            ? new BodyContext(moduleDeclSymbols, symbolFactory, outerScopeContext, lambda, bSeqFunc: false, bSetReturn: false, funcReturn: ret.Value)
            : new BodyContext(moduleDeclSymbols, symbolFactory, outerScopeContext, lambda, bSeqFunc: false, bSetReturn: false, funcReturn: null);

        var lambdaSymbol = lambda.MakeOpenSymbol(symbolFactory) as LambdaSymbol;
        Debug.Assert(lambdaSymbol != null);
        return (newBodyContext, lambdaSymbol);
    }

    public void CommitLambdasToDeclSymbolTree()
    {
        foreach (var lambda in lambdas)
            funcDeclSymbol.AddLambda(lambda);

        lambdas = default;
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

    public void SetReturn(bool bRef, IType retType)
    {
        bSetReturn = true;
        funcReturn = new FuncReturn(bRef, retType);
    }
}