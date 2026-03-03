#include "RFuncDecl.h"

#include "RGlobalFuncDecl.h"
#include "RClassCtorDecl.h"
#include "RClassFuncDecl.h"
#include "RStructCtorDecl.h"
#include "RStructDtorDecl.h"
#include "RStructFuncDecl.h"
#include "RLambdaDecl.h"

namespace Citron {

void RGlobalFuncDecl::Accept(RFuncDeclVisitor& visitor) { visitor.Visit(this); }
void RClassCtorDecl::Accept(RFuncDeclVisitor& visitor) { visitor.Visit(this); }
void RClassFuncDecl::Accept(RFuncDeclVisitor& visitor) { visitor.Visit(this); }
void RStructCtorDecl::Accept(RFuncDeclVisitor& visitor) { visitor.Visit(this); }
void RStructDtorDecl::Accept(RFuncDeclVisitor& visitor) { visitor.Visit(this); }
void RStructFuncDecl::Accept(RFuncDeclVisitor& visitor) { visitor.Visit(this); }
void RLambdaDecl::Accept(RFuncDeclVisitor& visitor) { visitor.Visit(this); }

} // namespace Citron