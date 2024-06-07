export module Citron.Symbols:MStructDecl;

import <variant>;
import :MNames;
import :MStructConstructorDecl;
import :MStructMemberFuncDecl;
import :MStructMemberVarDecl;
import :MTypeDeclComponent;
import :MFuncDeclComponent;

namespace Citron
{

using MStructDeclOuter = std::variant<
    std::weak_ptr<class MModuleDecl>,
    std::weak_ptr<class MNamespaceDecl>,
    std::weak_ptr<class MClassDecl>,
    std::weak_ptr<class MStructDecl>
>;

class MStruct;
class MInterface;

class MStructDecl
{
    struct BaseTypes
    {
        std::shared_ptr<MStruct> baseStruct;
        std::vector<std::shared_ptr<MInterface>> interfaces;
    };

    MStructDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MStructConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<MStructMemberVarDecl>> memberVars;
    MTypeDeclComponent typeComp;
    MFuncDeclComponent<std::shared_ptr<MStructMemberFuncDecl>> funcComp;

    std::optional<BaseTypes> oBaseTypes;
};

}