#include "ENames.h"

#include <string>
#include <variant>

#define IMPLEMENT_DEFAULTS(className) \
    className::className(className&&) = default; \
    className& className::operator=(className&&) = default; \
    className::~className() = default;

namespace Citron 
{

EName_Normal::EName_Normal(std::string&& text)
    : text(move(text))
{
}

IMPLEMENT_DEFAULTS(EName_Normal)

EName_Normal EName_Normal::Copy() const
{
    return EName_Normal(std::string(text));
}

EName_Reserved::EName_Reserved(std::string&& text)
    : text(move(text))
{
}

IMPLEMENT_DEFAULTS(EName_Reserved)

EName_Reserved EName_Reserved::Copy() const
{
    return EName_Reserved(std::string(text));
}

EName_Lambda::EName_Lambda(int index)
    : index(index)
{
}

IMPLEMENT_DEFAULTS(EName_Lambda)

EName_Lambda EName_Lambda::Copy() const
{
    return EName_Lambda(index);
}

EName_CtorParam::EName_CtorParam(int index, std::string&& paramText)
    : index(index), paramText(move(paramText))
{
}

IMPLEMENT_DEFAULTS(EName_CtorParam)

EName_CtorParam EName_CtorParam::Copy() const
{
    return EName_CtorParam(index, std::string(paramText));
}

EName Copy(const EName& name)
{
    return std::visit([](auto&& name) { return EName(name.Copy()); }, name);
}

}