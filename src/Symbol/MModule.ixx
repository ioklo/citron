module;
#include <string>
export module Citron.MDecls:MModule;

import :MDecl;
import :MTypeDeclOuter;
import :MNamespaceDeclContainerComponent;
import :MTypeDeclContainerComponent;
import :MFuncDeclContainerComponent;

namespace Citron {

export class MModule
{
    std::string moduleName;

public:
    MModule(std::string&& moduleName);
};

}