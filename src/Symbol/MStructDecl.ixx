export module Citron.Symbols:MStructDecl;

import <variant>;
import :MNames;
import :MStructConstructorDecl;
import :MStructMemberFuncDecl;
import :MStructMemberVarDecl;
import :MTypeDeclContainerComponent;
import :MFuncDeclContainerComponent;
import :MTypeDeclOuter;

namespace Citron
{

class MStruct;
class MInterface;

export class MStructDecl 
    : private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MStructMemberFuncDecl>>
{
    struct BaseTypes
    {
        std::shared_ptr<MStruct> baseStruct;
        std::vector<std::shared_ptr<MInterface>> interfaces;
    };

    MTypeDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MStructConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<MStructMemberVarDecl>> memberVars;

    std::optional<BaseTypes> oBaseTypes;
};

}