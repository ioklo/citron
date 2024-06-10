export module Citron.Symbols:MNamespaceDecl;

import :MTopLevelOuter;
import :MNamespaceDeclContainerComponent;
import :MTypeDeclContainerComponent;
import :MFuncDeclContainerComponent;

namespace Citron
{

export class MNamespaceDecl 
    : private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>
{
    MTopLevelOuter outer;
    std::string name;

    MNamespaceDeclContainerComponent topLevelComp;
};

}