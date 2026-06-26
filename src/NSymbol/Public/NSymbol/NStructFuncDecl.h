#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <memory>

#include "RSymbol/RStructFuncDecl.h"
#include "NDecl.h"
#include "NGenericsComponent.h"
#include "NFuncDeclImpl_UsingNCommonFuncDeclComponent.h"

namespace Citron {

class NStructFuncDecl
    : public NDecl
    , private NGenericsComponent
    , public NFuncDeclImpl_UsingNCommonFuncDeclComponent<RStructFuncDecl>
{
public:
    using RDeclType = RStructFuncDecl;
    using RDeclResType = RDeclRes_StructFuncs;

public:
    NStructDecl* _struct;
    RAccessor accessor;
    std::string name;
    bool _static;

public:
    NSYMBOL_API NStructFuncDecl(
        NStructDecl* _struct, RAccessor accessor, bool bStatic, bool bSeqFunc, 
        const std::string& name);
    using NGenericsComponent::InitTypeParams;
    NSYMBOL_API void InitFuncReturnAndParams(RFuncReturn&& funcRet, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::IsSeqFunc;
    
public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }
    
    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    RTypeParamDecl* GetTypeParam(size_t index) override { return NGenericsComponent::GetTypeParam(index); }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};

}