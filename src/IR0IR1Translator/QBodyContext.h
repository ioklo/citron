#pragma once

#include <string>

namespace Citron {

class QValue;
class QBlock;

namespace IR0IR1Translator {

class QBodyContext
{
public:
    QValue* MakeValue();
    QBlock* AddBlock(const std::string& debugText);
};

} // namespace IR0IR1Translator


} // namespace Citron