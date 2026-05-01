#pragma once
#include <vector>
#include "RSymbol/RNames.h"

namespace Citron {

class RType;
struct MStmt;

// catch_resume(errorType errorName) -> returnType { bodies... }
struct MCatch_Resume { RType* errorType; RName errorName; RType* retType; std::vector<MStmt*> body; };
struct MCatch_Return { RType* errorType; RName errorName; std::vector<MStmt*> body; };
struct MCatch_Error { RType* errorType; RName errroName; std::vector<MStmt*> body; };
struct MCatch_Break { };
struct MCatch_Continue { };

using MCatch = std::variant<MCatch_Resume, MCatch_Return, MCatch_Error, MCatch_Break, MCatch_Continue>;

} // namespace Citron