#pragma once
#include "InfraConfig.h"

#include <variant>
#include <string>
#include <vector>
#include <optional>

namespace Citron {

class IWriter;

using JsonItem = std::variant<struct JsonNull, struct JsonBool, struct JsonInt, struct JsonString, struct JsonArray, struct JsonObject>;

struct JsonNull
{
    bool operator==(const JsonNull& other) const
    {
        return true;
    }

    INFRA_API void ToString(IWriter& writer);
};

struct JsonBool
{
    bool value;
    JsonBool(bool value) : value(value) { }

    bool operator==(const JsonBool& other) const
    {
        return value == other.value;
    }

    INFRA_API void ToString(IWriter& writer);
};

struct JsonInt
{
    int value;
    JsonInt(int value) : value(value) { }

    bool operator==(const JsonInt& other) const
    {
        return value == other.value;
    }

    INFRA_API void ToString(IWriter& writer);
};

struct JsonString
{
    std::string value;
    JsonString(std::string value) : value(std::move(value)) { }
    INFRA_API JsonString(std::u32string value); // utility

    bool operator==(const JsonString& other) const
    {
        return value == other.value;
    }

    INFRA_API void ToString(IWriter& writer);
};

struct JsonArray
{
    std::vector<JsonItem> items;

    INFRA_API JsonArray(std::vector<JsonItem>&& items);
    INFRA_API JsonArray(std::initializer_list<JsonItem> items);
    INFRA_API JsonArray(const JsonArray&);
    INFRA_API JsonArray(JsonArray&&);

    INFRA_API ~JsonArray();

    INFRA_API bool operator==(const JsonArray& other) const;
    INFRA_API void ToString(IWriter& writer);
};

struct JsonObject
{
    std::vector<std::pair<std::string, JsonItem>> fields;

    INFRA_API JsonObject(std::initializer_list<std::pair<std::string, JsonItem>> list);
    INFRA_API JsonObject(const JsonObject& other);
    INFRA_API JsonObject(JsonObject&& other);
    INFRA_API bool operator==(const JsonObject& other) const;
    INFRA_API void ToString(IWriter& writer);
};

inline JsonBool ToJson(bool value)
{
    return JsonBool(value);
}

inline JsonInt ToJson(int value)
{
    return JsonInt(value);
}

inline JsonString ToJson(std::u32string& value)
{
    return JsonString(value);
}

// default
template<typename T>
JsonItem ToJson(T& t)
{
    return t.ToJson();
}

template<typename TElem>
JsonItem ToJson(std::optional<TElem>& oElem)
{
    if (oElem)
        return ToJson(*oElem);
    else
        return JsonNull();
}

template<typename TElem>
JsonItem ToJson(std::vector<TElem>& elems)
{
    std::vector<JsonItem> result;

    for (auto& elem : elems)
        result.push_back(ToJson(elem));

    return JsonArray(std::move(result));
}

inline void ToString(JsonItem& item, class IWriter& writer)
{
    return std::visit([&writer](auto&& i) { return i.ToString(writer); }, item);
}

}

// ToJson Helper
#define BEGIN_IMPLEMENT_JSON_CLASS(typeName) \
JsonItem typeName::ToJson() \
{ \
    return JsonObject { \
        { "$type", JsonString(#typeName) }, \

#define IMPLEMENT_JSON_MEMBER(memberName) \
        { #memberName, Citron::ToJson(memberName) },

#define IMPLEMENT_JSON_MEMBER_INDIRECT(ptr, memberName) \
        { #memberName, Citron::ToJson(ptr->memberName) },

#define IMPLEMENT_JSON_MEMBER_DIRECT(inst, memberName) \
        { #memberName, Citron::ToJson(inst.memberName) },

#define END_IMPLEMENT_JSON_CLASS() \
    }; \
}

#define BEGIN_IMPLEMENT_JSON_STRUCT(typeName, argName) \
JsonItem ToJson(typeName& argName) \
{ \
    return JsonObject { \
        { "$type", JsonString(#typeName) }, \

#define END_IMPLEMENT_JSON_STRUCT() \
    }; \
}

#define BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(typeName, argName) \
inline BEGIN_IMPLEMENT_JSON_STRUCT(typeName, argName)

#define END_IMPLEMENT_JSON_STRUCT_INLINE() \
    }; \
}

#define BEGIN_IMPLEMENT_JSON_ENUM_INLINE(typeName) \
inline JsonItem ToJson(typeName& arg) \
{ \
    switch(arg) \
    { \

#define IMPLEMENT_JSON_ENUM(e, n) \
    case e::n: return JsonString(#n);

#define END_IMPLEMENT_JSON_ENUM_INLINE() \
    }; \
    unreachable(); \
}
