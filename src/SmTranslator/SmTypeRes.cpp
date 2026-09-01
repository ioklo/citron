#include "SmTypeRes.h"
#include "RSymbol/RTypeDecl.h"
#include "RSymbol/RTypeAliasDecl.h"
#include "RSymbol/RTypes.h"

using namespace std;

namespace Citron {

SmTypeRes ToSmTypeRes(RTypeArguments* outerTypeArgs, RTypeDecl* typeDecl)
{
    struct Visitor
    {
        using ResultType = SmTypeRes;
        RTypeArguments* outerTypeArgs;

        ResultType Visit(RClassDecl* rTypeDecl) { return SmTypeRes_Class{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RStructDecl* rTypeDecl) { return SmTypeRes_Struct{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(REnumDecl* rTypeDecl) { return SmTypeRes_Enum{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(REnumElemDecl* rTypeDecl) { return SmTypeRes_EnumElem{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RInterfaceDecl* rTypeDecl) { return SmTypeRes_Interface{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RLambdaDecl* rTypeDecl) { return SmTypeRes_Lambda{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RTraitDecl* rTypeDecl) { return SmTypeRes_Trait{outerTypeArgs, rTypeDecl}; }
        ResultType Visit(RTypeAliasDecl* rTypeDecl) { return SmTypeRes_Type{rTypeDecl->GetTargetType()->Apply(outerTypeArgs)}; }
    };

    return Accept(Visitor{outerTypeArgs}, typeDecl);
}

} // namespace Citron
