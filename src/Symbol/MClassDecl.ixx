export module Citron.Symbols:MClassDecl;

import <variant>;
import :MClassConstructorDecl;
import :MClassMemberFuncDecl;
import :MClassMemberVarDecl;
import :MNames;
import :MTypeDeclComponent;
import :MFuncDeclComponent;

namespace Citron
{

using MClassDeclOuter = std::variant<
    std::weak_ptr<class MModuleDecl>,
    std::weak_ptr<class MNamespaceDecl>,
    std::weak_ptr<class MClassDecl>,
    std::weak_ptr<class MStructDecl>
>;

class MClass;
class MInterface;

class MClassDecl
{
    struct BaseTypes
    {
        std::shared_ptr<MClass> baseClass;
        std::vector<std::shared_ptr<MInterface>> interfaces;
    };

    MClassDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MClassConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<MClassMemberVarDecl>> memberVars;
    MTypeDeclComponent typeComp;
    MFuncDeclComponent<std::shared_ptr<MClassMemberFuncDecl>> funcComp;

    std::optional<BaseTypes> oBaseTypes;
};

}