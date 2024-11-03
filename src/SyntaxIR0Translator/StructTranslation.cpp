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

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

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

void AddStructConstructor_MemberDeclPhase(shared_ptr<NStructConstructorDecl> nConstructor, shared_ptr<SStructConstructorDecl> sConstructor, MemberDeclPhaseContext& context);
void AddStructConstructor_BodyPhase(shared_ptr<NStructConstructorDecl> nConstructor, shared_ptr<SStructConstructorDecl> sConstructor, BodyPhaseContext& context);

void AddStructConstructor(const shared_ptr<NStructDecl>& rStruct, shared_ptr<SStructConstructorDecl> sConstructor, SkeletonPhaseContext& context)
{
    auto accessModifier = MakeStructMemberAccessor(sConstructor->accessModifier);

    // TODO: 타이프 쳐서 만들어진 constructor는 'trivial' 표시를 하기 전까지는 trivial로 인식하지 않는다. 지금은 false로 표기
    // 그리고 컴파일러가 trivial 조건을 체크해서 에러를 낼 수도 있다 (하위 타입의 trivial constructor가 이 constructor를 참조하지 않는다)
    auto nConstructor = MakePtr<NStructConstructorDecl>(rStruct, accessModifier, false);
    context.AddMemberDeclPhaseTask([nConstructor, sConstructor = move(sConstructor)](MemberDeclPhaseContext& context) {
        AddStructConstructor_MemberDeclPhase(move(nConstructor), move(sConstructor), context);
    });

    rStruct->AddConstructor(move(nConstructor));
}

void AddStructConstructor_MemberDeclPhase(shared_ptr<NStructConstructorDecl> nConstructor, shared_ptr<SStructConstructorDecl> sConstructor, MemberDeclPhaseContext& context)
{
    // Constructor는 Type Parameter가 없으므로 파라미터를 만들 때, 상위(struct) declSymbol을 넘긴다
    auto [parameters, bLastParamVariadic] = context.MakeParameters(nConstructor, sConstructor->parameters);

    nConstructor->InitFuncParameters(move(parameters), bLastParamVariadic);

    context.AddBodyPhaseTask([rConstructor = move(nConstructor), sConstructor = move(sConstructor)](BodyPhaseContext& context) {
        AddStructConstructor_BodyPhase(move(rConstructor), move(sConstructor), context);
    });
}

void AddStructConstructor_BodyPhase(shared_ptr<NStructConstructorDecl> nConstructor, shared_ptr<SStructConstructorDecl> sConstructor, BodyPhaseContext& context)
{
    static_assert(false);

    //ScopeContext scopeContext;

    //auto oRBody = TranslateBody(sConstructor->body, scopeContext);

    //// scopeContext
    //if (!scopeContext.IsFailed())
    //{
    //    context.MarkFailed();
    //    return;
    //}

    //assert(oRBody);

    //rConstructor->InitBody(move(*oRBody));
}

#pragma endregion Constructor

#pragma region MemberFunc

void AddStructMemberFunc_MemberDeclPhase(shared_ptr<NStructMemberFuncDecl> nMemberFunc, shared_ptr<SStructMemberFuncDecl> sMemberFunc, MemberDeclPhaseContext& context);
void AddStructMemberFunc_BodyPhase(shared_ptr<NStructMemberFuncDecl> nMemberFunc, shared_ptr<SStructMemberFuncDecl> sMemberFunc, BodyPhaseContext& context);

void AddStructMemberFunc(const shared_ptr<NStructDecl>& rStruct, shared_ptr<SStructMemberFuncDecl> sMemberFunc, SkeletonPhaseContext& context)
{
    auto accessor = MakeStructMemberAccessor(sMemberFunc->accessModifier);
    auto typeParams = MakeTypeParams(sMemberFunc->typeParams);

    // TODO: bSequence
    auto nMemberFunc = MakePtr<NStructMemberFuncDecl>(rStruct, accessor, sMemberFunc->name, move(typeParams), sMemberFunc->bStatic);

    context.AddMemberDeclPhaseTask([nMemberFunc, sMemberFunc](MemberDeclPhaseContext& context) {
        AddStructMemberFunc_MemberDeclPhase(move(nMemberFunc), move(sMemberFunc), context);
    });

    rStruct->AddMemberFunc(move(nMemberFunc));
}

void AddStructMemberFunc_MemberDeclPhase(shared_ptr<NStructMemberFuncDecl> nMemberFunc, shared_ptr<SStructMemberFuncDecl> sMemberFunc, MemberDeclPhaseContext& context)
{
    auto rRetType = context.MakeType(sMemberFunc->retType, nMemberFunc);
    auto [rParameters, bLastParamVariadic] = context.MakeParameters(nMemberFunc, sMemberFunc->parameters);

    nMemberFunc->InitFuncReturnAndParams(move(rRetType), move(rParameters), bLastParamVariadic);

    context.AddBodyPhaseTask([nMemberFunc, sMemberFunc](BodyPhaseContext& context) {
        AddStructMemberFunc_BodyPhase(move(nMemberFunc), move(sMemberFunc), context);
    });
}

void AddStructMemberFunc_BodyPhase(shared_ptr<NStructMemberFuncDecl> nMemberFunc, shared_ptr<SStructMemberFuncDecl> sMemberFunc, BodyPhaseContext& context)
{
    static_assert(false);

    //ScopeContext scopeContext;

    //auto oRBody = TranslateBody(sMemberFunc->body, scopeContext);

    //// scopeContext
    //if (!scopeContext.IsFailed())
    //{
    //    context.MarkFailed();
    //    return;
    //}

    //assert(oRBody);
    //
    //rMemberFunc->InitBody(move(*oRBody));
}

#pragma endregion MemberFunc

#pragma region MemberVar

void AddStructMemberVar_MemberDeclPhase(vector<shared_ptr<NStructMemberVarDecl>>& rMemberVars, NDeclPtr nDecl, STypeExpPtr sTypeExp, MemberDeclPhaseContext& context);

void AddStructMemberVar(const shared_ptr<NStructDecl>& rStruct, SStructMemberVarDecl& sMemberVar, SkeletonPhaseContext& context)
{
    auto accessor = MakeStructMemberAccessor(sMemberVar.accessModifier);

    // TODO: bStatic 지원
    vector<shared_ptr<NStructMemberVarDecl>> rMemberVars;
    rMemberVars.reserve(sMemberVar.varNames.size());

    for (auto& varName : sMemberVar.varNames)
    {
        auto nMemberVar = MakePtr<NStructMemberVarDecl>(rStruct, accessor, false, varName);
        rMemberVars.push_back(nMemberVar); // for lazy-init
        rStruct->AddMemberVar(move(nMemberVar));
    }

    context.AddMemberDeclPhaseTask([rMemberVars = move(rMemberVars), varType = sMemberVar.varType, rStruct](MemberDeclPhaseContext& context) mutable {
        AddStructMemberVar_MemberDeclPhase(rMemberVars, rStruct, varType, context);
    });
}

void AddStructMemberVar_MemberDeclPhase(vector<shared_ptr<NStructMemberVarDecl>>& rMemberVars, NDeclPtr rDecl, STypeExpPtr sTypeExp, MemberDeclPhaseContext& context)
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
    void Visit(SClassDecl& decl) override { static_assert(false); }

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
        static_assert(false);
    }

    void Visit(SStructMemberVarDecl& decl) override
    {
        AddStructMemberVar(nStructDecl, decl, context);
    }
};

} // namespace

void AddStruct_MemberDeclPhase(shared_ptr<NStructDecl> nStruct, shared_ptr<SStructDecl> sStruct, MemberDeclPhaseContext& context);
void AddStruct_TrivialConstructorPhase(shared_ptr<NStructDecl> nStruct);

shared_ptr<NStructDecl> InnerMakeStruct(shared_ptr<SStructDecl> sStruct, NTypeDeclOuterWPtr nOuter, RAccessor accessor, SkeletonPhaseContext& context)
{
    auto typeParams = MakeTypeParams(sStruct->typeParams);
    auto nStruct = MakePtr<NStructDecl>(nOuter, accessor, RName_Normal(sStruct->name), typeParams);

    for (auto& memberDecl : sStruct->memberDecls)
    {
        StructMemberDeclVisitor visitor(nStruct, memberDecl, context);
        memberDecl->Accept(visitor);
    }

    context.AddMemberDeclPhaseTask([nStruct, sStruct](MemberDeclPhaseContext& context) {
        AddStruct_MemberDeclPhase(std::move(nStruct), std::move(sStruct), context);
    });

    return nStruct;
}

// RStructDecl or MStructDecl
// std::variant<std::monostate, std::shared_ptr<RStructDecl>, std::shared_ptr<MStructDecl>> 
// RStructDecl, MStructDecl
// RStructDecl = RInternalStructDecl, RExternalStructDecl

// MStructDecl이 있고, RStructDecl은 Body가 있는 버전과 없는 버전으로 구성될 수 있다

// 필요한 것만 IRStructDecl
// RStructDecl, RExStructDecl



void AddStruct_MemberDeclPhase(shared_ptr<NStructDecl> nStruct, shared_ptr<SStructDecl> sStruct, MemberDeclPhaseContext& context)
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
    context.AddTrivialConstructorPhaseTask([nStruct = std::move(nStruct)]() {
        AddStruct_TrivialConstructorPhase(std::move(nStruct));
    });
}

static bool IsMatchStructTrivialConstructorParameters(NStructConstructorDecl& rConstructor, vector<RFuncParameter>& baseConstructorParameters)
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

void AddStruct_TrivialConstructorPhase(shared_ptr<NStructDecl> nStruct)
{
    static_assert(false);
    // RStructDecl

    // 생성자 중, 파라미터가 같은 것이 있는지 확인
    //for(auto& constructor : rStruct->GetConstructors())
    //{
    //    IsMatchStructTrivialConstructorParameters(rStruct, *constructor);
    //}
    //
    //int constructorCount = rStruct->GetConstructorCount();
    //for (int i = 0; i < constructorCount; i++)
    //{
    //    var constructor = decl.GetConstructor(i);

    //    if (IsMatchStructTrivialConstructorParameters(baseConstructor, decl, constructor))
    //        return constructor;
    //}

    //return null;

    //int memberVarCount = rStruct->GetMemberVarCount();


    //int totalParamCount = (baseConstructor ? .GetParameterCount() ? ? 0) + memberVarCount;
    //var builder = ImmutableArray.CreateBuilder<FuncParameter>(totalParamCount);

    //// to prevent conflict between parameter names, using special name $'base'_<name>_index
    //// class A { A(int x) {} }
    //// class B : A { B(int $base_x0, int x) : base($base_x0) { } }
    //// class C : B { C(int $base_x0, int $base_x1, int x) : base($base_x0, $base_x1) { } }
    //if (baseConstructor != null)
    //{
    //    for (int i = 0; i < baseConstructor.GetParameterCount(); i++)
    //    {
    //        var baseParam = baseConstructor.GetParameter(i);
    //        var paramName = BuilderMisc.MakeBaseConstructorParamName(i, baseParam.Name);

    //        // 이름 보정, base로 가는 파라미터들은 다 이름이 ConstructorParam이다.
    //        // constructor에 out은 지원하지 않는다
    //        var newBaseParam = new FuncParameter(bOut: false, baseParam.Type, paramName);
    //        builder.Add(newBaseParam);
    //    }
    //}

    //for (int i = 0; i < memberVarCount; i++)
    //{
    //    var memberVar = declSymbol.GetMemberVar(i);
    //    var type = memberVar.GetDeclType();
    //    var name = memberVar.GetName();

    //    var param = new FuncParameter(bOut: false, type, name);
    //    builder.Add(param);
    //}

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

} // namespace Citron::SyntaxIR0Translator
