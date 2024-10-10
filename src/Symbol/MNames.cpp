#include "MNames.h"

#include <Infra/Defaults.h>

namespace Citron 
{

MName_Normal::MName_Normal(std::string&& text)
    : text(std::move(text))
{
}

IMPLEMENT_DEFAULTS(MName_Normal)

MName_Normal MName_Normal::Copy() const
{
    return MName_Normal(std::string(text));
}

MName_Reserved::MName_Reserved(std::string&& text)
    : text(std::move(text))
{
}

IMPLEMENT_DEFAULTS(MName_Reserved)

MName_Reserved MName_Reserved::Copy() const
{
    return MName_Reserved(std::string(text));
}

MName_Lambda::MName_Lambda(int index)
    : index(index)
{
}

IMPLEMENT_DEFAULTS(MName_Lambda)

MName_Lambda MName_Lambda::Copy() const
{
    return MName_Lambda(index);
}

MName_ConstructorParam::MName_ConstructorParam(int index, std::string&& paramText)
    : index(index), paramText(std::move(paramText))
{
}

IMPLEMENT_DEFAULTS(MName_ConstructorParam)

MName_ConstructorParam MName_ConstructorParam::Copy() const
{
    return MName_ConstructorParam(index, std::string(paramText));
}

MName Copy(const MName& name)
{
    return std::visit([](auto&& name) { return MName(name.Copy()); }, name);
}

}