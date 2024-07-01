#include "Misc.h"
#include <fstream>
#include <fmt/core.h>
#include <iostream>

using namespace std;
using namespace std::filesystem;

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
            
            hStream << memberInfo.name << "(std::move(" << memberInfo.name << "))";
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
            hStream << "std::move(" << memberInfo.memberVarName << "))";
            
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

            cppStream << "std::move(" << memberInfo.memberVarName << "))";
            
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

                
                cppStream << "std::move(" << memberInfo.memberVarName << ")";
                
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

void GenerateVariant(CommonInfo& commonInfo, VariantInfo& info, ostringstream& hStream, ostringstream& cppStream)
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

    cppStream << "JsonItem ToJson(" << info.name << "& " << info.argName << ")" << endl;
    cppStream << "{" << endl;
    cppStream << "    return std::visit(ToJsonVisitor(), " << info.argName << ");" << endl;
    cppStream << "}" << endl;
    cppStream << endl;
}

void GenerateItems(CommonInfo& commonInfo, ostringstream& hStream, ostringstream& cppStream, vector<ItemInfo>& itemInfos)
{
    struct
    {
        CommonInfo& commonInfo;
        ostringstream& hStream;
        ostringstream& cppStream;

        void operator()(EnumInfo& enumInfo) { GenerateEnum(commonInfo, enumInfo, hStream); }
        void operator()(StructInfo& structInfo) { GenerateStruct(commonInfo, structInfo, hStream); }
        void operator()(ClassInfo& classInfo) { GenerateClass(commonInfo, classInfo, hStream, cppStream); }
        void operator()(VariantInfo& info) { GenerateVariant(commonInfo, info, hStream, cppStream); }

    } visitor { commonInfo, hStream, cppStream };

    for (auto& itemInfo : itemInfos)
        std::visit(visitor, itemInfo);
}

