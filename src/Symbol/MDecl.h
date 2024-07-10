#pragma once
#include <variant>
#include <memory>

namespace Citron
{

using MDecl = std::variant<
    std::shared_ptr<class MModuleDecl>,
    std::shared_ptr<class MNamespaceDecl>,

    std::shared_ptr<class MGlobalFuncDecl>,

    std::shared_ptr<class MStructDecl>,
    std::shared_ptr<class MStructConstructorDecl>,
    std::shared_ptr<class MStructMemberFuncDecl>,
    std::shared_ptr<class MStructMemberVarDecl>,
    std::shared_ptr<class MClassDecl>,
    std::shared_ptr<class MClassConstructorDecl>,
    std::shared_ptr<class MClassMemberFuncDecl>,
    std::shared_ptr<class MClassMemberVarDecl>,

    std::shared_ptr<class MEnumDecl>,
    std::shared_ptr<class MEnumElemDecl>,
    std::shared_ptr<class MEnumElemMemberVarDecl>,

    std::shared_ptr<class MLambdaDecl>,
    std::shared_ptr<class MLambdaMemberVarDecl>,

    std::shared_ptr<class MInterfaceDecl>
>;
}