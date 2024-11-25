#include "pch.h"
#include "StructTranslation.h"

#include <Infra/Unreachable.h>
#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>

#include "SkeletonPhaseContext.h"
#include "MemberDeclPhaseContext.h"
#include "EnumTranslation.h"
#include "SStmtToNStmtTranslation.h"
#include "ScopeContext.h"
#include "BodyPhaseContext.h"
#include "TranslationContext.h"
#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

// forward declaration
void AddStructConstructor_MemberDeclPhase(const shared_ptr<NStructConstructorDecl>& nConstructor, const shared_ptr<SStructConstructorDecl>& sConstructor, MemberDeclPhaseContext& context);
void AddStructConstructor_BodyPhase(const shared_ptr<NStructConstructorDecl>& nConstructor, const shared_ptr<SStructConstructorDecl>& sConstructor, BodyPhaseContext& context);
void AddStructMemberFunc_MemberDeclPhase(const shared_ptr<NStructMemberFuncDecl>& nMemberFunc, const shared_ptr<SStructMemberFuncDecl>& sMemberFunc, MemberDeclPhaseContext& context);
void AddStructMemberFunc_BodyPhase(const shared_ptr<NStructMemberFuncDecl>& nMemberFunc, const shared_ptr<SStructMemberFuncDecl>& sMemberFunc, BodyPhaseContext& context);
void AddStructMemberVar_MemberDeclPhase(vector<shared_ptr<NStructMemberVarDecl>>&& nMemberVars, const NDeclPtr& nDecl, STypeExpPtr sTypeExp, MemberDeclPhaseContext& context);
void AddStruct_TrivialConstructorPhase(const shared_ptr<NStructDecl>& nStruct);

RAccessor MakeStructMemberAccessor(optional<SAccessModifier> accessModifier) // throws FatalException
{
    if (!accessModifier) return RAccessor::Public;

    switch (*accessModifier)
    {
    case SAccessModifier::Private: return RAccessor::Private;
    case SAccessModifier::Protected: throw NotImplementedException();
    case SAccessModifier::Public: throw NotImplementedException();
    }

    unreachable();
}

#pragma region Constructor

void AddStructConstructor(const shared_ptr<NStructDecl>& nStruct, shared_ptr<SStructConstructorDecl>&& sConstructor, SkeletonPhaseContext& context)
{
    auto accessModifier = MakeStructMemberAccessor(sConstructor->accessModifier);

    // TODO: 타이프 쳐서 만들어진 constructor는 'trivial' 표시를 하기 전까지는 trivial로 인식하지 않는다. 지금은 false로 표기
    // 그리고 컴파일러가 trivial 조건을 체크해서 에러를 낼 수도 있다 (하위 타입의 trivial constructor가 이 constructor를 참조하지 않는다)
    auto nConstructor = MakePtr<NStructConstructorDecl>(nStruct, accessModifier, false);
    context.AddMemberDeclPhaseTask([nConstructor, sConstructor](MemberDeclPhaseContext& context) {
        AddStructConstructor_MemberDeclPhase(nConstructor, sConstructor, context);
    });

    nStruct->AddConstructor(move(nConstructor));
}

void AddStructConstructor_MemberDeclPhase(const shared_ptr<NStructConstructorDecl>& nConstructor, const shared_ptr<SStructConstructorDecl>& sConstructor, MemberDeclPhaseContext& context)
{
    // Constructor는 Type Parameter가 없으므로 파라미터를 만들 때, 상위(struct) declSymbol을 넘긴다
    auto [parameters, bLastParamVariadic] = context.MakeParameters(nConstructor, sConstructor->parameters);

    nConstructor->InitFuncParameters(move(parameters), bLastParamVariadic);

    context.AddBodyPhaseTask([nConstructor, sConstructor](BodyPhaseContext& context) {
        AddStructConstructor_BodyPhase(nConstructor, sConstructor, context);
    });
}

void AddStructConstructor_BodyPhase(const shared_ptr<NStructConstructorDecl>& nConstructor, const shared_ptr<SStructConstructorDecl>& sConstructor, BodyPhaseContext& context)
{
    auto translationContext = context.MakeTranslationContext();

    vector<NStmtPtr> nStmts;
    if (!TranslateSBodyToNStmts(sConstructor->body, &nStmts, translationContext))
    {
        context.MarkFailed();
        return;
    }

    nConstructor->InitBody(std::move(nStmts));
}

#pragma endregion Constructor

#pragma region MemberFunc

void AddStructMemberFunc(const shared_ptr<NStructDecl>& nStruct, shared_ptr<SStructMemberFuncDecl>&& sMemberFunc, SkeletonPhaseContext& context)
{
    auto accessor = MakeStructMemberAccessor(sMemberFunc->accessModifier);
    auto typeParams = MakeTypeParams(sMemberFunc->typeParams);

    // TODO: bSequence
    auto nMemberFunc = MakePtr<NStructMemberFuncDecl>(nStruct, accessor, sMemberFunc->name, move(typeParams), sMemberFunc->bStatic);

    context.AddMemberDeclPhaseTask([nMemberFunc, sMemberFunc](MemberDeclPhaseContext& context) {
        AddStructMemberFunc_MemberDeclPhase(nMemberFunc, sMemberFunc, context);
    });

    nStruct->AddMemberFunc(move(nMemberFunc));
}

void AddStructMemberFunc_MemberDeclPhase(const shared_ptr<NStructMemberFuncDecl>& nMemberFunc, const shared_ptr<SStructMemberFuncDecl>& sMemberFunc, MemberDeclPhaseContext& context)
{
    auto rRetType = context.MakeType(sMemberFunc->retType, nMemberFunc);
    auto [rParameters, bLastParamVariadic] = context.MakeParameters(nMemberFunc, sMemberFunc->parameters);

    nMemberFunc->InitFuncReturnAndParams(move(rRetType), move(rParameters), bLastParamVariadic);

    context.AddBodyPhaseTask([nMemberFunc, sMemberFunc](BodyPhaseContext& context) {
        AddStructMemberFunc_BodyPhase(nMemberFunc, sMemberFunc, context);
    });
}

void AddStructMemberFunc_BodyPhase(const shared_ptr<NStructMemberFuncDecl>& nMemberFunc, const shared_ptr<SStructMemberFuncDecl>& sMemberFunc, BodyPhaseContext& context)
{
    auto translationContext = context.MakeTranslationContext();
    std::vector<NStmtPtr> nStmts;

    if (!TranslateSBodyToNStmts(sMemberFunc->body, &nStmts, translationContext))
    {
        context.MarkFailed();
        return;
    }

    nMemberFunc->InitBody(std::move(nStmts));
}

#pragma endregion MemberFunc

#pragma region MemberVar

void AddStructMemberVar(const shared_ptr<NStructDecl>& nStruct, SStructMemberVarDecl& sMemberVar, SkeletonPhaseContext& context)
{
    auto accessor = MakeStructMemberAccessor(sMemberVar.accessModifier);

    // TODO: bStatic 지원
    vector<shared_ptr<NStructMemberVarDecl>> nMemberVars;
    nMemberVars.reserve(sMemberVar.varNames.size());

    for (auto& varName : sMemberVar.varNames)
    {
        auto nMemberVar = MakePtr<NStructMemberVarDecl>(nStruct, accessor, false, varName);
        nMemberVars.push_back(nMemberVar); // for lazy-init
        nStruct->AddMemberVar(move(nMemberVar));
    }

    context.AddMemberDeclPhaseTask([nMemberVars = std::move(nMemberVars), varType = sMemberVar.varType, nStruct](MemberDeclPhaseContext& context) mutable {
        AddStructMemberVar_MemberDeclPhase(std::move(nMemberVars), std::move(nStruct), varType, context);
    });
}

void AddStructMemberVar_MemberDeclPhase(vector<shared_ptr<NStructMemberVarDecl>>&& rMemberVars, const NDeclPtr& rDecl, STypeExpPtr sTypeExp, MemberDeclPhaseContext& context)
{
    auto declType = context.MakeType(sTypeExp, rDecl);
    for (auto& rMemberVar : rMemberVars)
        rMemberVar->InitDeclType(declType);
}

#pragma endregion MemberVar
 
class StructMemberDeclVisitor : public SStructMemberDeclVisitor
{
    shared_ptr<NStructDecl> nStructDecl; // visit 중 옮겨질 수 있다 (visit이 한번만 불릴 것이므로)
    SStructMemberDeclPtr sharedMemberDecl;
    SkeletonPhaseContext& context;

public:
    StructMemberDeclVisitor(shared_ptr<NStructDecl> nStructDecl, SStructMemberDeclPtr sharedMemberDecl, SkeletonPhaseContext& context)
        : nStructDecl(move(nStructDecl)), sharedMemberDecl(move(sharedMemberDecl)), context(context)
    { }

    // Inherited via SStructMemberDeclVisitor
    void Visit(SClassDecl& decl) override 
    { 
        // ClassTranslation을 만들어야 한다
        static_assert(false); 
    }

    void Visit(SStructDecl& decl) override
    {
        auto sharedStructDecl = dynamic_pointer_cast<SStructDecl>(sharedMemberDecl);
        auto nStruct = MakeStruct(nStructDecl, move(sharedStructDecl), MakeStructMemberAccessor, context);
        nStructDecl->AddType(std::move(nStruct));
    }

    void Visit(SEnumDecl& decl) override
    {
        auto nEnum = MakeEnum(nStructDecl, decl, MakeStructMemberAccessor, context);
        nStructDecl->AddType(std::move(nEnum));
    }

    void Visit(SStructConstructorDecl& decl) override
    {
        auto sharedDecl = dynamic_pointer_cast<SStructConstructorDecl>(sharedMemberDecl);
        AddStructConstructor(nStructDecl, move(sharedDecl), context);
    }

    void Visit(SStructMemberFuncDecl& decl) override
    {
        auto sharedDecl = dynamic_pointer_cast<SStructMemberFuncDecl>(sharedMemberDecl);
        AddStructMemberFunc(nStructDecl, move(sharedDecl), context);
    }

    void Visit(SStructMemberVarDecl& decl) override
    {
        AddStructMemberVar(nStructDecl, decl, context);
    }
};

// RStructDecl or MStructDecl
// std::variant<std::monostate, std::shared_ptr<RStructDecl>, std::shared_ptr<MStructDecl>> 
// RStructDecl, MStructDecl
// RStructDecl = RInternalStructDecl, RExternalStructDecl

// MStructDecl이 있고, RStructDecl은 Body가 있는 버전과 없는 버전으로 구성될 수 있다

// 필요한 것만 IRStructDecl
// RStructDecl, RExStructDecl

void AddStruct_MemberDeclPhase(const shared_ptr<NStructDecl>& nStruct, const shared_ptr<SStructDecl>& sStruct, MemberDeclPhaseContext& context)
{
    // 유일한 베이스 타입은 struct인데, 외부에서 선언된 struct일수도 있고, 조합 타입일 수도 있다 (사실 조합타입이 될 가능성은 거의 없어보인다)
    RTypePtr rBaseStruct = nullptr; // nullable

    // 나머지는 interface들이다
    vector<RTypePtr> rInterfaces;

    for (auto& sType : sStruct->baseTypes)
    {
        auto rType = context.MakeType(sType, nStruct);
        auto rTypeKind = rType->GetCustomTypeKind();

        if (rTypeKind == RCustomTypeKind::Struct)
        {
            // 두개 이상의 struct를 상속받으려고 했다면, 에러 처리
            if (rBaseStruct != nullptr)
                throw NotImplementedException();

            rBaseStruct = std::move(rType);
        }
        else if (rTypeKind == RCustomTypeKind::Interface)
        {
            rInterfaces.push_back(std::move(rType));
        }
        else
        {
            // 다른 타입은 struct의 basetype자리에 올 수 없습니다 에러 출력
            throw NotImplementedException();
        }
    }

    nStruct->InitBaseTypes(rBaseStruct, std::move(rInterfaces));

    // base의 TrivialConstructor가 다 만들어 졌을 때, 수행하는 작업
    context.AddTrivialConstructorPhaseTask([nStruct]() {
        AddStruct_TrivialConstructorPhase(nStruct);
    });
}

bool IsMatchStructTrivialConstructorParameters(NStructConstructorDecl& nConstructor, vector<RFuncParameter>& baseConstructorParameters)
{
    static_assert(false);

    //int baseParamCount = baseConstructor != null ? baseConstructor->GetParameterCount() : 0;
    //int paramCount = constructorDecl.GetParameterCount();
    //int memberVarCount = declSymbol.GetMemberVarCount();

    //if (memberVarCount != paramCount) return false;

    //// constructorDecl의 앞부분이 baseConstructor와 일치하는지를 봐야 한다
    //for (int i = 0; i < baseParamCount; i++)
    //{
    //    Debug.Assert(baseConstructor != null);

    //    var baseParameter = baseConstructor.GetParameter(i);
    //    var parameter = constructorDecl.GetParameter(i);

    //    if (!BodyMisc.FuncParameterEquals(baseParameter, parameter)) return false;
    //}

    //// baseParam을 제외한 뒷부분이 memberVarType과 맞는지 봐야 한다
    //for (int i = 0; i < paramCount; i++)
    //{
    //    var memberVarType = declSymbol.GetMemberVar(i).GetDeclType();
    //    var parameter = constructorDecl.GetParameter(i + baseParamCount);

    //    // 타입을 비교해서 같지 않다면 제외
    //    if (!BodyMisc.TypeEquals(parameter.Type, memberVarType)) return false;
    //}

    //return true;
}

void AddStruct_TrivialConstructorPhase(const shared_ptr<NStructDecl>& nStruct)
{
    assert(nStruct->oBaseTypes);
    auto& baseStruct = nStruct->oBaseTypes->baseStruct;
    shared_ptr<NStructConstructorDecl> baseCtor;

    if (baseStruct)
    {
        baseCtor = baseStruct->GetOpenTrivialConstructor();

        // 베이스가 있는데 Trivial이 없으면 안만든다
        if (!baseCtor) return;
    }
    
    // 같은 파라미터가 있으면 못 만든다
    bool bExistCtorConflictTrivial = false;    
    for(auto& ctor : nStruct->GetOpenConstructors())
        if (IsMatchStructTrivialConstructorParameters(nStruct, *ctor,))
            return;
    
    size_t memberVarCount = nStruct->GetMemberVarCount();
    size_t totalParamCount = baseCtor
        ? baseCtor->GetParamCount() + memberVarCount
        : memberVarCount;

    vector<RFuncParameter> parameters;

    //// to prevent conflict between parameter names, using special name $'base'_<name>_index
    //// class A { A(int x) {} }
    //// class B : A { B(int $base_x0, int x) : base($base_x0) { } }
    //// class C : B { C(int $base_x0, int $base_x1, int x) : base($base_x0, $base_x1) { } }
    if (baseCtor)
    {
        size_t baseParamCount = baseCtor->GetParamCount();
        parameters.reserve(baseParamCount + memberVarCount);
        for (size_t i = 0; i < baseParamCount; i++)
        {
            auto& baseParam = baseCtor->GetOpenFuncParam(i);
            auto paramName = MakeBaseConstructorParamName(i, baseParam.name);

            // 이름 보정, base로 가는 파라미터들은 다 이름이 ConstructorParam이다.
            // constructor에 out은 지원하지 않는다
            parameters.emplace_back(/*bOut*/ false, baseParam.type, std::move(paramName));
        }

        for (auto& memberVar : nStruct->GetOpenMemberVars())
            parameters.emplace_back(/*bOut*/ false, memberVar->type, memberVar->name);
    }
    else
    {
        parameters.reserve(memberVarCount);
        for (auto& memberVar : nStruct->GetOpenMemberVars())
            parameters.emplace_back(/*bOut*/ false, memberVar->type, memberVar->name);
    }

    auto nCtor = MakePtr<NStructConstructorDecl>(nStruct, RAccessor::Public, /*bTrivial*/ true);
    nCtor->InitFuncParameters(parameters);

    //// trivial constructor를 만듭니다
    //return new StructConstructorDeclSymbol(declSymbol, Accessor.Public, builder.MoveToImmutable(), bTrivial: true, bLastParamVariadic : false);

    //if (!HasStructConstructorHasSameParamWithTrivial(rStruct))
    //{
    //    AddTrivialStructConstructor(rStruct);
    //}

    //var baseTrivialConstructor = uniqueBaseStruct ? .GetTrivialConstructor();

    //// baseStruct가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
    //// baseStruct가 있고, TrivialConstructor가 있는 경우 => 진행
    //// baseStruct가 없는 경우 => 없이 만들고 진행 
    //if (baseTrivialConstructor != null || uniqueBaseStruct == null)
    //{
    //    // 같은 인자의 생성자가 없으면 Trivial을 만든다
    //    if (GetStructConstructorHasSameParamWithTrivial(baseTrivialConstructor, structDeclSymbol) == null)
    //    {
    //        var trivialConstructor = MakeStructTrivialConstructorDecl(structDeclSymbol, baseTrivialConstructor);
    //        structDeclSymbol.AddConstructor(trivialConstructor);
    //    }
    //}
}

} // namespace

shared_ptr<NStructDecl> InnerMakeStruct(shared_ptr<SStructDecl>&& sStruct, const std::shared_ptr<NTypeDeclOuter>& nOuter, RAccessor accessor, SkeletonPhaseContext& context)
{
    auto typeParams = MakeTypeParams(sStruct->typeParams);
    auto nStruct = MakePtr<NStructDecl>(nOuter, accessor, RName_Normal(sStruct->name), std::move(typeParams));

    for (auto& memberDecl : sStruct->memberDecls)
    {
        StructMemberDeclVisitor visitor(nStruct, memberDecl, context);
        memberDecl->Accept(visitor);
    }

    context.AddMemberDeclPhaseTask([nStruct, sStruct](MemberDeclPhaseContext& context) {
        AddStruct_MemberDeclPhase(nStruct, sStruct, context);
    });

    return nStruct;
}

} // namespace Citron::SyntaxIR0Translator
