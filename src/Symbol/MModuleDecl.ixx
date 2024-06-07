export module Citron.Symbols:MModuleDecl;

import <string>;
import :MTopLevelDeclComponent;

namespace Citron {

export class MModuleDecl
{
    std::string moduleName;
    bool bReference; // 지금 만들고 있는 모듈인지, 아닌지 여부를 판별할때 쓴다
    MTopLevelDeclComponent topLevelComp;
};

}