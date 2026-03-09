#include "ReExp.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "NSymbol/NClassVarDecl.h"
#include "NSymbol/NStructVarDecl.h"
#include "NSymbol/NEnumElemVarDecl.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MInitExp.h"

namespace Citron {

void ReExp_Loc::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_Exp::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_InitExp::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }

RType* GetType(ReExp* reExp, RFactory* rFactory)
{
    struct Visitor {
        using ResultType = RType*;
        RFactory* rFactory;

        ResultType Visit(ReExp_Loc* reExp) { return GetType(reExp->mLoc, rFactory); }
        ResultType Visit(ReExp_Exp* reExp) { return GetType(reExp->mExp, rFactory); }
        ResultType Visit(ReExp_InitExp* reExp) { return GetType(reExp->mInitExp, rFactory); }
    };

    return Accept(Visitor{rFactory}, reExp);
}

}