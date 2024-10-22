#include "pch.h"
#include "BodyContext.h"

#include <variant>

#include <Infra/Ptr.h>
#include <Infra/Variants.h>
#include <IR0/RLambdaMemberVarDecl.h>
#include <IR0/RArgument.h>
#include <IR0/RFuncDecl.h>
#include <IR0/RFuncDeclOuter.h>

#include "ScopeContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

// static RName thisName = RName_Normal("this");
BodyContext::BodyContext(const ModuleDeclsPtr& moduleDecls, BodyContextOuter&& outer, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
    : moduleDecls(moduleDecls), outer(std::move(outer)), bSeqFunc(bSeqFunc), funcReturn(std::move(funcReturn)), funcParams(std::move(funcParams)), bLastParamVariadic(bLastParamVariadic)
{
}

BodyContextPtr BodyContext::MakeLambdaBodyContext(const ScopeContextPtr& curScopeContext, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
{
    // lambda이므로 outer는 scopeContext이다
    return MakePtr<BodyContext>(moduleDecls, BodyContextOuter_ScopeContext { curScopeContext }, /*bSeqFunc*/ false, std::move(funcRet), std::move(funcParams), bLastParamVariadic);
}

//BodyContextPtr BodyContext::Clone(CloneContext& context)
//{
//    auto* scopeContextOuter = get_if<BodyContextOuter_ScopeContext>(&outer);
//
//    auto newOuterScopeContext = scopeContextOuter ? context.GetClone(scopeContextOuter->scopeContext) : nullptr;
//    auto newBodyContext = MakePtr<BodyContext>(moduleDecls, funcDecl, bSeqFunc, funcReturn, newOuterScopeContext, factory);
//
//    newBodyContext->lambdaMemberVarAndInitArgs = lambdaMemberVarAndInitArgs;
//    newBodyContext->lambdaDecls = lambdaDecls;
//
//    return newBodyContext;
//}
//
//
//void BodyContext::Update(const BodyContextPtr& src, UpdateContext& context)
//{
//    // 안변하는 것들은 assert
//    assert(moduleDecls == src->moduleDecls);
//    assert(funcDecl == src->funcDecl);
//    assert(bSeqFunc == src->bSeqFunc);
//
//    funcReturn = src->funcReturn;
//    context.Update(outerScopeContext, src->outerScopeContext);
//
//    assert(factory == src->factory);
//
//    lambdaMemberVarAndInitArgs = src->lambdaMemberVarAndInitArgs;
//    lambdaDecls = src->lambdaDecls;
//}
    
bool BodyContext::CanAccess(RDecl* target)
{
    // TODO: 현재 scope에서 access check

    return visit(overloaded {
        [target](BodyContextOuter_RFuncDeclOuter& outer) { return outer.decl->GetDecl()->CanAccess(target); },
        [target](BodyContextOuter_ScopeContext& outer) { return outer.scopeContext->bodyContext->CanAccess(target); }
    }, outer);
}

// 아직 FuncDecl이 안 만들어진 시기이기 때문에 이 함수가 만들어 질 수가 없다
//RFuncDecl* BodyContext::GetOutermostFuncDecl()
//{
//    return visit(overloaded {
//        [](BodyContextOuter_RFuncDeclOuter& outer) { return this; }, // ???
//        [](BodyContextOuter_ScopeContext& outer) { return outer.scopeContext->bodyContext->GetOutermostFuncDecl(); }
//    }, outer);
//}   


struct DeclTypeVisitor : ITypeExpVisitor<DeclTypeInfo>
    {
        BodyContext context;

        DeclTypeInfo Normal(TypeExp typeExp)
        {
            var type = context.MakeType(typeExp);
            return new DeclTypeInfo(DeclTypeInfoKind.Normal, type);
        }

        DeclTypeInfo ITypeExpVisitor<DeclTypeInfo>.VisitBoxPtr(BoxPtrTypeExp typeExp)
        {
            if (IsVarType(typeExp.InnerTypeExp))
                return new DeclTypeInfo(DeclTypeInfoKind.BoxPtrVar, type: null);

            return Normal(typeExp);
        }

        DeclTypeInfo ITypeExpVisitor<DeclTypeInfo>.VisitId(IdTypeExp typeExp)
        {
            if (IsVarType(typeExp))
                return new DeclTypeInfo(DeclTypeInfoKind.PlainVar, type: null);

            return Normal(typeExp);
        }

        // local var i = ...
        DeclTypeInfo ITypeExpVisitor<DeclTypeInfo>.VisitLocal(LocalTypeExp typeExp)
        {
            if (IsVarType(typeExp.InnerTypeExp))
                return new DeclTypeInfo(DeclTypeInfoKind.LocalInterfaceVar, type: null);

            return Normal(typeExp);
        }

        DeclTypeInfo ITypeExpVisitor<DeclTypeInfo>.VisitLocalPtr(LocalPtrTypeExp typeExp)
        {
            if (IsVarType(typeExp.InnerTypeExp))
                return new DeclTypeInfo(DeclTypeInfoKind.LocalPtrVar, type: null);

            return Normal(typeExp);
        }

        DeclTypeInfo ITypeExpVisitor<DeclTypeInfo>.VisitMember(MemberTypeExp typeExp)
        {
            return Normal(typeExp);
        }

        // var? 
        DeclTypeInfo ITypeExpVisitor<DeclTypeInfo>.VisitNullable(NullableTypeExp typeExp)
        {
            if (IsVarType(typeExp.InnerTypeExp))
                return new DeclTypeInfo(DeclTypeInfoKind.NullableVar, type: null);

            return Normal(typeExp);
        }
    }

    // 
    public DeclTypeInfo GetDeclTypeInfo(TypeExp typeExp)
    {
        var visitor = new DeclTypeVisitor(this);
        return typeExp.Accept<DeclTypeVisitor, DeclTypeInfo>(ref visitor);
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
            foreach(var module in bodyContext.moduleDeclSymbols)
            {
                var outerDeclSymbol = module.GetDeclSymbol(outerPath);
                if (outerDeclSymbol == null) continue;

                var outerSymbol = outerDeclSymbol.MakeOpenSymbol(bodyContext.symbolFactory);
                var symbolQueryResult = outerSymbol.QueryMember(name, typeArgs.Length);

                if (symbolQueryResult == null)
                    continue;

                else if (symbolQueryResult is SymbolQueryResult.MultipleCandidatesError multipleCandidatesError) // 에러가 났으면 무시하지 말고 리턴
                {
                    var builder = ImmutableArray.CreateBuilder<IntermediateExp>(multipleCandidatesError.Results.Length);
                    foreach(var result in multipleCandidatesError.Results)
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

            foreach(var memberVar in bodyContext.lambdaMemberVars)
            {
                if (memberVar.GetName().Equals(name))
                {
                    var memberVarSymbol = memberVar.MakeOpenSymbol(bodyContext.symbolFactory) as LambdaMemberVarSymbol;
                    Debug.Assert(memberVarSymbol != null);
                    candidates.Add(new IntermediateExp.LambdaMemberVar(memberVarSymbol));
                }
            }
        }

        static IntermediateExp ? QueryTypeVar(Name name, IDeclSymbolNode curOuterNode)
        {
            int typeParamCount = curOuterNode.GetTypeParamCount();
            for (int i = 0; i < typeParamCount; i++)
            {
                var typeParam = curOuterNode.GetTypeParam(i);
                if (typeParam.Equals(name))
                {
                    int baseTypeParamIndex = curOuterNode.GetOuterDeclNode() ? .GetTotalTypeParamCount() ? ? 0;
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
                    case ClassDeclSymbol classDeclSymbol :
                        var classType = classDeclSymbol.MakeOpenSymbol(bodyContext.symbolFactory).MakeType(bLocalInterface : false);
                        candidates.Add(new IntermediateExp.ThisVar(classType)); // C
                        break;

                        case StructDeclSymbol structDeclSymbol :
                            var structType = structDeclSymbol.MakeOpenSymbol(bodyContext.symbolFactory).MakeType(bLocalInterface : false);
                            var structPtrType = new LocalPtrType(structType); // S*
                            candidates.Add(new IntermediateExp.ThisVar(structPtrType));
                            break;

                        default:
                            break;
                }
            }
        }

        // 즉시 체인을 빠져나갈땐 exception, 찾지 못해서 다음으로 제어를 넘길땐 null리턴
        // ExpResult.NotFound를 쓰지 않는다
        public IntermediateExp ? Resolve()
        {
            TryQueryThis(name, typeArgs);
            TryQueryLambdaMemberVar(name, typeArgs.Length);
            TryQueryTypeVar(name, bodyContext.funcDeclSymbol);

            int count = candidates.GetCount();
            if (count == 1) return candidates.GetAt(0);
            if (1 < count)
            {
                var builder = ImmutableArray.CreateBuilder<IntermediateExp>(count);
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
                    case IntermediateExp.LocalVar localResult :
                    {
                        var initExp = new R.LoadExp(new R.LocalVarLoc(localResult.Name), localResult.Type);
                        Debug.Assert(initExp != null);

                        var initArg = new R.Argument.Normal(initExp);
                        var symbol = bodyContext.StageLambdaMemberVar(localResult.Type, localResult.Name, initArg);
                        return new IntermediateExp.LambdaMemberVar(symbol);
                    }

                    case IntermediateExp.LambdaMemberVar lambdaMemberResult :
                    {
                        var initExp = new R.LoadExp(new R.LambdaMemberVarLoc(lambdaMemberResult.Symbol), lambdaMemberResult.Symbol.GetDeclType());
                        Debug.Assert(initExp != null);

                        var initArg = new R.Argument.Normal(initExp);
                        var symbol = bodyContext.StageLambdaMemberVar(lambdaMemberResult.Symbol.GetDeclType(), lambdaMemberResult.Symbol.GetName(), initArg);
                        return new IntermediateExp.LambdaMemberVar(symbol);
                    }

                    case IntermediateExp.ThisVar thisResult :
                    {
                        // TODO: 워닝, struct의 this는 복사가 일어납니다. 원본과 다를 수 있습니다. ref this로 명시적으로 지정해주세요(?)
                        if (thisResult.Type is StructType)
                            throw new NotImplementedException();

                        var initExp = new R.LoadExp(new R.ThisLoc(), thisResult.Type);
                        Debug.Assert(initExp != null);

                        var initArg = new R.Argument.Normal(initExp);
                        var symbol = bodyContext.StageLambdaMemberVar(thisResult.Type, thisName, initArg);
                        return new IntermediateExp.LambdaMemberVar(symbol);
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
                    var builder = ImmutableArray.CreateBuilder<IntermediateExp>(count);
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
    public IntermediateExp ? ResolveIdentifier(Name name, ImmutableArray<IType> typeArgs)
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
    
    public void CommitLambdasToDeclSymbolTree()
    {
        foreach(var lambda in lambdaDs)
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
    public FuncReturn ? GetReturn()
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
    public ImmutableArray<IntermediateExp> Candidates { get; }
        public IdentifierResolverMultipleCandidatesException(ImmutableArray<IntermediateExp> candidates)
    {
        Candidates = candidates;
    }
}


} // namespace Citron::SyntaxIR0Translator
