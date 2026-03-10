#include "ImExp.h"
#include "RSymbol/DeclWithOuterTypeArgs.h"
#include "RSymbol/RStructFuncDecl.h"

using namespace std;

namespace Citron {

void ImExp_Namespace::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_GlobalFuncs::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_TypeVar::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_Class::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_ClassFuncs::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_Struct::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_StructFuncs::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_Enum::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_EnumElem::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_ClassVar::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_StructVar::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_Loc::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }
void ImExp_Exp::Accept(ImExpVisitor& visitor) { visitor.Visit(this); }

} // namespace Citron
