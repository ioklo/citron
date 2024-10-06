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
    bool bFirst = true;
    for(auto& variantInterface : classInfo.variantInterfaces)
    {
        if (bFirst)
        {
            hStream << "    : public " << variantInterface << endl;
            bFirst = false;
        }
        else
        {
            hStream << "    , public " << variantInterface << endl;
        }
    }
    hStream << "{" << endl;

    hStream << "public:" << endl;

    bool bHModified = false, bCppModified = false;
    for (auto& memberInfo : classInfo.memberInfos)
    {
        hStream << fmt::format(R"---(    {} {};)---", memberInfo.type, memberInfo.memberVarName) << endl;
        bHModified = true;
    }

    AddNewLineIfNeeded(bHModified, hStream);
    // SYNTAX_API IdentifierExpSyntax(std::string value);
    hStream << "    " << commonInfo.linkage << ' ' << classInfo.name << "(";

    bFirst = true;
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

    if (!classInfo.memberInfos.empty())
    {
        cppStream << "    : ";

        // non pimpl
        bFirst = true;
        for (auto& memberInfo : classInfo.memberInfos)
        {
            if (bFirst)
            {
                bFirst = false;
            }
            else
                cppStream << ", ";

            cppStream << memberInfo.memberVarName << '(';

            cppStream << "std::move(" << memberInfo.memberVarName << "))";

        }
        cppStream << " { }" << endl;
        bCppModified = true;
    }
    else
    {
        cppStream << "{ }" << endl;
    }

    // 추가 생성자
    for (auto& extraConstructor : classInfo.extraConstructors)
    {
        hStream << "    " << extraConstructor << endl;
    }

    bHModified |= !classInfo.extraConstructors.empty();

    // copy constructor, move constructor

    // IdentifierExpSyntax(const IdentifierExpSyntax&) = delete;
    // SYNTAX_API IdentifierExpSyntax(IdentifierExpSyntax&& other);
    hStream << "    " << classInfo.name << "(const " << classInfo.name << "&) = delete;" << endl;
    hStream << "    " << commonInfo.linkage << " " << classInfo.name << "(" << classInfo.name << "&&) noexcept;" << endl;
    bHModified = true;

    // IdentifierExpSyntax::IdentifierExpSyntax(IdentifierExpSyntax&& other)
    AddNewLineIfNeeded(bCppModified, cppStream);
    cppStream << classInfo.name << "::" << classInfo.name << "(" << classInfo.name << "&& other) noexcept = default;" << endl;
    bCppModified = true;

    // 소멸자, inline이 아닐때만 생성한다
    // SYNTAX_API ~IdentifierExpSyntax();
    // SYNTAX_API ~IdentifierExpSyntax();
    if (classInfo.variantInterfaces.empty())
        hStream << "    " << commonInfo.linkage << " ~" << classInfo.name << "();" << endl;
    else
        hStream << "    " << commonInfo.linkage << " virtual ~" << classInfo.name << "();" << endl;
    bHModified = true;

    // IdentifierExpSyntax::~IdentifierExpSyntax() = default;
    AddNewLineIfNeeded(bCppModified, cppStream);
    cppStream << classInfo.name << "::~" << classInfo.name << "() = default;" << endl;
    bCppModified = true;

    // copy assignment, move assignment

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

    AddNewLineIfNeeded(bHModified, hStream);
    AddNewLineIfNeeded(bCppModified, cppStream);

    //// Getter
    //for (auto& memberInfo : classInfo.memberInfos)
    //{
    //    //    std::vector<TypeExpSyntax>& GetTypeArgs() { return typeArgs; }
    //    hStream << "    " << memberInfo.type << "& " << memberInfo.getterName << "() { return " << memberInfo.memberVarName << "; }" << endl;
    //    bHModified = true;
    //}

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
        cppStream << "        { \"" << memberInfo.memberVarName << "\", Citron::ToJson(" << memberInfo.memberVarName << ") }," << endl;
    }

    cppStream << "    };" << endl;
    cppStream << "}" << endl << endl;
    bCppModified = true;

    AddNewLineIfNeeded(bHModified, hStream);

    for (auto& variantInterface : classInfo.variantInterfaces)
    {
        hStream << "    void Accept(" << variantInterface << "Visitor& visitor) override { visitor.Visit(*this); }" << endl;
        bHModified = true;
    }
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

void GenerateForwardClassDecls(CommonInfo& commonInfo, ForwardClassDeclsInfo& info, ostringstream& hStream)
{
    for(auto& name : info.names)
    {
        hStream << "class " << name << ";" << endl;
    }

    if (!info.names.empty())
        hStream << endl;
}

void GenerateVariantInterface(CommonInfo& commonInfo, VariantInterfaceInfo& info, ostringstream& hStream, ostringstream& cppStream)
{
    // class 'name'Visitor 
    // {
    // public:
    //     virtual void Visit(A& a) = 0;
    //     virtual void Visit(B& b) = 0;
    //     virtual void Visit(C& c) = 0;
    // };
    // 
    // class 'name' : 'bases...'
    // {
    // public:
    //     virtual ~'name'() { }
    //     virtual void Accept('name'Visitor& visitor) = 0;
    // };

    hStream << "class " << info.name << "Visitor" << endl;
    hStream << "{" << endl;
    hStream << "public:" << endl;
    hStream << "    virtual ~" << info.name << "Visitor() { }" << endl;
    for(auto& member : info.members)
        hStream << "    virtual void Visit(" << member << "& " << info.argName << ") = 0;" << endl;
    hStream << "};" << endl << endl;

    hStream << "class " << info.name;

    if (!info.bases.empty())
    {
        bool bFirst = true;
        for (auto& base : info.bases)
        {
            if (bFirst)
                hStream << " : public " << base;
            else
                hStream << ", public " << base;
        }
    }

    hStream << endl;

    hStream << "{" << endl;
    hStream << "public:" << endl;
    hStream << "    " << info.name << "() = default;" << endl;
    hStream << "    " << info.name << "(const " << info.name << "&) = delete;" << endl;
    hStream << "    " << info.name << "(" << info.name << "&&) = default;" << endl;
    hStream << "    virtual ~" << info.name << "() { }" << endl;
    hStream << "    " << info.name << "& operator=(const " << info.name << "& other) = delete;" << endl;
    hStream << "    " << info.name << "& operator=(" << info.name << "&& other) noexcept = default;" << endl;
    hStream << "    virtual void Accept(" << info.name << "Visitor& visitor) = 0;" << endl;
    hStream << "};" << endl << endl;

    // SYNTAX_API JsonItem ToJson('name'Ptr& 'argName');
    hStream << commonInfo.linkage << " JsonItem ToJson(" << info.name << "Ptr& " << info.argName << ");" << endl << endl;

    // struct 'name'ToJsonVisitor
    // {
    //     JsonItem result;
    //     void Visit(A& a) override { result = a.ToJson(); }
    //     void Visit(B& a) override { result = a.ToJson(); }
    // }
    // 
    // JsonItem ToJson('name'Ptr& 'argName')
    // {
    //     'name'ToJsonVisitor visitor;
    //     'argName'->Accept(visitor);
    //     return visitor.result;
    // }
    cppStream << "struct " << info.name << "ToJsonVisitor : " << "public " << info.name << "Visitor" << endl;
    cppStream << "{" << endl;
    cppStream << "    JsonItem result;" << endl;
    for (auto& member : info.members)
        cppStream << "    void Visit(" << member << "& " << info.argName << ") override { result = " << info.argName << ".ToJson(); }" << endl;
    cppStream << "};" << endl << endl;

    cppStream << "JsonItem ToJson(" << info.name << "Ptr& " << info.argName << ")" << endl;
    cppStream << "{" << endl;
    cppStream << "    if (!" << info.argName << ") return JsonNull();" << endl << endl;
    cppStream << "    " << info.name << "ToJsonVisitor visitor;" << endl;
    cppStream << "    " << info.argName << "->Accept(visitor);" << endl;
    cppStream << "    return visitor.result;" << endl;
    cppStream << "}" << endl;
}

void GenerateSharedPtrDecls(CommonInfo& commonInfo, SharedPtrDeclsInfo& info, ostringstream& hStream)
{
    // using 'name'Ptr = std::shared_ptr<'name'>;
    for (auto& name : info.names)
        hStream << "using " << name << "Ptr = std::shared_ptr<" << name << ">;" << endl;
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
        void operator()(ForwardClassDeclsInfo& info) { GenerateForwardClassDecls(commonInfo, info, hStream); }
        void operator()(VariantInterfaceInfo& info) { GenerateVariantInterface(commonInfo, info, hStream, cppStream); }
        void operator()(SharedPtrDeclsInfo& info) { GenerateSharedPtrDecls(commonInfo, info, hStream); }

    } visitor { commonInfo, hStream, cppStream };

    for (auto& itemInfo : itemInfos)
        std::visit(visitor, itemInfo);
}

