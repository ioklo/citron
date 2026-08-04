#pragma once
#include "MIRConfig.h"

#include <memory>
#include <vector>
#include "Infra/Ref.h"
#include "RSymbol/RAppliedDecl.h"

namespace Citron {

class RType;
class RClassVarDecl;
class RStructVarDecl;
class RFactory;

struct MLoc;
struct MSharedExpVisitor;

// &연산으로 shared를 만들어내는 exp
struct MSharedExp
{
public:
    virtual ~MSharedExp() {}
    virtual void Accept(MSharedExpVisitor& visitor) = 0;
};

// &C.x
struct MSharedExp_Static : MSharedExp
{
    MLoc* loc;
    std::vector<RAppliedDecl<RStructVarDecl>> segments;
    MSharedExp_Static(MLoc* loc)
        : loc{loc}, segments{}
    { }
    MIR_API void Accept(MSharedExpVisitor& visitor) override;
};

// &c.x => MSharedExp_ClassVar(MLoc_LocalVar("c"), C::x)
struct MSharedExp_ClassVar : MSharedExp
{
    MLoc* base;
    RAppliedDecl<RClassVarDecl> appliedDecl;
    std::vector<RAppliedDecl<RStructVarDecl>> segments;

    MSharedExp_ClassVar(MLoc* base, TakeRef<RAppliedDecl<RClassVarDecl>> appliedDecl)
        : base{base}, appliedDecl{appliedDecl.Take()}, segments{}
    { }
    MIR_API void Accept(MSharedExpVisitor& visitor) override;
};

// shared<S> pS;
// &ps->x => MSharedExp_SharedStructVar(MLoc_LocalVar("pS"), S::x, [])
// ps->x.y => MSharedExp_SharedStructVar(MLoc_LocalVar("pS"), S::x, [S::y])
struct MSharedExp_SharedStructVar : MSharedExp
{   
    MLoc* base;
    RAppliedDecl<RStructVarDecl> appliedDecl;
    std::vector<RAppliedDecl<RStructVarDecl>> segments;

    MSharedExp_SharedStructVar(MLoc* base, TakeRef<RAppliedDecl<RStructVarDecl>> appliedDecl)
        : base{base}, appliedDecl{appliedDecl.Take()}, segments{}
    { }
    MIR_API void Accept(MSharedExpVisitor& visitor) override;
};

MIR_API RType* GetType(MSharedExp* sharedExp, RFactory* rFactory);

} // namespace Citron

#include "MSharedExpVisitor.g.h"