export module Citron.Symbols:MClassDecl;

import "std.h";

import :MClassConstructorDecl;
import :MClassMemberFuncDecl;
import :MClassMemberVarDecl;
import :MNames;
import :MTypeDeclContainerComponent;
import :MFuncDeclContainerComponent;
import :MTypeDeclOuter;
import :MAccessor;

namespace Citron
{

export class MClass;
export class MInterface;

export class MClassDecl 
    : private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MClassMemberFuncDecl>>
{
    struct BaseTypes
    {
        std::shared_ptr<MClass> baseClass;
        std::vector<std::shared_ptr<MInterface>> interfaces;
    };

    MTypeDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MClassConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<MClassMemberVarDecl>> memberVars;

    std::optional<BaseTypes> oBaseTypes;
};

}