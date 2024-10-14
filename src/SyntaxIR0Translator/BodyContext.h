#pragma once

#include <memory>
#include <IR0/RNames.h>
#include <IR0/RReturnType.h>

namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;
using RFuncDeclPtr = std::shared_ptr<class RFuncDecl>;

namespace SyntaxIR0Translator {

class BodyContext
{
public:
    RFuncDeclPtr curFuncDecl;

public:
    RFuncReturn GetReturnType();

};

} // namespace SyntaxIR0Translator 

} // namespace Citron