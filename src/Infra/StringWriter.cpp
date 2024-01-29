#include "pch.h"
#include <Infra/StringWriter.h>
#include <cassert>


using namespace std;

namespace Citron {

StringWriter::StringWriter()
{
    indent = 0;
}

StringWriter::~StringWriter() = default;

void StringWriter::AddIndent()
{
    indent += 4;
}

void StringWriter::RemoveIndent()
{
    assert(0 <= indent - 4);
    indent -= 4;
}

void StringWriter::Write(const std::string& str)
{
    oss << str;
}

void StringWriter::WriteLine()
{
    oss << endl;

    for (int i = 0; i < indent; i++)
        oss << ' ';
}

}