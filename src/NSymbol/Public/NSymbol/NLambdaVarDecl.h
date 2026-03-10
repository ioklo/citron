#pragma once
#include "NSymbolConfig.h"

#include <optional>

#include "RSymbol/RLambdaVarDecl.h"
#include "NDecl.h"

namespace Citron
{
class RType;

class NLambdaDecl;

class NLambdaVarDecl
    : public NDecl
    , public RLambdaVarDecl
{
public:
    NLambdaDecl* lambda;
    RType* type;
    RName name;

    NSYMBOL_API NLambdaVarDecl(RType* type, const RName& name);
    NSYMBOL_API void InitLambda(NLambdaDecl* lambda);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    NSYMBOL_API RType* GetDeclType(RTypeArguments* typeArgs) override;

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    size_t GetTypeParamCount() override { return 0; }
    RTypeParamDecl* GetTypeParam(size_t index) override { return nullptr; }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RLambdaVarDecl
    NSYMBOL_API RName GetName() override { return name; }
    NSYMBOL_API RType* GetUnboundDeclType() override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

};

}