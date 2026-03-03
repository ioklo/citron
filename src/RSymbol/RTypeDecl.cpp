#include "RTypeDecl.h"

#include "RClassDecl.h"
#include "RStructDecl.h"
#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "RInterfaceDecl.h"
#include "RLambdaDecl.h"
#include "RTypeParamDecl.h"

namespace Citron {

void RClassDecl::Accept(RTypeDeclVisitor& visitor) { visitor.Visit(this); }
void RStructDecl::Accept(RTypeDeclVisitor& visitor) { visitor.Visit(this); }
void REnumDecl::Accept(RTypeDeclVisitor& visitor) { visitor.Visit(this); }
void REnumElemDecl::Accept(RTypeDeclVisitor& visitor) { visitor.Visit(this); }
void RInterfaceDecl::Accept(RTypeDeclVisitor& visitor) { visitor.Visit(this); }
void RLambdaDecl::Accept(RTypeDeclVisitor& visitor) { visitor.Visit(this); }
void RTypeParamDecl::Accept(RTypeDeclVisitor& visitor) { visitor.Visit(this); }

} // namespace Citron