#include "MNames.h"

#include <Infra/Defaults.h>

namespace Citron 
{

MNormalName::MNormalName(std::string&& text)
    : text(std::move(text))
{
}

IMPLEMENT_DEFAULTS(MNormalName)

MNormalName MNormalName::Copy() const
{
    return MNormalName(std::string(text));
}

MReservedName::MReservedName(std::string&& text)
    : text(std::move(text))
{
}

IMPLEMENT_DEFAULTS(MReservedName)

MReservedName MReservedName::Copy() const
{
    return MReservedName(std::string(text));
}

MLambdaName::MLambdaName(int index)
    : index(index)
{
}

IMPLEMENT_DEFAULTS(MLambdaName)

MLambdaName MLambdaName::Copy() const
{
    return MLambdaName(index);
}

MConstructorParamName::MConstructorParamName(int index, std::string&& paramText)
    : index(index), paramText(std::move(paramText))
{
}

IMPLEMENT_DEFAULTS(MConstructorParamName)

MConstructorParamName MConstructorParamName::Copy() const
{
    return MConstructorParamName(index, std::string(paramText));
}

MName Copy(const MName& name)
{
    return std::visit([](auto&& name) { return MName(name.Copy()); }, name);
}

}