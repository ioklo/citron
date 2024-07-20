#include "pch.h"
#include <Infra/Json.h>
#include <Infra/IWriter.h>
#include <sstream>

using namespace std;

namespace {
string QuoteString(const string& str)
{
    // escape required
    // \ => \\
    // " => \"
    // (enter) => \n

    // 바꿀 모든 문자가 0~127사이에 있고, utf-8에서 이어지는 문자들은 모두 128이상으로 인코딩 되기 때문에 그냥 돌면 된다
    ostringstream oss;

    oss << '"';

    for (char c : str)
    {
        if (c == u8'\\' || c == u8'"')
            oss << '\\' << c;
        else if (c == u8'\r')
            oss << "\\r";
        else if (c == u8'\n')
            oss << "\\n";
        else
            oss << c;
    }

    oss << '"';

    return oss.str();
}

}

namespace Citron {

void JsonNull::ToString(IWriter& writer)
{
    writer.Write("null");
}

void JsonBool::ToString(IWriter& writer)
{
    writer.Write(value ? "true" : "false");
}

void JsonInt::ToString(IWriter& writer)
{
    ostringstream oss;
    oss << value;

    writer.Write(oss.str());
}

void JsonString::ToString(IWriter& writer)
{
    writer.Write(QuoteString(value));
}

JsonArray::JsonArray(std::vector<JsonItem>&& items)
    : items(std::move(items)) { }

JsonArray::JsonArray(std::initializer_list<JsonItem> items)
    : items(std::move(items)) { }

JsonArray::JsonArray(const JsonArray&) = default;
JsonArray::JsonArray(JsonArray&&) = default;

JsonArray& JsonArray::operator=(const JsonArray& other)
{
    items = other.items;
    return *this;
}

JsonArray& JsonArray::operator=(JsonArray&& other) noexcept
{
    items = std::move(other.items);
    return *this;
}

JsonArray::~JsonArray()
{
}

INFRA_API void JsonArray::ToString(IWriter& writer)
{
    if (items.size() == 0)
    {
        writer.Write("[]");
        return;
    }

    writer.Write("[");
    writer.AddIndent();
    writer.WriteLine();

    bool bFirst = true;
    for (auto& item : items)
    {
        if (bFirst)
        {
            bFirst = false;
        }
        else 
        {
            writer.Write(","); 
            writer.WriteLine();
        }

        Citron::ToString(item, writer);
    }

    writer.RemoveIndent();
    writer.WriteLine();
    writer.Write("]");
}

bool JsonArray::operator==(const JsonArray& other) const
{
    return items == other.items;
}

JsonObject::JsonObject(std::initializer_list<std::pair<std::string, JsonItem>> list)
    : fields(std::move(list)) { }

JsonObject::JsonObject(const JsonObject& other) = default;
JsonObject::JsonObject(JsonObject&& other) = default;

JsonObject& JsonObject::operator=(const JsonObject&) = default;
JsonObject& JsonObject::operator=(JsonObject&&) noexcept = default;

INFRA_API void JsonObject::ToString(IWriter& writer)
{
    if (fields.empty())
    {
        writer.Write("{}");
        return;
    }

    writer.Write("{");

    writer.AddIndent();
    writer.WriteLine();

    bool bFirst = true;
    for (auto& kv : fields)
    {
        if (bFirst)
        {
            bFirst = false;
        }
        else
        {
            writer.Write(",");
            writer.WriteLine();
        }

        // "key": "value"
        writer.Write(QuoteString(kv.first));
        writer.Write(": ");
        Citron::ToString(kv.second, writer);
    }

    writer.RemoveIndent();
    writer.WriteLine();
    writer.Write("}");
}

bool JsonObject::operator==(const JsonObject& other) const
{
    return fields == other.fields;
}

}