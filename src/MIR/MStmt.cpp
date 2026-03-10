#include "MStmt.h"
#include "MExp.h"

using namespace std;

namespace Citron {

void MStmt_Command::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_LocalVarDecl::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_LocalRefDecl::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_If::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_IfBind::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_For::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Continue::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Break::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Return::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Block::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Blank::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Exp::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Task::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Await::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Async::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Foreach::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Yield::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_CallBaseClassCtor::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_CallBaseStructCtor::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Directive::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Call::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Assign::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Do::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }

}