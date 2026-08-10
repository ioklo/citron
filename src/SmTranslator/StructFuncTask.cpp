#include "StructFuncTask.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "BuildNonTypeSymbolContext.h"
#include "TranslateBodyContext.h"
#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "SmDeclContext_Decl.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"

using namespace std;

namespace Citron {

void StructFuncTask::Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructFuncTask> task{new StructFuncTask(move(structDeclContext), rStructDecl, syntax, std::move(rFactory))};
    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> StructFuncTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructFunc->accessModifier);

    rStructFunc = rFactory->MakeDecl<RStructFuncDecl>(
        rStruct, accessor, RName::Normal(sStructFunc->name), sStructFunc->bSequence);

    // typeParam을 만들땐 rStructFunc가 필요하다 (rStructFunc는 tree에 매달려있지 않은 상태라도 상관없다)
    auto typeParams = MakeTypeParams(rStruct->GetAllTypeParamCount(), rStructFunc, sStructFunc->typeParams, rFactory);

    // 만들어진 typeParam도 검색대상이다
    SmTypeResolveScope_DeclHeader scope{structDeclContext.get(), typeParams};
    auto e_funcRet = context.MakeFuncReturn(sStructFunc->funcRet, rStructFunc, typeParams, scope);
    RETURN_ON_ERROR(e_funcRet);

    auto e_parameters = context.MakeFuncParameters(sStructFunc->parameters, scope);
    RETURN_ON_ERROR_REFDECL(e_parameters, [parameters, bLastParamVariadic]);
    
    RThisKind thisKind = [this]() -> RThisKind {
        if (sStructFunc->bStatic) return RThisKind_Static{};
        auto* structType = rFactory->MakeStructType(RAppliedDecl<RStructDecl>{rStruct, rStruct->MakeOpenTypeArgs(*rFactory)});
        return RThisKind_Ref{structType};
    }();

    // init
    rStructFunc->Init(RDeclKey::Func(RName::Normal(sStructFunc->name), parameters), move(thisKind), move(*e_funcRet), move(typeParams), move(parameters), bLastParamVariadic);

    // AddFunc할땐 declKey가 필요하다. declKey는 FuncParamter가 필요하다
    rStruct->AddFunc(rStructFunc);
    return {};
}

expected<MFuncBody, DiagPtr> StructFuncTask::TranslateBody(TranslateBodyContext& context)
{
    SmDeclContextPtr structFuncDeclContext = MakePtr<SmDeclContext_Decl<RStructFuncDecl>>(structDeclContext, rStructFunc, rStructFunc->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(structFuncDeclContext), rStructFunc, sStructFunc->bSequence, sStructFunc->body);
}


}