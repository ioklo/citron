#pragma once
#include "RSymbolConfig.h"
#include <string>

namespace Citron {

using RModuleName = std::string;
class RNamespace;

// 각 모듈별로 있는 syntax tree. 모듈끼리는 namespaceDecl을 공유하지 않는다
class RModule
{   
    RModuleName name;
    RNamespace* rootNamespace;

public:
    RSYMBOL_API RModule(RModuleName&& name);
    void InitRootNamespace(RNamespace* rootNamespace) { this->rootNamespace = rootNamespace; }
    RModuleName& GetName() { return name; }

    // 익스포트 하지 않고, 내부에서만 호출하는 함수
    void FillIdentifier(std::string& buffer);
};

} // namespace Citron