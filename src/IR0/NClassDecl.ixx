export module Citron.NDecls:NClassDecl;

import "IR0Config.h";
import <memory>;

import Citron.RDecls;
import :NDecl;
import :NTypeDecl;
import :NTypeDeclOuter;
import :NClassCtorDecl;
import :NClassFuncDecl;
import :NClassVarDecl;
import :NTypeDeclContainerComponent;
import :NFuncDeclContainerComponent;
import :NTypeDeclOuter;

namespace Citron
{

export class NClassDecl
    : public NDecl
    , public NTypeDecl
    , public NTypeDeclOuter
    , public NFuncDeclOuter
    , public RClassDecl
    , private NTypeDeclContainerComponent
    , private NFuncDeclContainerComponent<NClassFuncDecl>
{
    struct BaseTypes
    {
        std::shared_ptr<RType_Class> baseClass;
        std::vector<RType_Interface> interfaces;
    };

    NTypeDeclOuterWPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<NClassCtorDecl>> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<std::shared_ptr<NClassVarDecl>> vars;

    std::optional<BaseTypes> oBaseTypes;

    std::unordered_map<RName, std::shared_ptr<NClassVarDecl>> varsMap;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RDecl* GetROuter() override;
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RFuncDeclOuter
    //RDecl* GetRDecl() override { return this; }

    // from RClassDecl
    IR0_API std::optional<RMember_ClassVar> GetVar(const RTypeArgumentsPtr& typeArgs, const RName& name) override;
};

}