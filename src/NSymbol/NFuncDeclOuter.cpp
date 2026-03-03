#include "NFuncDeclOuter.h"

#include "NNamespaceDecl.h"
#include "NGlobalFuncDecl.h"
#include "NClassDecl.h"
#include "NClassCtorDecl.h"
#include "NClassFuncDecl.h"
#include "NStructDecl.h"
#include "NStructCtorDecl.h"
#include "NStructDtorDecl.h"
#include "NStructFuncDecl.h"
#include "NLambdaDecl.h"

namespace Citron {

void NNamespaceDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NGlobalFuncDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NClassDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NClassCtorDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NClassFuncDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NStructDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NStructCtorDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NStructDtorDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NStructFuncDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }
void NLambdaDecl::Accept(NFuncDeclOuterVisitor& visitor) { visitor.Visit(this); }

} // namespace Citron