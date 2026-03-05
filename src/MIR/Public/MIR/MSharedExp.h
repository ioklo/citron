#pragma once
#include "MIRConfig.h"

#include <memory>

namespace Citron {

class RType;
class RTypeArguments;
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
    MIR_API void Accept(MSharedExpVisitor& visitor) override;
};

// &c.x => MSharedExp_ClassVar(MLoc_LocalVar("c"), C::x)
struct MSharedExp_ClassVar : MSharedExp
{
    MLoc* base;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

    MIR_API void Accept(MSharedExpVisitor& visitor) override;
};

// box S* pS;
// &ps->x => MSharedExp_SharedStructVar(MLoc_LocalVar("pS"), S::x)
// MSharedExp_SharedStructVar
struct MSharedExp_SharedStructVar : MSharedExp
{   
    MLoc* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    
    MIR_API void Accept(MSharedExpVisitor& visitor) override;
};

// C c;
// shared A a = &c.s.a; => MSharedExp_StructVar(MSharedExp_ClassVar(MLoc_LocalVar("c"), C::s), A::a)
struct MSharedExp_StructVar : MSharedExp
{
    MSharedExp* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    
    MIR_API void Accept(MSharedExpVisitor& visitor) override;
};

MIR_API RType* GetType(MSharedExp* sharedExp, RFactory* rFactory);

} // namespace Citron

#include "MSharedExpVisitor.g.h"