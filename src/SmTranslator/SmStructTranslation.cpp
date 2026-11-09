#include "SmStructTranslation.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RImplTraitDeclOuter.h"

#include "CommonTranslation.h"
#include "SmPhaseManager.h"
#include "BuildTypeHierarchyContext.h"
#include "BuildImplicitSymbolContext.h"
#include "Misc.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "SmDeclContext_Decl.h"
#include "SmStructFuncTranslation.h"
#include "SmStructVarTranslation.h"
#include "SmStructCtorTranslation.h"
#include "SmStructDtorTranslation.h"
#include "SmImplTraitTranslation.h"
#include "SmTraitTranslation.h"
#include "SmEnumTranslation.h"

using namespace std;

namespace Citron {

namespace {

class SmStructElemTranslator
{
    SmDeclContextPtr structDeclContext; // rStruct의 declContext
    RStructDecl* rStruct;
    RFactoryPtr rFactory;
    SmPhaseManager& phaseManager;

public:
    SmStructElemTranslator(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
        : structDeclContext{*structDeclContext}, rStruct{rStruct}, rFactory{*rFactory}, phaseManager{phaseManager}
    {
    }

    void operator()(auto* decl) { Visit(decl); }

    void Visit(SClassDecl* decl);
    void Visit(SStructDecl* decl);
    void Visit(SEnumDecl* decl);
    void Visit(STraitDecl* decl);
    void Visit(SImplTraitDecl* decl);
    void Visit(STypeAliasDecl* decl);
    void Visit(SStructFuncDecl* decl);
    void Visit(SStructCtorDecl* decl);
    void Visit(SStructDtorDecl* decl);
    void Visit(SStructVarDecl* decl);
};

class SmStructTask
    : public IBuildTypeHierarchyTask
    , public IBuildImplicitSymbolTask
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStructDecl;
    SStructDecl* syntax;

public:
    SmStructTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructDecl* syntax);
    std::expected<void, DiagPtr> BuildTypeHierarchy(BuildTypeHierarchyContext& context) override;
    void BuildImplicitSymbol(BuildImplicitSymbolContext& context) override;


private:
    void SynthesizeMemberwiseCtor(BuildImplicitSymbolContext& context);
};

} // namespace
} // namespace Citron

namespace Citron {

// prepare task
void SmTranslateStruct(InRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, SStructDecl* syntax, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    RName name{RName::Normal(syntax->name)};
    auto* rStructDecl = (*rFactory)->MakeDecl<RStructDecl>(RDeclKey::Normal(name), outer, move(name), *rFactory);

    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rStructDecl, syntax->typeParams, *rFactory);
    rStructDecl->InitTypeParams(move(typeParams));
    outer.AddType(rStructDecl);

    // TODO: RDecl::MakeOpenTypeArgs는 전체적으로 typeArgs를 다시만드는데, 이 rDecl만큼만 만들게 할 수 있을 것이다
    auto* openTypeArgs = rStructDecl->MakeOpenTypeArgs(**rFactory);
    SmDeclContextPtr structDeclContext = MakePtr<SmDeclContext_Decl<RStructDecl>>(*outerDeclContext, rStructDecl, openTypeArgs);

    shared_ptr<SmStructTask> task{new SmStructTask(move(structDeclContext), rStructDecl, syntax)};
    phaseManager.AddBuildTypeHierarchyTask(task);
    phaseManager.AddBuildImplicitSymbolTask(task);

    // child     
    for (auto& memberDecl : syntax->memberDecls)
    {
        visit(SmStructElemTranslator{structDeclContext, rStructDecl, *rFactory, phaseManager}, memberDecl);
    }
}

namespace {

void SmStructElemTranslator::Visit(SClassDecl* decl)
{
    throw NotImplementedException{};
}

void SmStructElemTranslator::Visit(SStructDecl* decl)
{
    auto accessor = MakeStructMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Struct outer{rStruct, accessor};
    SmTranslateStruct(structDeclContext, outer, decl, rFactory, phaseManager);
}

void SmStructElemTranslator::Visit(SEnumDecl* decl)
{
    auto accessor = MakeStructMemberAccessor(decl->accessModifier);
    RTypeDeclOuter_Struct outer{rStruct, accessor};
    SmTranslateEnum(structDeclContext, outer, decl, rFactory, phaseManager);
}

void SmStructElemTranslator::Visit(STraitDecl* decl)
{
    RTypeDeclOuter_Struct outer{rStruct, MakeStructMemberAccessor(decl->accessModifier)};
    SmTranslateTrait(structDeclContext, outer, decl, rFactory, phaseManager);
}

void SmStructElemTranslator::Visit(SImplTraitDecl* decl)
{
    SmTranslateImplTrait(decl, structDeclContext, RImplTraitDeclOuter{rStruct}, rFactory, phaseManager);
}

void SmStructElemTranslator::Visit(STypeAliasDecl* decl)
{
    // TODO: [79] type alias 구현
    throw NotImplementedException{};
}

void SmStructElemTranslator::Visit(SStructFuncDecl* decl)
{
    SmTranslateStructFunc(structDeclContext, rStruct, decl, rFactory, phaseManager);
}

void SmStructElemTranslator::Visit(SStructCtorDecl* decl)
{
    SmTranslateStructCtor(structDeclContext, rStruct, decl, rFactory, phaseManager);
}

void SmStructElemTranslator::Visit(SStructDtorDecl* decl)
{
    SmTranslateStructDtor(structDeclContext, rStruct, decl, rFactory, phaseManager);
}

void SmStructElemTranslator::Visit(SStructVarDecl* decl)
{
    SmTranslateStructVar(structDeclContext, rStruct, decl, rFactory, phaseManager);
}

SmStructTask::SmStructTask(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructDecl* syntax)
    : structDeclContext{*structDeclContext}, rStructDecl{rStructDecl}, syntax{syntax}
{
}

expected<void, DiagPtr> SmStructTask::BuildTypeHierarchy(BuildTypeHierarchyContext& context)
{
    // NOTICE: struct, class의 base부분을 볼때는 base 전용 type lookup을 해야한다 
    // (type parameter는 검색이 되지만, {멤버 타입/base타입의 멤버타입}은 검색이 안되게) => SmTypeResolveScope_DeclHeader를 만들었다
    SmTypeResolveScope_DeclHeader scope{structDeclContext.get(), rStructDecl->GetTypeParams()};

    vector<RAppliedDecl<RTraitDecl>> rTraits;
    for (auto* sTrait : syntax->traits)
    {
        auto e_rTrait = context.MakeTrait(sTrait, scope);
        RETURN_ON_ERROR(e_rTrait);

        rTraits.push_back(move(*e_rTrait));
    }

    rStructDecl->InitTraits(move(rTraits));
    return {};
}

void SmStructTask::BuildImplicitSymbol(BuildImplicitSymbolContext& context)
{
    SynthesizeMemberwiseCtor(context);
}


void SmStructTask::SynthesizeMemberwiseCtor(BuildImplicitSymbolContext& context)
{
    // memberwise constructor, 시그니처만 만든다 (resolve identifier용)
    vector<RFuncParameter> rParameters;

    for (auto* rVarDecl : rStructDecl->GetUnboundVars())
        rParameters.emplace_back(RFuncParameterKind::Init, rVarDecl->GetUnboundDeclType(), rVarDecl->GetName());

    auto* rCtor = context.MakeRDecl<RStructCtorDecl>(rStructDecl, RStructMemberAccessor::Public, RStructCtorKind::Memberwise);
    rCtor->Init(RDeclKey::Ctor(rParameters), vector<RTypeParam*>{}, move(rParameters), /*bLastParameterVariadic*/false);

    rStructDecl->AddCtor(rCtor);
}

} // namespace
} // namespace Citron
