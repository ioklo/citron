#include "StructTranslation.h"

#include <cassert>

#include "Infra/Unreachable.h"
#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "IR0/RTypes.h"
#include "IR0/NStructDecl.h"
#include "IR0/NStructCtorDecl.h"
#include "IR0/NEnumDecl.h"

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
void AddStructCtor_MemberDeclPhase(NStructCtorDecl* nCtor, SStructCtorDecl* sCtor, MemberDeclPhaseContext& context);
void AddStructCtor_BodyPhase(NStructCtorDecl* nCtor, SStructCtorDecl* sCtor, BodyPhaseContext& context);
void AddStructFunc_MemberDeclPhase(NStructFuncDecl* nFunc, SStructFuncDecl* sFunc, MemberDeclPhaseContext& context);
void AddStructFunc_BodyPhase(NStructFuncDecl* nFunc, SStructFuncDecl* sFunc, BodyPhaseContext& context);
void AddStructVar_MemberDeclPhase(vector<NStructVarDecl*>&& nVars, NDecl* nDecl, STypeExp* sTypeExp, MemberDeclPhaseContext& context);
void AddStruct_TrivialCtorPhase(NStructDecl* nStruct);

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

#pragma region Ctor

void AddStructCtor(NStructDecl* nStruct, SStructCtorDecl* sCtor, SkeletonPhaseContext& context)
{
    auto accessModifier = MakeStructMemberAccessor(sCtor->accessModifier);

    // TODO: 타이프 쳐서 만들어진 ctor는 'trivial' 표시를 하기 전까지는 trivial로 인식하지 않는다. 지금은 false로 표기
    // 그리고 컴파일러가 trivial 조건을 체크해서 에러를 낼 수도 있다 (하위 타입의 trivial constructor가 이 constructor를 참조하지 않는다)
    auto* nCtor = context.MakeNDecl<NStructCtorDecl>(nStruct, accessModifier, false);
    context.AddMemberDeclPhaseTask([nCtor, sCtor](MemberDeclPhaseContext& context) {
        AddStructCtor_MemberDeclPhase(nCtor, sCtor, context);
    });

    nStruct->AddCtor(move(nCtor));
}

void AddStructCtor_MemberDeclPhase(NStructCtorDecl* nCtor, SStructCtorDecl* sCtor, MemberDeclPhaseContext& context)
{
    // ctor는 Type Parameter가 없으므로 파라미터를 만들 때, 상위(struct) declSymbol을 넘긴다
    auto [parameters, bLastParamVariadic] = context.MakeParameters(nCtor, sCtor->parameters);

    nCtor->InitFuncParameters(move(parameters), bLastParamVariadic);

    context.AddBodyPhaseTask([nCtor, sCtor](BodyPhaseContext& context) {
        AddStructCtor_BodyPhase(nCtor, sCtor, context);
    });
}

void AddStructCtor_BodyPhase(NStructCtorDecl* nCtor, SStructCtorDecl* sCtor, BodyPhaseContext& context)
{
    auto translationContext = context.MakeTranslationContext();

    auto eNStmts = TranslateSBodyToNStmts(sCtor->body, translationContext);

    if (!eNStmts)
    {
        context.MarkFailed();
        return;
    }

    nCtor->InitBody(move(*eNStmts));
}

#pragma endregion Ctor

#pragma region MemberFunc

void AddStructFunc(NStructDecl* nStruct, SStructFuncDecl* sMemberFunc, SkeletonPhaseContext& context)
{
    auto accessor = MakeStructMemberAccessor(sMemberFunc->accessModifier);
    auto typeParams = MakeTypeParams(sMemberFunc->typeParams);

    // TODO: bSequence
    auto* nMemberFunc = context.MakeNDecl<NStructFuncDecl>(nStruct, accessor, sMemberFunc->name, move(typeParams), sMemberFunc->bStatic);

    context.AddMemberDeclPhaseTask([nMemberFunc, sMemberFunc](MemberDeclPhaseContext& context) {
        AddStructFunc_MemberDeclPhase(nMemberFunc, sMemberFunc, context);
    });

    nStruct->AddFunc(nMemberFunc);
}

void AddStructFunc_MemberDeclPhase(NStructFuncDecl* nMemberFunc, SStructFuncDecl* sMemberFunc, MemberDeclPhaseContext& context)
{
    auto* rRetType = context.MakeType(sMemberFunc->retType, nMemberFunc);
    auto [rParameters, bLastParamVariadic] = context.MakeParameters(nMemberFunc, sMemberFunc->parameters);

    nMemberFunc->InitFuncReturnAndParams(rRetType, move(rParameters), bLastParamVariadic);

    context.AddBodyPhaseTask([nMemberFunc, sMemberFunc](BodyPhaseContext& context) {
        AddStructFunc_BodyPhase(nMemberFunc, sMemberFunc, context);
    });
}

void AddStructFunc_BodyPhase(NStructFuncDecl* nMemberFunc, SStructFuncDecl* sMemberFunc, BodyPhaseContext& context)
{
    auto translationContext = context.MakeTranslationContext();
    auto eNStmts = TranslateSBodyToNStmts(sMemberFunc->body, translationContext);

    if (!eNStmts)
    {
        context.MarkFailed();
        return;
    }

    nMemberFunc->InitBody(move(*eNStmts));
}

#pragma endregion StructFunc

#pragma region StructVar

void AddStructVar(NStructDecl* nStruct, SStructVarDecl* sStructVar, SkeletonPhaseContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructVar->accessModifier);

    // TODO: bStatic 지원
    vector<NStructVarDecl*> nStructVars;
    nStructVars.reserve(sStructVar->varNames.size());

    for (auto& varName : sStructVar->varNames)
    {
        auto* nStructVar = context.MakeNDecl<NStructVarDecl>(nStruct, accessor, false, varName);
        nStructVars.push_back(nStructVar); // for lazy-init
        nStruct->AddVar(nStructVar);
    }

    context.AddMemberDeclPhaseTask([nStructVars = move(nStructVars), varType = sStructVar->varType, nStruct](MemberDeclPhaseContext& context) mutable {
        AddStructVar_MemberDeclPhase(move(nStructVars), nStruct, varType, context);
    });
}

void AddStructVar_MemberDeclPhase(vector<NStructVarDecl*>&& nStructVars, NDecl* nDecl, STypeExp* sTypeExp, MemberDeclPhaseContext& context)
{
    auto declType = context.MakeType(sTypeExp, nDecl);
    for (auto* nStructVar : nStructVars)
        nStructVar->InitDeclType(declType);
}

#pragma endregion Var
 
class StructMemberDeclVisitor : public SStructMemberDeclVisitor
{
    NStructDecl* nStructDecl;
    SkeletonPhaseContext& context;

public:
    StructMemberDeclVisitor(NStructDecl* nStructDecl, SkeletonPhaseContext& context)
        : nStructDecl(nStructDecl), context(context)
    { }

    // Inherited via SStructMemberDeclVisitor
    void Visit(SClassDecl* decl) override 
    { 
        // ClassTranslation을 만들어야 한다
        throw NotImplementedException();
        /*auto sSharedClassDecl = dynamic_pointer_cast<SClassDecl>(sSharedMemberDecl);
        auto nNestedClassDecl= MakeClass(nStructDecl, sSharedClassDecl, MakeStructMemberAccessor, context);
        nStructDecl->AddType(move(nNestedClassDecl));*/
    }

    void Visit(SStructDecl* decl) override
    {
        auto* nNestedStructDecl = MakeStruct(nStructDecl, decl, MakeStructMemberAccessor, context);
        nStructDecl->AddType(nNestedStructDecl);
    }

    void Visit(SEnumDecl* decl) override
    {
        auto* nEnum = MakeEnum(nStructDecl, decl, MakeStructMemberAccessor, context);
        nStructDecl->AddType(move(nEnum));
    }

    void Visit(SStructCtorDecl* decl) override
    {
        AddStructCtor(nStructDecl, decl, context);
    }

    void Visit(SStructFuncDecl* decl) override
    {
        AddStructFunc(nStructDecl, decl, context);
    }

    void Visit(SStructVarDecl* decl) override
    {
        AddStructVar(nStructDecl, decl, context);
    }
};

void AddStruct_MemberDeclPhase(NStructDecl* nStruct, SStructDecl* sStruct, MemberDeclPhaseContext& context)
{
    // 유일한 베이스 타입은 struct인데, 외부에서 선언된 struct일수도 있고, 조합 타입일 수도 있다 (사실 조합타입이 될 가능성은 거의 없어보인다)
    RType_Struct* rBaseStruct = nullptr; // nullable

    // 나머지는 interface들이다
    vector<RType_Interface*> rInterfaces;

    for (auto* sType : sStruct->baseTypes)
    {
        auto rType = context.MakeType(sType, nStruct);
        auto rTypeKind = rType->GetCustomTypeKind();

        if (rTypeKind == RCustomTypeKind::Struct)
        {
            // 두개 이상의 struct를 상속받으려고 했다면, 에러 처리
            if (rBaseStruct != nullptr)
                throw NotImplementedException();

            rBaseStruct = dynamic_cast<RType_Struct*>(rType);
            assert(rBaseStruct); // CustomTypeKind가 Struct이면서 RType_Struct를 따르지 않는것이 뭐가 있을까
        }
        else if (rTypeKind == RCustomTypeKind::Interface)
        {
            auto rInterface = dynamic_cast<RType_Interface*>(rType);
            if (!rInterface)
            {
                throw NotImplementedException();
            }

            // func<>, 등도 interface type인데, 어떻게 할지
            rInterfaces.push_back(move(rInterface));
        }
        else
        {
            // 다른 타입은 struct의 basetype자리에 올 수 없습니다 에러 출력
            throw NotImplementedException();
        }
    }

    nStruct->InitBaseTypes(rBaseStruct, move(rInterfaces));

    // base의 TrivialCtor가 다 만들어 졌을 때, 수행하는 작업
    context.AddTrivialCtorPhaseTask([nStruct]() {
        AddStruct_TrivialCtorPhase(nStruct);
    });
}

// nStruct의 constructor중에 trivial constructor랑 모양이 같은 것이 있다면 만들지 않는다 (모양이 같은 함수가 trivial인지는 체크하지 않는다)
bool HasConflictTrivialCtor(NStructDecl* nStruct, RStructCtorDecl* rBaseTrivialCtor)
{
    size_t baseParamCount = rBaseTrivialCtor ? rBaseTrivialCtor->GetParamCount() : 0;
    size_t varCount = nStruct->GetVarCount();

    for (auto* nCtor : nStruct->EnumerateUnboundCtors())
    {
        size_t paramCount = nCtor->GetParamCount();
        if (varCount != paramCount) continue;

        //// constructorDecl의 앞부분이 baseConstructor와 일치하는지를 봐야 한다
        bool bMatch = true;
        for (size_t i = 0; i < baseParamCount; i++)
        {
            auto& baseParameter = rBaseTrivialCtor->GetUnboundFuncParam(i);
            auto& parameter = nCtor->GetUnboundFuncParam(i);

            bMatch &= (baseParameter.type == parameter.type);
        }

        if (!bMatch) continue;

        // baseParam을 제외한 뒷부분이 varType과 맞는지 봐야 한다
        for (size_t i = 0; i < paramCount; i++)
        {
            auto* structVar = nStruct->GetUnboundVar(i); // varCount == paramCount체크를 위에서 했다
            auto& ctorParam = nCtor->GetUnboundFuncParam(i + baseParamCount);

            bMatch &= (structVar->GetUnboundDeclType() == ctorParam.type);
        }

        if (bMatch) return true;
    }

    return false;
}

void AddStruct_TrivialCtorPhase(NStructDecl* nStruct)
{   
    auto rBaseStruct = nStruct->GetUnboundBaseStruct();
    RStructCtorDecl* rBaseTrivialCtor;

    if (rBaseStruct)
    {
        rBaseTrivialCtor = rBaseStruct->GetUnboundTrivialCtor();

        // 베이스가 있는데 Trivial이 없으면 안만든다
        if (!rBaseTrivialCtor) return;
    }
    
    // 같은 파라미터가 있으면 안 만든다
    if (HasConflictTrivialCtor(nStruct, rBaseTrivialCtor))
        return;
    
    size_t varCount = nStruct->GetVarCount();
    size_t totalParamCount = rBaseTrivialCtor
        ? rBaseTrivialCtor->GetParamCount() + varCount
        : varCount;

    vector<RFuncParameter> parameters;

    //// to prevent conflict between ctorParam names, using special name $'base'_<name>_index
    //// class A { A(int x) {} }
    //// class B : A { B(int $base_x0, int x) : base($base_x0) { } }
    //// class C : B { C(int $base_x0, int $base_x1, int x) : base($base_x0, $base_x1) { } }
    if (rBaseTrivialCtor)
    {
        size_t baseParamCount = rBaseTrivialCtor->GetParamCount();
        parameters.reserve(baseParamCount + varCount);
        for (size_t i = 0; i < baseParamCount; i++)
        {
            auto& baseParam = rBaseTrivialCtor->GetUnboundFuncParam(i);
            auto paramName = MakeBaseCtorParamName(i, baseParam.name);

            // 이름 보정, base로 가는 파라미터들은 다 이름이 CtorParam이다.
            // ctor에 out은 지원하지 않는다
            parameters.emplace_back(/*bOut*/ false, baseParam.type, move(paramName));
        }

        for (auto* var : nStruct->EnumerateUnboundVars())
            parameters.emplace_back(/*bOut*/ false, var->GetUnboundDeclType(), RName_Normal{var->name});
    }
    else
    {
        parameters.reserve(varCount);
        for (auto* var : nStruct->EnumerateUnboundVars())
            parameters.emplace_back(/*bOut*/ false, var->GetUnboundDeclType(), RName_Normal{var->name});
    }

    throw NotImplementedException{};

    // auto* nCtor = context.MakeNDecl<NStructCtorDecl>(nStruct, RAccessor::Public, /*bTrivial*/ true);
    //
    // nCtor->InitFuncParameters(parameters, /*bLastParameterVariadic*/ false);
    // nCtor->InitBodyWillBeGenerated();
    //
    // nStruct->AddCtor(nCtor);
}

} // namespace

NStructDecl* InnerMakeStruct(SStructDecl* sStruct, NTypeDeclOuter* nOuter, RAccessor accessor, SkeletonPhaseContext& context)
{
    auto typeParams = MakeTypeParams(sStruct->typeParams);
    auto* nStruct = context.MakeNDecl<NStructDecl>(nOuter, accessor, RName_Normal(sStruct->name), move(typeParams));

    for (auto* memberDecl : sStruct->memberDecls)
    {
        StructMemberDeclVisitor visitor{nStruct, context};
        memberDecl->Accept(visitor);
    }

    context.AddMemberDeclPhaseTask([nStruct, sStruct](MemberDeclPhaseContext& context) {
        AddStruct_MemberDeclPhase(nStruct, sStruct, context);
    });

    return nStruct;
}

} // namespace Citron::SyntaxIR0Translator
