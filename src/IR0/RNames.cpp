#include "RNames.h"

#include <Infra/Defaults.h>

namespace Citron 
{

RNormalName::RNormalName(std::string&& text)
    : text(std::move(text))
{
}

IMPLEMENT_DEFAULTS(RNormalName)

RNormalName RNormalName::Copy() const
{
    return RNormalName(std::string(text));
}

RReservedName::RReservedName(std::string&& text)
    : text(std::move(text))
{
}

IMPLEMENT_DEFAULTS(RReservedName)

RReservedName RReservedName::Copy() const
{
    return RReservedName(std::string(text));
}

RLambdaName::RLambdaName(int index)
    : index(index)
{
}

IMPLEMENT_DEFAULTS(RLambdaName)

RLambdaName RLambdaName::Copy() const
{
    return RLambdaName(index);
}

RConstructorParamName::RConstructorParamName(int index, std::string&& paramText)
    : index(index), paramText(std::move(paramText))
{
}

IMPLEMENT_DEFAULTS(RConstructorParamName)

RConstructorParamName RConstructorParamName::Copy() const
{
    return RConstructorParamName(index, std::string(paramText));
}

RName Copy(const RName& name)
{
    return std::visit([](auto&& name) { return RName(name.Copy()); }, name);
}

}