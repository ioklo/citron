export module Citron.Symbols:MModuleDecl;

import "std.h";

import :MNamespaceDeclContainerComponent;
import :MTypeDeclContainerComponent;
import :MFuncDeclContainerComponent;
import :MGlobalFuncDecl;

namespace Citron {

export class MModuleDecl 
    : private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>

{
    std::string moduleName;
    bool bReference; // 지금 만들고 있는 모듈인지, 아닌지 여부를 판별할때 쓴다
};

}