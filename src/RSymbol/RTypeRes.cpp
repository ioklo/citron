#include "RTypeRes.h"
#include "RTypeDecl.h"

using namespace std;

namespace Citron {

RTypeRes ToRTypeRes(RTypeArguments* outerTypeArgs, RTypeDecl* typeDecl)
{
    struct Visitor
    {
        using ResultType = RTypeRes;
        RTypeArguments* outerTypeArgs;

        ResultType Visit(RClassDecl* rTypeDecl) { return RTypeRes_Class{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RStructDecl* rTypeDecl) { return RTypeRes_Struct{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(REnumDecl* rTypeDecl) { return RTypeRes_Enum{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(REnumElemDecl* rTypeDecl) { return RTypeRes_EnumElem{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RInterfaceDecl* rTypeDecl) { return RTypeRes_Interface{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RLambdaDecl* rTypeDecl) { return RTypeRes_Lambda{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RTraitDecl* rTypeDecl) { return RTypeRes_Trait{outerTypeArgs, rTypeDecl}; }
    };

    return Accept(Visitor{outerTypeArgs}, typeDecl);
}

} // namespace Citron