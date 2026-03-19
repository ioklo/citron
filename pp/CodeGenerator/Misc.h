#include <string>
#include <vector>
#include <variant>
#include <sstream>
#include <filesystem>

namespace Citron {

struct CommonInfo
{
    std::string linkage;
};

struct ClassMemberInfo
{
    std::string type;
    std::string memberVarName;
    std::string getterName;
};

struct ClassInfo
{
    std::string name;
    std::vector<std::string> virtualBases;      // virtual public SSyntax
    std::vector<std::string> variantInterfaces; // public SStmt
    // void Accept(SStmtVisitor& visitor) override { visitor.Visit(*this); }
    std::vector<ClassMemberInfo> memberInfos;
    std::vector<std::string> extraCtors;

    bool bHasVirtualDestructor;
};

struct EnumInfo
{
    std::string name;
    std::vector<std::string> cases;
};

struct StructMemberInfo
{
    std::string type;
    std::string name;
    bool bUseMove;
};

struct StructInfo
{
    std::string name;
    std::vector<StructMemberInfo> memberInfos;
    std::vector<std::string> extraCtors;
};

struct ForwardClassDeclsInfo
{
    std::vector<std::string> names;
};

struct VariantInterfaceInfo
{
    std::string name;
    std::vector<std::string> virtualBases;
    std::string argName;
    std::vector<std::string> members;
};

struct VariantInfo
{
    std::string name;
    std::string argName;

    std::vector<std::string> memberNames; // with 'class ' 'struct ', ex) class IdentifierExpSyntax
};

struct SharedPtrDeclsInfo
{
    std::vector<std::string> names;
};

using ItemInfo = std::variant<StructInfo, EnumInfo, ClassInfo, VariantInfo, ForwardClassDeclsInfo, VariantInterfaceInfo, SharedPtrDeclsInfo>;
void GenerateItems(CommonInfo& commonInfo, std::ostringstream& ixxStream, std::ostringstream& cppStream, std::vector<ItemInfo>& itemInfos);

void AddNewLineIfNeeded(bool& bModified, std::ostringstream& oss);
std::string ReadAll(std::filesystem::path filePath);
void WriteAll(std::filesystem::path filePath, std::string contents);

} // export