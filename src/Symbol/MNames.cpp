module Citron.MNames;

#define IMPLEMENT_DEFAULTS(className) \
    className::className(className&&) = default; \
    className& className::operator=(className&&) = default; \
    className::~className() = default;


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

MName_CtorParam::MName_CtorParam(int index, std::string&& paramText)
    : index(index), paramText(std::move(paramText))
{
}

IMPLEMENT_DEFAULTS(MName_CtorParam)

MName_CtorParam MName_CtorParam::Copy() const
{
    return MName_CtorParam(index, std::string(paramText));
}

MName Copy(const MName& name)
{
    return std::visit([](auto&& name) { return MName(name.Copy()); }, name);
}

}