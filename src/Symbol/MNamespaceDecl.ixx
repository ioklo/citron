export module Citron.Symbols:MNamespaceDecl;

import :MTopLevelOuter;
import :MTopLevelDeclComponent;

namespace Citron
{

class MNamespaceDecl
{
    MTopLevelOuter outer;
    std::string name;

    MTopLevelDeclComponent topLevelComp;
};

}