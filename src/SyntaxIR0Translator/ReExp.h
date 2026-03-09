#pragma once

#include <memory>
#include <optional>

#include "RSymbol/RNames.h"

namespace Citron {

class RType;
class RFactory;
struct MExp;
struct MLoc;
struct MInitExp;
struct ReExpVisitor;

// NameResolvedExp 
struct ReExp
{
    virtual ~ReExp() { }
    virtual void Accept(ReExpVisitor& visitor) = 0;
};

struct ReExp_Loc : ReExp
{
    MLoc* mLoc;
    ReExp_Loc(MLoc* mLoc) : mLoc{mLoc}
    { }

    void Accept(ReExpVisitor& visitor) override;
};

// 기타의 경우, Value
struct ReExp_Exp : ReExp
{
    MExp* mExp;
    
    ReExp_Exp(MExp* mExp)
        : mExp{mExp}
    { }

    void Accept(ReExpVisitor& visitor) override;
};

struct ReExp_InitExp : ReExp
{
    MInitExp* mInitExp;

    ReExp_InitExp(MInitExp* mInitExp)
        : mInitExp{mInitExp}
    { }
    void Accept(ReExpVisitor& visitor) override;
};

RType* GetType(ReExp* reExp, RFactory* rFactory);

} // namespace Citron

#include "ReExpVisitor.g.h"