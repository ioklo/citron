// CodeGenerator.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <string>
#include <vector>
#include <sstream>
#include <variant>
#include <fmt/core.h>
#include <locale>
#include <filesystem>
#include <fstream>

using namespace std;
using namespace std::filesystem;

struct ClassMemberInfo
{
    string type;
    string memberVarName;
    string getterName;
    bool bUsePimpl;
    bool bUseMove;
};

struct CommonInfo
{
    string linkage;
};

struct ClassInfo
{
    string name;
    vector<ClassMemberInfo> memberInfos;
    bool bDefaultsInline;
    vector<string> extraConstructors;
};

struct EnumInfo
{
    string name;
    vector<string> cases;
};

struct StructMemberInfo
{
    string type;
    string name;
    bool bUseMove;
};

struct StructInfo
{
    string name;
    vector<StructMemberInfo> memberInfos;
    vector<string> extraConstructors;
};

struct VariantInfo
{
    string name;
    string argName;

    vector<string> memberNames; // with 'class ' 'struct ', ex) class IdentifierExpSyntax
};

using ItemInfo = std::variant<StructInfo, EnumInfo, ClassInfo, VariantInfo>;

void AddNewLineIfNeeded(bool& bModified, ostringstream& oss)
{
    if (bModified)
    {
        oss << endl;
        bModified = false;
    }
}

// u8string
std::string ReadAll(path filePath)
{
    ifstream ifs(filePath);
    ostringstream oss;
    oss << ifs.rdbuf();

    return oss.str();


    //ifs.seekg(0, ifs.end);
    //ifstream::pos_type length = ifs.tellg();
    //ifs.seekg(0, ifs.beg);

    //std::string s;
    //s.resize(length);

    //ifs.read(s.data(), length);

    // return s;
}

void WriteAll(path filePath, string contents)
{
    if (exists(filePath))
    {
        auto prevContents = ReadAll(filePath);
        if (contents == prevContents) return;
    }

    filesystem::create_directories(filePath.parent_path());

    ofstream ofs(filePath);
    ofs.write(contents.c_str(), contents.size());
    ofs.close();
}

// struct는 거진 인라인으로 작성한다
void GenerateStruct(CommonInfo& commonInfo, StructInfo structInfo, ostringstream& hStream)
{
    hStream << "struct " << structInfo.name << endl;
    hStream << "{" << endl;

    for (auto& memberInfo : structInfo.memberInfos)
        hStream << "    " << memberInfo.type << " " << memberInfo.name << ";" << endl;

    if (!structInfo.memberInfos.empty())
        hStream << endl;

    // 생성자 
    // ArgumentSyntax(bool bOut, bool bParams, ExpSyntax exp)
    hStream << "    " << structInfo.name << "(";

    bool bFirst = true;
    for (auto& memberInfo : structInfo.memberInfos)
    {
        if (bFirst) bFirst = false;
        else hStream << ", ";

        hStream << memberInfo.type << " " << memberInfo.name;
    }

    hStream << ")" << endl;

    // : bOut(bOut), bParams(bParams), exp(std::move(exp))
    if (!structInfo.memberInfos.empty())
    {
        hStream << "        : ";
        bFirst = true;
        for (auto& memberInfo : structInfo.memberInfos)
        {
            if (bFirst) bFirst = false;
            else hStream << ", ";

            if (memberInfo.bUseMove)
                hStream << memberInfo.name << "(std::move(" << memberInfo.name << "))";
            else
                hStream << memberInfo.name << "(" << memberInfo.name << ")";
        }
    }

    hStream << " { }" << endl;

    for (auto& extraConstructor : structInfo.extraConstructors)    
        hStream << endl << extraConstructor << endl;

    hStream << "};" << endl << endl;
}

void GenerateClass(CommonInfo& commonInfo, ClassInfo& classInfo, ostringstream& hStream, ostringstream& cppStream)
{
    // class begin
    hStream << "class " << classInfo.name << endl;
    hStream << "{" << endl;
    
    bool bHModified = false, bCppModified = false;
    bool bUsePimpl = false;
    for (auto& memberInfo : classInfo.memberInfos)
    {
        if (!memberInfo.bUsePimpl)
        {
            hStream << fmt::format(R"---(    {} {};)---", memberInfo.type, memberInfo.memberVarName) << endl;
            bHModified = true;
        }
        else
            bUsePimpl = true;
    }

    if (bUsePimpl)
    {
        AddNewLineIfNeeded(bHModified, hStream);

        hStream << "    struct Impl;" << endl;
        hStream << "    std::unique_ptr<Impl> impl;" << endl;
        bHModified = true;

        cppStream << fmt::format(R"---(struct {}::Impl 
{{
)---", classInfo.name);

        for (auto& memberInfo : classInfo.memberInfos)
        {
            if (!memberInfo.bUsePimpl) continue;
            cppStream << fmt::format("    {} {};", memberInfo.type, memberInfo.memberVarName) << endl;
        }

        cppStream << "};" << endl;
        bCppModified = true;
    }

    AddNewLineIfNeeded(bHModified, hStream);
    hStream << "public:" << endl;

    // bDefaultsInline이 true인데 bUsePimpl이면 워닝
    if (bUsePimpl && classInfo.bDefaultsInline)
        cout << classInfo.name << ": bDefaultsInline 인데 멤버함수 중에 bUsePimpl인 것이 있습니다" << endl;
    

    bool bDefaultsInline = !bUsePimpl && classInfo.bDefaultsInline;

    // 생성자
    if (bDefaultsInline) // inline mode
    {
        // IdentifierExpSyntax(std::string value) : value(std::move(value)) {{ }}
        hStream << "    " << classInfo.name << "(";
        bool bFirst = true;
        for (auto& memberInfo : classInfo.memberInfos)
        {
            if (bFirst) bFirst = false;
            else hStream << ", ";
            hStream << memberInfo.type << ' ' << memberInfo.memberVarName;
        }
        hStream << ")";

        bFirst = true;
        for (auto& memberInfo : classInfo.memberInfos)
        {
            if (bFirst)
            {
                hStream << endl << "        : ";
                bFirst = false;
            }
            else
                hStream << ", ";

            hStream << memberInfo.memberVarName << '(';

            if (memberInfo.bUseMove)
                hStream << "std::move(" << memberInfo.memberVarName << "))";
            else
                hStream << memberInfo.memberVarName << ')';
        }

        hStream << " { }" << endl;
        bHModified = true;
    }
    else
    {
        AddNewLineIfNeeded(bHModified, hStream);
        // SYNTAX_API IdentifierExpSyntax(std::string value);
        hStream << "    " << commonInfo.linkage << ' ' << classInfo.name << "(";
        bool bFirst = true;
        for (auto& memberInfo : classInfo.memberInfos)
        {
            if (bFirst) bFirst = false;
            else hStream << ", ";
            hStream << memberInfo.type << ' ' << memberInfo.memberVarName;
        }
        hStream << ");" << endl;
        bHModified = true;

        // IdentifierExpSyntax::IdentifierExpSyntax(std::string value) : value(std::move(value)) { }

        AddNewLineIfNeeded(bCppModified, cppStream);
        cppStream << classInfo.name << "::" << classInfo.name << "(";
        bFirst = true;
        for (auto& memberInfo : classInfo.memberInfos)
        {
            if (bFirst) bFirst = false;
            else cppStream << ", ";
            cppStream << memberInfo.type << ' ' << memberInfo.memberVarName;
        }
        cppStream << ")" << endl;
        cppStream << "    : ";

        // non pimpl
        bFirst = true;
        for (auto& memberInfo : classInfo.memberInfos)
        {
            if (memberInfo.bUsePimpl) continue;

            if (bFirst)
            {
                bFirst = false;
            }
            else
                cppStream << ", ";

            cppStream << memberInfo.memberVarName << '(';

            if (memberInfo.bUseMove)
                cppStream << "std::move(" << memberInfo.memberVarName << "))";
            else
                cppStream << memberInfo.memberVarName << ')';
        }

        if (bUsePimpl)
        {
            // pimpl
            // impl(new Impl{ std::move(value) })
            if (!bFirst) cppStream << ", ";

            cppStream << "impl(new Impl{ ";
            bFirst = true;
            for (auto& memberInfo : classInfo.memberInfos)
            {
                if (!memberInfo.bUsePimpl) continue;

                if (bFirst) bFirst = false;
                else cppStream << ", ";

                if (memberInfo.bUseMove)
                    cppStream << "std::move(" << memberInfo.memberVarName << ")";
                else
                    cppStream << memberInfo.memberVarName;
            }

            cppStream << " })";
        }

        cppStream << " { }" << endl;
        bCppModified = true;
    }

    // 추가 생성자
    for (auto& extraConstructor : classInfo.extraConstructors)
    {
        hStream << "    " << extraConstructor << endl;
    }

    bHModified |= !classInfo.extraConstructors.empty();

    // copy constructor, move constructor
    if (!bDefaultsInline)
    {
        // IdentifierExpSyntax(const IdentifierExpSyntax&) = delete;
        // SYNTAX_API IdentifierExpSyntax(IdentifierExpSyntax&& other);
        hStream << "    " << classInfo.name << "(const " << classInfo.name << "&) = delete;" << endl;
        hStream << "    " << commonInfo.linkage << " " << classInfo.name << "(" << classInfo.name << "&&) noexcept;" << endl;
        bHModified = true;

        // IdentifierExpSyntax::IdentifierExpSyntax(IdentifierExpSyntax&& other)
        AddNewLineIfNeeded(bCppModified, cppStream);
        cppStream << classInfo.name << "::" << classInfo.name << "(" << classInfo.name << "&& other) noexcept = default;" << endl;
        bCppModified = true;
    }
    else
    {
        // copy는 delete로 막아야 한다
        hStream << "    " << classInfo.name << "(const " << classInfo.name << "&) = delete;" << endl;
        hStream << "    " << classInfo.name << "(" << classInfo.name << "&&) = default;" << endl;
        bHModified = true;
    }

    // 소멸자, inline이 아닐때만 생성한다
    if (!bDefaultsInline)
    {
        // SYNTAX_API ~IdentifierExpSyntax();
        hStream << "    " << commonInfo.linkage << " ~" << classInfo.name << "();" << endl;
        bHModified = true;

        // IdentifierExpSyntax::~IdentifierExpSyntax() = default;
        AddNewLineIfNeeded(bCppModified, cppStream);
        cppStream << classInfo.name << "::~" << classInfo.name << "() = default;" << endl;
        bCppModified = true;
    }
    

    // copy assignment, move assignment
    if (!bDefaultsInline)
    {
        // IdentifierExpSyntax& operator=(const IdentifierExpSyntax& other) = delete;
        // SYNTAX_API IdentifierExpSyntax& operator=(IdentifierExpSyntax&& other) noexcept;
        AddNewLineIfNeeded(bHModified, hStream);
        hStream << "    " << classInfo.name << "& operator=(const " << classInfo.name << "& other) = delete;" << endl;
        hStream << "    " << commonInfo.linkage << " " << classInfo.name << "& operator=(" << classInfo.name << "&& other) noexcept;" << endl;
        bHModified = true;

        // IdentifierExpSyntax& IdentifierExpSyntax::operator=(IdentifierExpSyntax&& other) noexcept = default;
        AddNewLineIfNeeded(bCppModified, cppStream);
        cppStream << classInfo.name << "& " << classInfo.name << "::operator=(" << classInfo.name << "&& other) noexcept = default;" << endl;
        bCppModified = true;
    }
    else
    {
        // IdentifierExpSyntax& operator=(const IdentifierExpSyntax& other) = delete;
        AddNewLineIfNeeded(bHModified, hStream);
        hStream << "    " << classInfo.name << "& operator=(const " << classInfo.name << "& other) = delete;" << endl;
        hStream << "    " << classInfo.name << "& operator=(" << classInfo.name << "&& other) = default;" << endl;
        bHModified = true;
    }


    AddNewLineIfNeeded(bHModified, hStream);
    AddNewLineIfNeeded(bCppModified, cppStream);

    // Getter
    for (auto& memberInfo : classInfo.memberInfos)
    {
        if (!memberInfo.bUsePimpl)
        {
            //    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }
            hStream << "    " << memberInfo.type << "& " << memberInfo.getterName << "() { return " << memberInfo.memberVarName << "; }" << endl;
            bHModified = true;
        }
        else
        {
            //     SYNTAX_API std::vector<TypeExpSyntax>& GetTypeArgs();
            hStream << "    " << commonInfo.linkage << ' ' << memberInfo.type << "& " << memberInfo.getterName << "();" << endl;
            bHModified = true;

            // std::vector<TypeExpSyntax>& IdentifierExpSyntax::GetTypeArgs()
            // {
            //     return impl->typeArgs;
            // }
            AddNewLineIfNeeded(bCppModified, cppStream);

            cppStream << memberInfo.type << "& " << classInfo.name << "::" << memberInfo.getterName << "()" << endl;
            cppStream << '{' << endl;
            cppStream << "    return impl->" << memberInfo.memberVarName << ';' << endl;
            cppStream << '}' << endl;
            bCppModified = true;
        }
    }

    // Json

    // SYNTAX_API JsonItem ToJson();
    AddNewLineIfNeeded(bHModified, hStream);
    hStream << "    " << commonInfo.linkage << " JsonItem ToJson();";
    bHModified = true;

    // JsonItem IdentifierExpSyntax::ToJson()
    AddNewLineIfNeeded(bCppModified, cppStream);
    cppStream << "JsonItem " << classInfo.name << "::ToJson()" << endl;
    cppStream << "{" << endl;
    cppStream << "    return JsonObject {" << endl;
    cppStream << "        { \"$type\", JsonString(\"" << classInfo.name << "\") }," << endl;

    for (auto& memberInfo : classInfo.memberInfos)
    {
        if (!memberInfo.bUsePimpl)
            cppStream << "        { \"" << memberInfo.memberVarName << "\", Citron::ToJson(" << memberInfo.memberVarName << ") }," << endl;
        else
            cppStream << "        { \"" << memberInfo.memberVarName << "\", Citron::ToJson(impl->" << memberInfo.memberVarName << ") }," << endl;
    }

    cppStream << "    };" << endl;
    cppStream << "}" << endl << endl;
    bCppModified = true;

    AddNewLineIfNeeded(bHModified, hStream);
    hStream << "};" << endl << endl;
    

    // 이름: IdentifierExpSyntax
    // 멤버: ("std::string" "value", "GetValue"), ("std::vector<TypeExpSyntax>", "typeArgs", "GetTypeArgs")
    // 따로 만들 생성자(h, cpp): 
    //   h: IdentifierExpSyntax(std::string value);
    //   cpp: IdentifierExpSyntax::IdentifierExpSyntax(std::string value) : value(std::move(value)) { }
    // 
    // 따로 만들 새성자 inline(h):
    //   h: IdentifierExpSyntax(std::string value) : value(std::move(value)) { }

    // 생성할 것

    // 1. 인라인 모드, impl모드, 반참조 모드
    // 인라인: 생성자, 

    // 기본 생성자 (인라인이라면
    // 따로 만들 생성자 (h, cpp)
    // Getters (인라인이라면 h에 전부, 아니라면 cpp에도)
    // JSON 선언 (h)
}

void GenerateEnum(CommonInfo& commonInfo, EnumInfo enumInfo, ostringstream& hStream)
{
    hStream << "enum class " << enumInfo.name << endl;
    hStream << "{" << endl;

    for (auto& c : enumInfo.cases)
        hStream << "    " << c << "," << endl;

    hStream << "};" << endl << endl;

    // json
    // inline JsonItem ToJson(typeName & arg)
    hStream << "inline JsonItem ToJson(" << enumInfo.name << "& arg)" << endl;
    hStream << "{" << endl;
    hStream << "    switch(arg)" << endl;
    hStream << "    {" << endl;

    // case AccessModifierSyntax::Public: return JsonString("Public");
    for (auto& c : enumInfo.cases)
        hStream << "    case " << enumInfo.name << "::" << c << ": return JsonString(\"" << c << "\");" << endl;
    
    hStream << "    }" << endl;
    hStream << "    unreachable();" << endl;
    hStream << "}" << endl << endl;
}

void GenerateVariant(CommonInfo& commonInfo, VariantInfo info, ostringstream& hStream, ostringstream& cppStream)
{
    // using ExpSyntax = std::variant<
    // >;
    hStream << "using " << info.name << " = std::variant<" << endl;
    bool bFirst = true;
    for (auto& memberName : info.memberNames)
    {
        if (bFirst) bFirst = false;
        else hStream << "," << endl;
        hStream << "    " << memberName;
    }
    hStream << ">;" << endl << endl;

    // SYNTAX_API JsonItem ToJson(ExpSyntax& exp);
    hStream << commonInfo.linkage << " JsonItem ToJson(" << info.name << "& " << info.argName << ");" << endl << endl;

    /*JsonItem ToJson(ExpSyntax & exp)
    {
        return std::visit([](auto&& exp) { return exp.ToJson(); }, exp);
    }*/

    cppStream << "JsonItem ToJson(" << info.name << "& " << info.argName << ")" << endl;
    cppStream << "{" << endl;
    cppStream << "    return std::visit([](auto&& " << info.argName << ") { return " << info.argName << ".ToJson(); }, " << info.argName << ");" << endl;
    cppStream << "}" << endl;
    cppStream << endl;
}

void GenerateSyntax(path srcPath)
{   
    // src/Syntax/Syntaxes.g.h
    // src/Syntax/Syntaxes.g.cpp
    path hPath = [srcPath]() mutable { return absolute(srcPath.append("..").append("include").append("Syntax").append("Syntaxes.g.h")); }();
    path cppPath = [srcPath]() mutable { return srcPath.append("Syntax").append("Syntaxes.g.cpp"); }();

    ostringstream hStream, cppStream;

    //  
    hStream << R"---(#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <vector>
#include <optional>
#include <variant>

#include <Infra/Json.h>
#include <Infra/Unreachable.h>

namespace Citron {

)---";

    cppStream << R"---(#include "pch.h"

#include <Syntax/Syntaxes.g.h>
#include <Infra/Json.h>

using namespace std;

namespace Citron {

)---";

    CommonInfo commonInfo = {
        /* linkage */ "SYNTAX_API",
    };

    ItemInfo itemInfos[] = {

        VariantInfo { 
            "StmtSyntax",
            "stmt",
            {
                "class CommandStmtSyntax",
                "class VarDeclStmtSyntax",
                "class IfStmtSyntax",
                "class IfTestStmtSyntax",
                "class ForStmtSyntax",

                "class ContinueStmtSyntax",
                "class BreakStmtSyntax",
                "class ReturnStmtSyntax",
                "class BlockStmtSyntax",
                "class BlankStmtSyntax",
                "class ExpStmtSyntax",

                "class TaskStmtSyntax",
                "class AwaitStmtSyntax",
                "class AsyncStmtSyntax",
                "class ForeachStmtSyntax",
                "class YieldStmtSyntax",

                "class DirectiveStmtSyntax"
            }
        },

        VariantInfo { 
            "ExpSyntax", "exp", 
            {
                "class IdentifierExpSyntax",
                "class StringExpSyntax",
                "class IntLiteralExpSyntax",
                "class BoolLiteralExpSyntax",
                "class NullLiteralExpSyntax",
                "class BinaryOpExpSyntax",
                "class UnaryOpExpSyntax",
                "class CallExpSyntax",
                "class LambdaExpSyntax",
                "class IndexerExpSyntax",
                "class MemberExpSyntax",
                "class ListExpSyntax",
                "class NewExpSyntax",
                "class BoxExpSyntax",
                "class IsExpSyntax",
                "class AsExpSyntax"
            }
        },

        VariantInfo {
            "TypeExpSyntax",
            "typeExp",
            {
                "class IdTypeExpSyntax",
                "class MemberTypeExpSyntax",
                "class NullableTypeExpSyntax",
                "class LocalPtrTypeExpSyntax",
                "class BoxPtrTypeExpSyntax",
                "class LocalTypeExpSyntax"
            }
        },

        ClassInfo {
            "IdTypeExpSyntax",
            {
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove*/ true },
                { "std::vector<TypeExpSyntax>", "typeArgs", "GetTypeArgs", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ true,
        },

        // MemberTypeExpSyntax(TypeExpSyntax typeExp, std::string name, std::vector<TypeExpSyntax> typeArgs);
        ClassInfo {
            "MemberTypeExpSyntax",
            {
                { "TypeExpSyntax", "parentType", "GetParentType", /* bUsePimpl */ true, /* bUseMove*/ true },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove*/ true },
                { "std::vector<TypeExpSyntax>", "typeArgs", "GetTypeArgs", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false,
        },

        // NullableTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            "NullableTypeExpSyntax",
            {
                { "TypeExpSyntax", "innerType", "GetInnerType", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false,
        },

        // LocalPtrTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            "LocalPtrTypeExpSyntax",
            {
                { "TypeExpSyntax", "innerType", "GetInnerType", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false,
        },

        // BoxPtrTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            "BoxPtrTypeExpSyntax",
            {
                { "TypeExpSyntax", "innerType", "GetInnerType", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false,
        },

        // LocalTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            "LocalTypeExpSyntax",
            {
                { "TypeExpSyntax", "innerType", "GetInnerType", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false,
        },

        ClassInfo {
            "LambdaExpParamSyntax",        
            {
                { "std::optional<TypeExpSyntax>", "type", "GetType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "bool", "hasOut", "HasOut", /* bUseMove */ false, /* bUseMove */ false },
                { "bool", "hasParams", "HasParams", /* bUseMove */ false, /* bUseMove */ false },
            },
            /* bDefaultsInline */ true
        },

        EnumInfo { 
            "AccessModifierSyntax", 
            { "Public", "Protected", "Private"} 
        },

        EnumInfo { 
            "BinaryOpSyntaxKind",
            {
                "Multiply", "Divide", "Modulo",
                "Add", "Subtract",
                "LessThan", "GreaterThan", "LessThanOrEqual", "GreaterThanOrEqual",
                "Equal", "NotEqual",
                "Assign",
            }
        },

        EnumInfo { "UnaryOpSyntaxKind",
            {
                "PostfixInc", "PostfixDec",
                "Minus", "LogicalNot", "PrefixInc", "PrefixDec",
                "Ref", "Deref", // &, *, local인지 box인지는 분석을 통해서 알아내게 된다
            }
        },

        ClassInfo {
            "ArgumentSyntax",
            {
                { "bool", "bOut", "HasOut", /* bUsePimpl */ false, /* bUseMove */ false},
                { "bool", "bParams", "GetParams", /* bUsePimpl */ false, /* bUseMove */ false},
                { "ExpSyntax", "exp", "GetExp", /* bUsePimpl */ true, /* bUseMove */ true},
            },
            /* bDefaultsInline */ false,
            {
                "SYNTAX_API ArgumentSyntax(ExpSyntax exp);"
            }
        },

        ClassInfo {
            "IdentifierExpSyntax",
            {
                { "std::string", "value", "GetValue", /* bUsePimpl */ false, /* bUseMove*/ true },
                { "std::vector<TypeExpSyntax>", "typeArgs", "GetTypeArgs", false, true },
            },
            /* bDefaultsInline */ true,
        },

        // StringExpSyntaxElement
        VariantInfo {
            "StringExpSyntaxElement",
            "elem",
            {
                "class TextStringExpSyntaxElement",
                "class ExpStringExpSyntaxElement"
            }
        },

        ClassInfo {
            "TextStringExpSyntaxElement",
            { 
                { "std::string", "text", "GetText", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ true,
        },

        ClassInfo {
            "ExpStringExpSyntaxElement",
            {
                { "ExpSyntax", "exp", "GetExp", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false,
        },

        ClassInfo {
            "StringExpSyntax",
            {
                { "std::vector<StringExpSyntaxElement>", "elements", "GetElements", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false,
            /* extraConstructors*/ {
                "SYNTAX_API StringExpSyntax(std::string str);"
            }
        },

        ClassInfo {
            "IntLiteralExpSyntax",
            {
                { "int", "value", "GetValue", /* bUsePimpl */ false, /* bUseMove*/ false },
            },
            /* bDefaultsInline */ true
        },

        ClassInfo {
            "BoolLiteralExpSyntax",
            {
                { "bool", "value", "GetValue", /* bUsePimpl */ false, /* bUseMove*/ false },
            },
            /* bDefaultsInline */ true
        },

        ClassInfo {
            "NullLiteralExpSyntax",
            { },
            /* bDefaultsInline */ true
        },

        ClassInfo {
            "BinaryOpExpSyntax",
            {
                { "BinaryOpSyntaxKind", "kind", "GetKind", /* bUsePimpl */ false, /* bUseMove*/ false },
                { "ExpSyntax", "operand0", "GetOperand0", /* bUsePimpl */ true, /* bUseMove*/ true },
                { "ExpSyntax", "operand1", "GetOperand1", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "UnaryOpExpSyntax",
            {
                { "UnaryOpSyntaxKind", "kind", "GetKind", /* bUsePimpl */ false, /* bUseMove*/ false },
                { "ExpSyntax", "operand", "GetOperand", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "CallExpSyntax",
            {
                { "ExpSyntax", "callable", "GetCallable", /* bUsePimpl */ true, /* bUseMove*/ true },
                { "std::vector<ArgumentSyntax>", "args", "GetArgs", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "LambdaExpSyntax",
            {
                { "std::vector<LambdaExpParamSyntax>", "params", "GetParams", /* bUsePimpl */ false, /* bUseMove*/ true },
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "IndexerExpSyntax",
            {
                { "ExpSyntax", "obj", "GetObject", /* bUsePimpl */ true, /* bUseMove*/ true },
                { "ExpSyntax", "index", "GetIndex", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "MemberExpSyntax",
            {
                { "ExpSyntax", "parent", "GetParent", /* bUsePimpl */ true, /* bUseMove*/ true },
                { "std::string", "memberName", "GetMemberName", /* bUsePimpl */ false, /* bUseMove*/ true },
                { "std::vector<TypeExpSyntax>", "memberTypeArgs", "GetMemberTypeArgs", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false,
            /* extraConstructors */ {
                "SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::string memberName);"
            }
        },

        ClassInfo {
            "ListExpSyntax",
            {
                { "std::vector<ExpSyntax>", "elems", "GetElems", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "NewExpSyntax",
            {
                { "TypeExpSyntax", "type", "GetType", /* bUsePimpl */ false, /* bUseMove*/ true },
                { "std::vector<ArgumentSyntax>", "args", "GetArgs", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "BoxExpSyntax",
            {
                { "ExpSyntax", "innerExp", "GetInnerExp", /* bUsePimpl */ true, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo{
            "IsExpSyntax",
            {
                { "ExpSyntax", "exp", "GetExp", /* bUsePimpl */ true, /* bUseMove*/ true },
                { "TypeExpSyntax", "type", "GetType", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "AsExpSyntax",
            {
                { "ExpSyntax", "exp", "GetExp", /* bUsePimpl */ true, /* bUseMove*/ true },
                { "TypeExpSyntax", "type", "GetType", /* bUsePimpl */ false, /* bUseMove*/ true },
            },
            /* bDefaultsInline */ false
        },
        
        // Embeddable 
        VariantInfo {
            "EmbeddableStmtSyntax",
            "embeddableStmt",
            {
                "class SingleEmbeddableStmtSyntax",
                "class BlockEmbeddableStmtSyntax",
            }
        },
        
        // SingleEmbeddableStmtSyntax(StmtSyntax stmt)
        ClassInfo {
            "SingleEmbeddableStmtSyntax",
            {
                { "StmtSyntax", "stmt", "GetStmt", /* bUsePimpl*/ true, /* bUseMove */ true },
            },
            /* bDefaultsInline */ false
        },

        // BlockEmbeddableStmtSyntax(std::vector<StmtSyntax> stmts)
        ClassInfo {
            "BlockEmbeddableStmtSyntax",
            {
                { "std::vector<StmtSyntax>", "stmts", "GetStmts", /* bUsePimpl*/ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ false
        },

        // VarDeclSyntax
        ClassInfo {
            "VarDeclSyntaxElement",
            {
                { "std::string", "varName", "GetVarName", /* bUsePimpl */ false,  /* bUseMove */ true },
                { "std::optional<ExpSyntax>", "initExp", "GetInitExp", /* bUsePimpl */ true, /* bUseMove */ true}
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "VarDeclSyntax",
            {
                { "TypeExpSyntax", "type", "GetType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<VarDeclSyntaxElement>", "elems", "GetElems", /* bUsePimpl */ false, /* bUseMove */ true}
            },
            /* bDefaultsInline */ true
        },
        
        // Stmt

        // CommandStmtSyntax(std::vector<StringExpSyntax> commands)
        ClassInfo {
            "CommandStmtSyntax",
            {
                { "std::vector<StringExpSyntax>", "commands", "GetCommands", /* bUsePimpl*/ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ false
        },

        // VarDeclStmtSyntax(VarDeclSyntax varDecl)
        ClassInfo {
            "VarDeclStmtSyntax",
            {
                { "VarDeclSyntax", "varDecl", "GetVarDecl", /* bUsePimpl*/ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody)
        ClassInfo {
            "IfStmtSyntax",
            {
                { "ExpSyntax", "cond", "GetCond", /* bUsePimpl*/ true, /* bUseMove */ true },
                { "EmbeddableStmtSyntax", "body", "GetBody", /* bUsePimpl*/ true, /* bUseMove */ true },
                { "std::optional<EmbeddableStmtSyntax>", "elseBody", "GetElseBody", /* bUsePimpl*/ true, /* bUseMove */ true },
            },
            /* bDefaultsInline */ false
        },

        // IfTestStmtSyntax(TypeExpSyntax testTypeExp, std::string varName, ExpSyntax exp, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
        ClassInfo {
            "IfTestStmtSyntax",
            {
                { "TypeExpSyntax", "testType", "GetTestType", /* bUsePimpl*/ false, /* bUseMove */ true },
                { "std::string", "varName", "GetVarName", /* bUsePimpl*/ false, /* bUseMove */ true },
                { "ExpSyntax", "exp", "GetExp", /* bUsePimpl*/ true, /* bUseMove */ true },
                { "EmbeddableStmtSyntax", "body", "GetBody", /* bUsePimpl*/ true, /* bUseMove */ true },
                { "std::optional<EmbeddableStmtSyntax>", "elseBody", "GetElseBody", /* bUsePimpl*/ true, /* bUseMove */ true },
            },
            /* bDefaultsInline */ false
        },

        // 
        VariantInfo {
            "ForStmtInitializerSyntax",
            "forInit",
            {
                "class ExpForStmtInitializerSyntax",
                "class VarDeclForStmtInitializerSyntax"
            }
        },

        ClassInfo {
            "ExpForStmtInitializerSyntax",
            {
                { "ExpSyntax", "exp", "GetExp", /* bUsePimpl*/ true, /* bUseMove */ true }
            },
            /* bDefaultsInline */ false
        },

        ClassInfo {
            "VarDeclForStmtInitializerSyntax",
            {
                { "VarDeclSyntax", "varDecl", "GetVarDecl", /* bUsePimpl*/ true, /* bUseMove */ true }
            },
            /* bDefaultsInline */ false
        },

        // ForStmtSyntax(std::optional<ForStmtInitializerSyntax> initializer, 
        //      std::optional<ExpSyntax> condExp, 
        //      std::optional<ExpSyntax> continueExp, 
        //      EmbeddableStmtSyntax body);
        ClassInfo {
            "ForStmtSyntax",
            {
                { "std::optional<ForStmtInitializerSyntax>", "initializer", "GetInitializer", /* bUsePimpl*/ true, /* bUseMove */ true },
                { "std::optional<ExpSyntax>", "cond", "GetCond", /* bUsePimpl*/ true, /* bUseMove */ true },
                { "std::optional<ExpSyntax>", "cont", "GetCont", /* bUsePimpl*/ true, /* bUseMove */ true },
                { "EmbeddableStmtSyntax", "body", "GetBody", /* bUsePimpl*/ true, /* bUseMove */ true },
            },
            /* bDefaultsInline */ false
        },

        // ContinueStmtSyntax
        ClassInfo {
            "ContinueStmtSyntax",
            {},
            /* bDefaultsInline */ true
        },

        // BreakStmtSyntax
        ClassInfo {
            "BreakStmtSyntax",
            {},
            /* bDefaultsInline */ true
        },
        
        ClassInfo {
            "ReturnStmtSyntax",
            {
                { "std::optional<ExpSyntax>", "value", "GetValue", /* bUsePimpl*/ true, /* bUseMove */ true },
            },
            /* bDefaultsInline */ false
        },

        // BlockStmtSyntax(std::vector<StmtSyntax> stmts)
        ClassInfo {
           "BlockStmtSyntax",
           {
               { "std::vector<StmtSyntax>", "stmts", "GetStmts", /* bUsePimpl*/ false, /* bUseMove */ true },
           },
           /* bDefaultsInline */ false
        },

        // BlankStmtSyntax
        ClassInfo {
            "BlankStmtSyntax",
            {},
            /* bDefaultsInline */ true
        },

        // ExpStmtSyntax(ExpSyntax exp)
        ClassInfo {
            "ExpStmtSyntax",
            {   
                { "ExpSyntax", "exp", "GetExp", /* bUsePimpl*/ true, /* bUseMove */ true},
            },
            /* bDefaultsInline */ false
        },

        // TaskStmtSyntax(std::vector<StmtSyntax> body)
        ClassInfo {
            "TaskStmtSyntax",
            {   
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl*/ false, /* bUseMove */ true},
            },
            /* bDefaultsInline */ false
        },

        // AwaitStmtSyntax(std::vector<StmtSyntax> body);
        ClassInfo {
            "AwaitStmtSyntax",
            {
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl*/ false, /* bUseMove */ true},
            },
            /* bDefaultsInline */ false
        },

        // AsyncStmtSyntax(std::vector<StmtSyntax> body);
        ClassInfo {
            "AsyncStmtSyntax",
            {
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl*/ false, /* bUseMove */ true},
            },
            /* bDefaultsInline */ false
        },

        // ForeachStmtSyntax(TypeExpSyntax type, std::u32string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body);
        ClassInfo {
            "ForeachStmtSyntax",
            {
                { "TypeExpSyntax", "type", "GetType", /* bUsePimpl*/ false, /* bUseMove */ true},
                { "std::string", "varName", "GetVarName", /* bUsePimpl*/ false, /* bUseMove */ true},
                { "ExpSyntax", "enumerable", "GetEnumerable", /* bUsePimpl*/ true, /* bUseMove */ true},
                { "EmbeddableStmtSyntax", "body", "GetBody", /* bUsePimpl*/ true, /* bUseMove */ true},
            },
            /* bDefaultsInline */ false
        },

        // YieldStmtSyntax(ExpSyntax value)
        ClassInfo {
            "YieldStmtSyntax",
            {
                { "ExpSyntax", "value", "GetValue", /* bUsePimpl*/ true, /* bUseMove */ true },
            },
            /* bDefaultsInline */ false
        },

        // DirectiveStmtSyntax(std::u32string name, std::vector<ExpSyntax> args)
        ClassInfo {
            "DirectiveStmtSyntax",
            {
                { "std::string", "name", "GetName", /* bUsePimpl*/ false, /* bUseMove*/ true},
                { "std::vector<ExpSyntax>", "args", "GetArgs", /* bUsePimpl*/ false, /* bUseMove*/ true},
            },

            /* bDefaultsInline */ false
        },

        // TypeParamSyntax
        ClassInfo {
            "TypeParamSyntax",
            {
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // FuncParamSyntax
        ClassInfo {
            "FuncParamSyntax",
            {
                { "bool", "hasOut", "HasOut", /* bUsePimpl */ false, /* bUseMove */ false },
                { "bool", "hasParams", "HasParams", /* bUsePimpl */ false, /* bUseMove */ false },
                { "TypeExpSyntax", "type", "GetType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true }
            },
        },

        // GlobalFuncDeclSyntax
        ClassInfo {
            "GlobalFuncDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "bool", "bSequence", "IsSequence", /* bUsePimpl */ false, /* bUseMove */ false }, // seq 함수인가        
                { "TypeExpSyntax", "retType", "GetRetType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<TypeParamSyntax>", "typeParams", "GetTypeParams", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<FuncParamSyntax>", "parameters", "GetParameters", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // ClassMemberFuncDeclSyntax
        ClassInfo {
            "ClassMemberFuncDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "bool", "bStatic", "IsStatic", /* bUsePimpl */ false, /* bUseMove */ false },
                { "bool", "bSequence", "IsSequence", /* bUsePimpl */ false, /* bUseMove */ false },
                { "TypeExpSyntax", "retType", "GetRetType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<TypeParamSyntax>", "typeParams", "GetTypeParams", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<FuncParamSyntax>", "parameters", "GetParameters", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // ClassConstructorDeclSyntax
        ClassInfo {
            "ClassConstructorDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<FuncParamSyntax>", "parameters", "GetParameters", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::optional<std::vector<ArgumentSyntax>>", "baseArgs", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // ClassMemberVarDeclSyntax
        ClassInfo { 
            "ClassMemberVarDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "TypeExpSyntax", "varType", "GetVarType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<std::string>", "varNames", "GetVarNames", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // ClassMemberDeclSyntax
        VariantInfo {
            "ClassMemberDeclSyntax",
            "decl",
            { 
                "class ClassDeclSyntax",
                "class StructDeclSyntax",
                "class EnumDeclSyntax",
                "ClassMemberFuncDeclSyntax",
                "ClassConstructorDeclSyntax",
                "ClassMemberVarDeclSyntax",
            }
        },

        // ClassDeclSyntax
        ClassInfo {
            "ClassDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<TypeParamSyntax>", "typeParams", "GetTypeParams", /* bUsePimpl */ false, /* bUseMove */ true},
                { "std::vector<TypeExpSyntax>" "baseTypes", "GetBaseTypes", /* bUsePimpl */ false, /* bUseMove */ true},
                { "std::vector<ClassMemberDeclSyntax>", "memberDecls", "GetMemberDecls", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // StructMemberFuncDeclSyntax
        ClassInfo {
            "StructMemberFuncDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAcessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "bool", "bStatic", "IsStatic", /* bUsePimpl */ false, /* bUseMove */ false },
                { "bool", "bSequence", "IsSequence", /* bUsePimpl */ false, /* bUseMove */ false }, // seq 함수인가  
                { "TypeExpSyntax", "retType", "GetRetType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<TypeParamSyntax>", "typeParams", "GetTypeParams", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<FuncParamSyntax>", "parameters", "GetParameters", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // StructConstructorDeclSyntax
        ClassInfo {
            "StructConstructorDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<FuncParamSyntax>", "parameters", "GetParameters", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<StmtSyntax>", "body", "GetBody", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        }

        // StructMemberVarDeclSyntax
        ClassInfo {
            "StructMemberVarDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "TypeExpSyntax", "varType", "GetVarType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<std::string>", "varNames", "GetVarNames", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        }

        // StructMemberDeclSyntax
        VariantInfo {
            "StructMemberDeclSyntax",
            "decl",
            { 
                "class ClassDeclSyntax",
                "class StructDeclSyntax",
                "class EnumDeclSyntax",
                "StructMemberFuncDeclSyntax",
                "StructConstructorDeclSyntax",
                "StructMemberVarDeclSyntax",
            }
        },

        // StructDeclSyntax
        ClassInfo {
            "StructDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<TypeParamSyntax>", "typeParams", "GetTypeParams", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<TypeExpSyntax>", "baseTypes", "GetBaseTypes", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<StructMemberDeclSyntax>", "memberDecls", "GetMemberDecls", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // EnumElemMemberVarDeclSyntax
        ClassInfo {
            "EnumElemMemberVarDeclSyntax",
            {
                { "TypeExpSyntax", "type", "GetType", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // EnumElemDeclSyntax
        ClassInfo {
            "EnumElemDeclSyntax",
            {
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<EnumElemMemberVarDeclSyntax>", "memberVars", "GetMemberVars", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // EnumDeclSyntax
        ClassInfo {
            "EnumDeclSyntax",
            {
                { "std::optional<AccessModifierSyntax>", "accessModifier", "GetAccessModifier", /* bUsePimpl */ false, /* bUseMove */ false },
                { "std::string", "name", "GetName", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<TypeParamSyntax>", "typeParams", "GetTypeParams", /* bUsePimpl */ false, /* bUseMove */ true },
                { "std::vector<EnumElemDeclSyntax>", "elems", "GetElems", /* bUsePimpl */ false, /* bUseMove */ true },
            },
            /* bDefaultsInline */ true
        },

        // NamespaceDeclSyntaxElement
        VariantInfo {
            "NamespaceDeclSyntaxElement",
            "elem",
            {
                "GlobalFuncDeclSyntax",
                "class NamespaceDeclSyntax", // forward declaration
                "ClassDeclSyntax",
                "StructDeclSyntax",
                "EnumDeclSyntax",
            }
        },

        // NamespaceDeclSyntax
        ClassInfo {
            "NamespaceDeclSyntax",
            {
                { "std::vector<std::string>", "names", "GetNames", false, true },
                { "std::vector<NamespaceDeclSyntaxElement>", "elems", "GetElems", false, true }
            },
            /* bDefaultsInline */ false
        },

        // ScriptSyntaxElement
        VariantInfo {
            "ScriptSyntaxElement",
            "elem",
            {
                "NamespaceDeclSyntax",
                "GlobalFuncDeclSyntax",
                "TypeDeclSyntax"
            }
        },

        // Script
        ClassInfo {
            "ScriptSyntax",
            {
                { "std::vector<ScriptSyntaxElement>", "elems", "GetElems", false, true },
            },
            /* bDefaultsInline */ false
        },
    };

    struct
    {
        CommonInfo& commonInfo;
        ostringstream& hStream;
        ostringstream& cppStream;

        void operator()(EnumInfo& enumInfo) { GenerateEnum(commonInfo, enumInfo, hStream); }
        void operator()(StructInfo& structInfo) { GenerateStruct(commonInfo, structInfo, hStream); }
        void operator()(ClassInfo& classInfo) { GenerateClass(commonInfo, classInfo, hStream, cppStream); }
        void operator()(VariantInfo& info) { GenerateVariant(commonInfo, info, hStream, cppStream); }

    } visitor{ commonInfo, hStream, cppStream };

    for (auto& itemInfo : itemInfos)
        std::visit(visitor, itemInfo);

    // footer(close namespaces)
    hStream << endl << '}' << endl;
    cppStream << '}' << endl;

    WriteAll(hPath, hStream.str());
    WriteAll(cppPath, cppStream.str());
}

int wmain(int argc, wchar_t* argv[])
{
    locale::global(locale(".utf8"));

    if (argc < 2)
    {
        wcout << L"Usage: " << argv[0] << ' ' << L"[Source Directory]" << endl;
        wcout << L"   ex: " << argv[0] << ' ' << L"..\\..   (relative to working directory)" << endl;
        return 1;
    }
    
    auto srcPath = absolute(argv[1]);
    wcout << L"Source Directory: " << srcPath << endl;

    // Syntax Generation
    // variant 만들기
    GenerateSyntax(srcPath);
}
