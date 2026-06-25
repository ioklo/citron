#pragma once
#include "NSymbolConfig.h"

#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RFuncDecl.h"
#include "NDecl.h"
#include "NFuncDeclOuter.h"

namespace Citron
{
struct RFuncParameter;

using NFuncDecl = std::variant<
    NGlobalFuncDecl*,
    NClassCtorDecl*,
    NClassFuncDecl*,
    NStructCtorDecl*,
    NStructDtorDecl*,
    NStructFuncDecl*,
    NLambdaDecl*>;

// RFuncDecl과 겹치는게 있으면 지우자
NSYMBOL_API NDecl* GetNDecl(NFuncDecl& funcDecl);
NSYMBOL_API RFuncDecl GetRFuncDecl(NFuncDecl& funcDecl);
NSYMBOL_API bool IsSeqFunc(NFuncDecl& funcDecl);
NSYMBOL_API NFuncDeclOuter GetNFuncDeclOuter(NFuncDecl& funcDecl);

}