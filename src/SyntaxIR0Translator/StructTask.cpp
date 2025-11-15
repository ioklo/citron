#include "StructTask.h"

#include "Infra/Exceptions.h"

#include "Syntax/Syntax.h"
#include "NSymbol/NStructDecl.h"

#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "ResolveTypeHierarchyContext.h"
#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {
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
} // namespace 

StructTask::StructTask(NStructDecl* nStructDecl, SStructDecl* syntax, AccessorContext accessorContext)
    : nStructDecl{nStructDecl}, syntax{syntax}, accessorContext {accessorContext}
{
}

void StructTask::Register(NStructDecl* nStructDecl, SStructDecl* syntax, AccessorContext accessorContext, PhaseManager& phaseManager)
{
    shared_ptr<StructTask> task{new StructTask(nStructDecl, syntax, accessorContext)};
    phaseManager.AddResolveTypeHierarchyTask(task);
    phaseManager.AddSynthesizeImplicitSymbolTask(task);
}

void StructTask::ResolveTypeHierarchy(ResolveTypeHierarchyContext& context)
{
    // 유일한 베이스 타입은 struct인데, 외부에서 선언된 struct일수도 있고, 조합 타입일 수도 있다 (사실 조합타입이 될 가능성은 거의 없어보인다)
    RType_Struct* rBaseStruct = nullptr; // nullable

    // 나머지는 interface들이다
    vector<RType_Interface*> rInterfaces;

    for (auto* sType : syntax->baseTypes)
    {
        auto rType = context.MakeType(sType, nStructDecl);
        auto rTypeKind = rType->GetCustomTypeKind();

        if (rTypeKind == RCustomTypeKind::Struct)
        {
            // 두개 이상의 struct를 상속받으려고 했다면, 에러 처리
            if (rBaseStruct != nullptr)
                throw NotImplementedException{};

            rBaseStruct = dynamic_cast<RType_Struct*>(rType);
            assert(rBaseStruct); // CustomTypeKind가 Struct이면서 RType_Struct를 따르지 않는것이 뭐가 있을까
        }
        else if (rTypeKind == RCustomTypeKind::Interface)
        {
            auto* rInterface = dynamic_cast<RType_Interface*>(rType);
            if (!rInterface)
            {
                throw NotImplementedException{};
            }

            // func<>, 등도 interface type인데, 어떻게 할지
            rInterfaces.push_back(rInterface);
        }
        else
        {
            // 다른 타입은 struct의 basetype자리에 올 수 없습니다 에러 출력
            throw NotImplementedException{};
        }
    }

    nStructDecl->InitBaseTypes(rBaseStruct, move(rInterfaces));
}

// base의 TrivialCtor가 다 만들어 졌을 때, 수행하는 작업
void StructTask::SynthesizeImplicitSymbol(SynthesizeImplicitSymbolContext& context)
{
    auto* rBaseStruct = nStructDecl->GetUnboundBaseStruct();
    RStructCtorDecl* rBaseTrivialCtor = nullptr;

    if (rBaseStruct)
    {
        rBaseTrivialCtor = rBaseStruct->GetUnboundTrivialCtor();

        // 베이스가 있는데 Trivial이 없으면 안만든다
        if (!rBaseTrivialCtor) return;
    }

    // 같은 파라미터가 있으면 안 만든다
    if (HasConflictTrivialCtor(nStructDecl, rBaseTrivialCtor))
        return;

    size_t varCount = nStructDecl->GetVarCount();
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

        for (auto* var : nStructDecl->EnumerateUnboundVars())
            parameters.emplace_back(/*bOut*/ false, var->GetUnboundDeclType(), RName_Normal{var->name});
    }
    else
    {
        parameters.reserve(varCount);
        for (auto* var : nStructDecl->EnumerateUnboundVars())
            parameters.emplace_back(/*bOut*/ false, var->GetUnboundDeclType(), RName_Normal{var->name});
    }

    throw NotImplementedException{};

    // auto* nCtor = context.MakeNDecl<NStructCtorDecl>(nStructDecl, RAccessor::Public, /*bTrivial*/ true);
    //
    // nCtor->InitFuncParameters(parameters, /*bLastParameterVariadic*/ false);
    // nCtor->InitBodyWillBeGenerated();
    //
    // nStructDecl->AddCtor(nCtor);
}


} // Citron::SyntaxIR0Translator
