#include "SyntaxCodeGenerator.h"

#include <iostream>
#include <filesystem>

#include "Misc.h"

using namespace std;
using namespace std::filesystem;

namespace Citron {

void GenerateSyntax(path srcPath)
{   
    // [src]/Syntax/Public/Syntax/Syntaxes.g.h
    // [src]/Syntax/Syntaxes.g.ixx
    // [src]/Syntax/Syntaxes.g.cpp
    path hPath = [srcPath]() mutable { return srcPath.append("Syntax").append("Public").append("Syntax").append("Syntaxes.g.h"); }();
    // path ixxPath = [srcPath]() mutable { return srcPath.append("Syntax").append("Syntaxes.g.ixx"); }();
    path cppPath = [srcPath]() mutable { return srcPath.append("Syntax").append("Syntaxes.g.cpp"); }();

    ostringstream hStream, cppStream;

    //  
    hStream << R"---(#pragma once
#include "SyntaxConfig.h"

#include <string>
#include <vector>
#include <optional>
#include <memory>
#include <variant>

#include "Infra/Json.h"
#include "Infra/Unreachable.h"

namespace Citron {
class SFactory;

)---";

    cppStream << R"---(#include "Syntaxes.g.h"

#include "Infra/Json.h"

using namespace std;

namespace Citron {

namespace {
struct ToJsonVisitor {
    template<typename T>
    JsonItem operator()(T* t) { return t->ToJson(); }

    template<typename T>
    JsonItem operator()(T& t) { return t.ToJson(); }
};
}

)---";

    CommonInfo commonInfo = { .linkage = "SYNTAX_API" };

    vector<ItemInfo> itemInfos {

        // Stmt
        ForwardClassDeclsInfo {
            .names {
                "SStmt",
                "SStmt_Command",
                "SStmt_VarDecl",
                "SStmt_If",
                "SStmt_IfTest",
                "SStmt_For",
                "SStmt_Continue",
                "SStmt_Break",
                "SStmt_Return",
                "SStmt_Block",
                "SStmt_Blank",
                "SStmt_Exp",
                "SStmt_Task",
                "SStmt_Await",
                "SStmt_Async",
                "SStmt_Foreach",
                "SStmt_Yield",
                "SStmt_Directive",
            }
        },

        // SExp
        ForwardClassDeclsInfo {
            .names {
                "SExp",
                "SExp_Identifier",
                "SExp_String",
                "SExp_IntLiteral",
                "SExp_BoolLiteral",
                "SExp_NullLiteral",
                "SExp_BinaryOp",
                "SExp_UnaryOp",
                "SExp_Call",
                "SExp_Lambda",
                "SExp_Indexer",
                "SExp_Member",
                "SExp_IndirectMember",
                "SExp_List",
                "SExp_New",
                "SExp_Box",
                "SExp_Is",
                "SExp_As",
            }
        },

        // TypeExp
        ForwardClassDeclsInfo {
            .names {
                "STypeExp",
                "STypeExp_Id",
                "STypeExp_Member",
                "STypeExp_Nullable",
                "STypeExp_LocalPtr",
                "STypeExp_Local"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SStringExpElement",
                "SStringExpElement_Text",
                "SStringExpElement_Exp"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SLambdaExpBody",
                "SLambdaExpBody_Stmts",
                "SLambdaExpBody_Exp"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SEmbeddableStmt",
                "SEmbeddableStmt_Single",
                "SEmbeddableStmt_Block"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SForStmtInitializer",
                "SForStmtInitializer_Exp",
                "SForStmtInitializer_VarDecl"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SClassMemberDecl",
                "SClassFuncDecl",
                "SClassCtorDecl",
                "SClassVarDecl",
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SStructMemberDecl",
                "SStructFuncDecl",
                "SStructCtorDecl",
                "SStructVarDecl",
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SNamespaceDeclElement",
                "SScriptElement",
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SClassDecl",
                "SStructDecl",
                "SEnumDecl",
                "SGlobalFuncDecl",
                "SNamespaceDecl",
                "SScript",
                "SArgument",
                "SArguments"
            }
        },

        // 여기에 신규 ForwardClassDeclInfo 추가
        EnumInfo {
            .name = "SAccessModifier",
            .cases = { "Public", "Protected", "Private"}
        },

        EnumInfo {
            .name = "SBinaryOpKind",
            .cases {
                "Multiply", "Divide", "Modulo",
                "Add", "Subtract",
                "LessThan", "GreaterThan", "LessThanOrEqual", "GreaterThanOrEqual",
                "Equal", "NotEqual",
                "Assign",
            }
        },

        EnumInfo {
            .name = "SUnaryOpKind",
            .cases {
                "PostfixInc", "PostfixDec",
                "Minus", "LogicalNot", "PrefixInc", "PrefixDec",
                "Ref", "Deref", // &, *, local인지 box인지는 분석을 통해서 알아내게 된다
            }
        },

        ClassInfo {
            .name = "SSyntax",
            .bHasVirtualDestructor = true
        },

        ClassInfo {
            .name = "SArgument",
            .memberInfos {
                {.type = "bool", .memberVarName = "bOut", .getterName = "HasOut" },
                {.type = "bool", .memberVarName = "bParams", .getterName = "GetParams" },
                {.type = "SExp*", .memberVarName = "exp", .getterName = "GetExp" },
            },
            .extraCtors {
                "SYNTAX_API SArgument(SExp* exp);"
            }
        },

        ClassInfo {
            .name = "SArguments",
            .memberInfos {
                { .type = "std::vector<SArgument*>", .memberVarName = "items", .getterName = "GetItems" },
            }
        },

        ClassInfo {
            .name = "SLambdaExpParam",
            .memberInfos {
                {.type = "STypeExp*", .memberVarName = "type", .getterName = "GetType" },
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "bool", .memberVarName = "hasOut", .getterName = "HasOut" },
                {.type = "bool", .memberVarName = "hasParams", .getterName = "HasParams" },
            },
        },

        // SVarDecl
            ClassInfo {
                .name = "SVarDeclElement",
                .memberInfos {
                    {.type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                    {.type = "SExp*", .memberVarName = "initExp", .getterName = "GetInitExp" }
                },
        },

            ClassInfo {
                .name = "SVarDecl",
                .memberInfos {
                    {.type = "STypeExp*", .memberVarName = "type", .getterName = "GetType" },
                    {.type = "std::vector<SVarDeclElement>", .memberVarName = "elements", .getterName = "GetElements" }
                },
        },

        // STypeParam
        ClassInfo {
            .name = "STypeParam",
            .memberInfos {
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
            },
        },

        // SFuncParam
        ClassInfo {
            .name = "SFuncParam",
            .memberInfos {
                {.type = "bool", .memberVarName = "hasOut", .getterName = "HasOut" },
                {.type = "bool", .memberVarName = "hasParams", .getterName = "HasParams" },
                {.type = "STypeExp*", .memberVarName = "type", .getterName = "GetType" },
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" }
            },
        },


        // Variants 
        #pragma region VariantInterfaceInfo
        VariantInterfaceInfo {
            .name = "SStmt",
            .virtualBases { "SSyntax" },
            .argName = "stmt",
            .members {
                "SStmt_Command",
                "SStmt_VarDecl",
                "SStmt_If",
                "SStmt_IfTest",
                "SStmt_For",
                "SStmt_Continue",
                "SStmt_Break",
                "SStmt_Return",
                "SStmt_Block",
                "SStmt_Blank",
                "SStmt_Exp",
                "SStmt_Task",
                "SStmt_Await",
                "SStmt_Async",
                "SStmt_Foreach",
                "SStmt_Yield",
                "SStmt_Directive"
            }
        },

        VariantInterfaceInfo {
            .name = "SExp",
            .virtualBases { "SSyntax" },
            .argName = "exp",
            .members {
                "SExp_Identifier",
                "SExp_String",
                "SExp_IntLiteral",
                "SExp_BoolLiteral",
                "SExp_NullLiteral",
                "SExp_BinaryOp",
                "SExp_UnaryOp",
                "SExp_Call",
                "SExp_Lambda",
                "SExp_Indexer",
                "SExp_Member",
                "SExp_IndirectMember",
                "SExp_List",
                "SExp_New",
                "SExp_Box",
                "SExp_Is",
                "SExp_As",
            }
        },

        VariantInterfaceInfo {
            .name = "STypeExp",
            .virtualBases { "SSyntax" },
            .argName = "typeExp",
            .members {
                "STypeExp_Id",
                "STypeExp_Member",
                "STypeExp_Nullable",
                "STypeExp_LocalPtr",
                "STypeExp_Local"
            }
        },

        VariantInterfaceInfo {
            .name = "SStringExpElement",
            .virtualBases { "SSyntax" },
            .argName = "elem",
            .members {
                "SStringExpElement_Text",
                "SStringExpElement_Exp"
            }
        },

        VariantInterfaceInfo {
            .name = "SLambdaExpBody",
            .virtualBases { "SSyntax" },
            .argName = "body",
            .members {
                "SLambdaExpBody_Stmts",
                "SLambdaExpBody_Exp"
            }
        },

        VariantInterfaceInfo {
            .name = "SEmbeddableStmt",
            .virtualBases { "SSyntax" },
            .argName = "stmt",
            .members {
                "SEmbeddableStmt_Single",
                "SEmbeddableStmt_Block"
            }
        },

        VariantInterfaceInfo {
            .name = "SForStmtInitializer",
            .virtualBases { "SSyntax" },
            .argName = "initializer",
            .members {
                "SForStmtInitializer_Exp",
                "SForStmtInitializer_VarDecl"
            }
        },

        // SClassMemberDecl
        VariantInterfaceInfo {
            .name = "SClassMemberDecl",
            .virtualBases { "SSyntax" },
            .argName = "decl",
            .members {
                "SClassDecl",
                "SStructDecl",
                "SEnumDecl",
                "SClassFuncDecl",
                "SClassCtorDecl",
                "SClassVarDecl",
            }
        },

        // SStructMemberDecl
        VariantInterfaceInfo {
            .name = "SStructMemberDecl",
            .virtualBases { "SSyntax" },
            .argName = "decl",
            .members {
                "SClassDecl",
                "SStructDecl",
                "SEnumDecl",
                "SStructFuncDecl",
                "SStructCtorDecl",
                "SStructVarDecl",
            }
        },

        // SNamespaceDeclElement
        VariantInterfaceInfo {
            .name = "SNamespaceDeclElement",
            .virtualBases { "SSyntax" },
            .argName = "elem",
            .members {
                "SGlobalFuncDecl",
                "SNamespaceDecl",
                "SClassDecl",
                "SStructDecl",
                "SEnumDecl",
            }
        },

        // SScriptElement
        VariantInterfaceInfo {
            .name = "SScriptElement",
            .virtualBases { "SSyntax" },
            .argName = "elem",
            .members {
                "SNamespaceDecl",
                "SGlobalFuncDecl",
                "SClassDecl",
                "SStructDecl",
                "SEnumDecl",
            }
        },

        // 여기에 신규 VariantInterfaceInfo 추가
        #pragma endregion VariantInterfaceInfo

        #pragma region SExps
        ClassInfo {
            .name = "SExp_Identifier",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "std::string", .memberVarName = "value", .getterName = "GetValue" },
                {.type = "std::vector<STypeExp*>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
            .extraCtors {
                "SExp_Identifier(std::string value) : SExp_Identifier(move(value), {}) { }"
            }
        },

        ClassInfo {
            .name = "SExp_String",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "std::vector<SStringExpElement*>", .memberVarName = "elements", .getterName = "GetElements" },
            },
            .extraCtors {
                "SYNTAX_API SExp_String(std::string&& str, SFactory& factory);"
            }
        },

        ClassInfo {
            .name = "SExp_IntLiteral",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "int", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        ClassInfo {
            .name = "SExp_BoolLiteral",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "bool", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        ClassInfo {
            .name = "SExp_NullLiteral",
            .variantInterfaces { "SExp" },
            .memberInfos { },
        },

        ClassInfo {
            .name = "SExp_List",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "std::vector<SExp*>", .memberVarName = "elements", .getterName = "GetElements" },
            },
        },

        ClassInfo {
            .name = "SExp_New",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "STypeExp*", .memberVarName = "type", .getterName = "GetType" },
                {.type = "SArguments*", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        ClassInfo {
            .name = "SExp_BinaryOp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SBinaryOpKind", .memberVarName = "kind", .getterName = "GetKind" },
                {.type = "SExp*", .memberVarName = "operand0", .getterName = "GetOperand0" },
                {.type = "SExp*", .memberVarName = "operand1", .getterName = "GetOperand1" },
            },
        },

        ClassInfo {
            .name = "SExp_UnaryOp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SUnaryOpKind", .memberVarName = "kind", .getterName = "GetKind" },
                {.type = "SExp*", .memberVarName = "operand", .getterName = "GetOperand" },
            },
        },

        ClassInfo {
            .name = "SExp_Call",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "callable", .getterName = "GetCallable" },
                {.type = "SArguments*", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        ClassInfo {
            .name = "SExp_Lambda",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "std::vector<SLambdaExpParam>", .memberVarName = "params", .getterName = "GetParams" },
                { .type = "SLambdaExpBody*", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        ClassInfo {
            .name = "SExp_Indexer",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "obj", .getterName = "GetObject" },
                {.type = "SExp*", .memberVarName = "index", .getterName = "GetIndex" },
            },
        },

        ClassInfo {
            .name = "SExp_Member",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "parent", .getterName = "GetParent" },
                {.type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName" },
                {.type = "std::vector<STypeExp*>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs" },
            },
            .extraCtors {
                "SYNTAX_API SExp_Member(SExp* parent, std::string&& memberName);"
            }
        },

        ClassInfo {
            .name = "SExp_IndirectMember",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "parent", .getterName = "GetParent" },
                {.type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName" },
                {.type = "std::vector<STypeExp*>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs" },
            },
            .extraCtors {
                "SYNTAX_API SExp_IndirectMember(SExp* parent, std::string&& memberName);"
            }
        },

        ClassInfo {
            .name = "SExp_Box",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "innerExp", .getterName = "GetInnerExp" },
            },
        },

        ClassInfo {
            .name = "SExp_Is",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "exp", .getterName = "GetExp" },
                {.type = "STypeExp*", .memberVarName = "type", .getterName = "GetType" },
            },
        },

        ClassInfo {
            .name = "SExp_As",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "exp", .getterName = "GetExp" },
                {.type = "STypeExp*", .memberVarName = "type", .getterName = "GetType" },
            },
        },

        #pragma endregion SExps

        #pragma region TypeExps

        
        ClassInfo {
            .name = "STypeExp_Id",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "std::vector<STypeExp*>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
            .extraCtors {
                "SYNTAX_API STypeExp_Id(std::string&& name);"
            }
        },

        // STypeExp_Member(STypeExp typeExp, std::string name, std::vector<STypeExp*> typeArgs);
        ClassInfo {
            .name = "STypeExp_Member",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExp*", .memberVarName = "parentType", .getterName = "GetParentType" },
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "std::vector<STypeExp*>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
        },

        // STypeExp_Nullable(STypeExp typeExp)
        ClassInfo {
            .name = "STypeExp_Nullable",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExp*", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // STypeExp_LocalPtr(STypeExp typeExp)
        ClassInfo {
            .name = "STypeExp_LocalPtr",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExp*", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // STypeExp_Local(STypeExp typeExp)
        ClassInfo {
            .name = "STypeExp_Local",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExp*", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        #pragma endregion STypeExps

        #pragma region SStringExpElements

        ClassInfo {
            .name = "SStringExpElement_Text",
            .variantInterfaces { "SStringExpElement" },
            .memberInfos {
                {.type = "std::string", .memberVarName = "text", .getterName = "GetText" },
            },
        },

        ClassInfo {
            .name = "SStringExpElement_Exp",
            .variantInterfaces { "SStringExpElement" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "exp", .getterName = "GetExp" },
            },
        },
        #pragma endregion SStringExpElements

        #pragma region SLambdaExpBodys
        
        ClassInfo {
            .name = "SLambdaExpBody_Stmts",
            .variantInterfaces { "SLambdaExpBody" },
            .memberInfos {
                { .type = "std::vector<SStmt*>", .memberVarName = "stmts", .getterName = "GetStmts" }
            },
        },

        ClassInfo {
            .name = "SLambdaExpBody_Exp",
            .variantInterfaces { "SLambdaExpBody" },
            .memberInfos = {
                { .type = "SExp*", .memberVarName = "exp", .getterName = "GetExp" }
            },
        },
        #pragma endregion SLambdaExpBodys

        #pragma region SEmbeddableStmts

        // SEmbeddableStmt_Single(SStmt* stmt), SStmt에 depends
        ClassInfo {
            .name = "SEmbeddableStmt_Single",
            .variantInterfaces { "SEmbeddableStmt" },
            .memberInfos {
                { .type = "SStmt*", .memberVarName = "stmt", .getterName = "GetStmt" },
            },
        },

        // SEmbeddableStmt_Block(std::vector<SStmt*> stmts)
        ClassInfo {
            .name = "SEmbeddableStmt_Block",
            .variantInterfaces { "SEmbeddableStmt" },
            .memberInfos {
                {.type = "std::vector<SStmt*>", .memberVarName = "stmts", .getterName = "GetStmts" },
            },
        },

        #pragma endregion SEmbeddableStmts

        #pragma region SForStmtInitializer
        
        ClassInfo {
            .name = "SForStmtInitializer_Exp",
            .variantInterfaces { "SForStmtInitializer" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "exp", .getterName = "GetExp" }
            },
        },

        ClassInfo {
            .name = "SForStmtInitializer_VarDecl",
            .variantInterfaces { "SForStmtInitializer" },
            .memberInfos {
                {.type = "SVarDecl", .memberVarName = "varDecl", .getterName = "GetVarDecl" }
            },
        },

        #pragma endregion SForStmtInitializer

        #pragma region SStmts

        // SStmt_Command(std::vector<SExp_String> commands)
        ClassInfo {
            .name = "SStmt_Command",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SExp_String*>", .memberVarName = "commands", .getterName = "GetCommands" },
            },
        },

        // SStmt_VarDecl(SVarDecl varDecl)
        ClassInfo {
            .name = "SStmt_VarDecl",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SVarDecl", .memberVarName = "varDecl", .getterName = "GetVarDecl" },
            },
        },

        // SStmt_Continue
        ClassInfo {
            .name = "SStmt_Continue",
            .variantInterfaces { "SStmt" },
            .memberInfos {},
        },

        // SStmt_Break
        ClassInfo {
            .name = "SStmt_Break",
            .variantInterfaces { "SStmt" },
            .memberInfos {},
        },

        // SStmt_Block(std::vector<SStmt*> stmts)
        ClassInfo {
            .name = "SStmt_Block",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmt*>", .memberVarName = "stmts", .getterName = "GetStmts" },
            },
        },

        // SStmt_Blank
            ClassInfo {
                .name = "SStmt_Blank",
                .variantInterfaces { "SStmt" },
                .memberInfos {},
        },

        // SStmt_Task(std::vector<SStmt*> body)
        ClassInfo {
            .name = "SStmt_Task",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmt*>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStmt_Await(std::vector<SStmt*> body);
        ClassInfo {
            .name = "SStmt_Await",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmt*>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStmt_Async(std::vector<SStmt*> body);
            ClassInfo {
                .name = "SStmt_Async",
                .variantInterfaces { "SStmt" },
                .memberInfos {
                    {.type = "std::vector<SStmt*>", .memberVarName = "body", .getterName = "GetBody" },
                },
        },

        // SStmt_Directive(std::u32string name, std::vector<SExp*> args)
        ClassInfo {
            .name = "SStmt_Directive",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<SExp*>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        // SStmt_If(SExp cond, SEmbeddableStmt body, SEmbeddableStmt* elseBody)
        ClassInfo {
            .name = "SStmt_If",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                { .type = "SExp*", .memberVarName = "cond", .getterName = "GetCond" },
                { .type = "SEmbeddableStmt*", .memberVarName = "body", .getterName = "GetBody" },
                { .type = "SEmbeddableStmt*", .memberVarName = "elseBody", .getterName = "GetElseBody" },
            },
        },

        // SStmt_IfTest(STypeExp testTypeExp, std::string varName, SExp exp, SEmbeddableStmt* body, SEmbeddableStmt* elseBody);
            ClassInfo {
                .name = "SStmt_IfTest",
                .variantInterfaces { "SStmt" },
                .memberInfos {
                    {.type = "STypeExp*", .memberVarName = "testType", .getterName = "GetTestType" },
                    {.type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                    {.type = "SExp*", .memberVarName = "exp", .getterName = "GetExp" },
                    {.type = "SEmbeddableStmt*", .memberVarName = "body", .getterName = "GetBody" },
                    {.type = "SEmbeddableStmt*", .memberVarName = "elseBody", .getterName = "GetElseBody" },
                },
        },

        // SStmt_For(SForStmtInitializer* initializer, 
        //      SExp* condExp, 
        //      SExp* continueExp, 
        //      SEmbeddableStmt body);
        ClassInfo {
            .name = "SStmt_For",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SForStmtInitializer*", .memberVarName = "initializer", .getterName = "GetInitializer" },
                {.type = "SExp*", .memberVarName = "cond", .getterName = "GetCond" },
                {.type = "SExp*", .memberVarName = "cont", .getterName = "GetCont" },
                {.type = "SEmbeddableStmt*", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        ClassInfo {
            .name = "SStmt_Return",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        // SStmt_Exp(SExp exp)
        ClassInfo {
            .name = "SStmt_Exp",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "exp", .getterName = "GetExp" },
            },
        },

        // SStmt_Foreach(STypeExp type, std::u32string varName, SExp enumerable, SEmbeddableStmt body);
        ClassInfo {
            .name = "SStmt_Foreach",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "STypeExp*", .memberVarName = "type", .getterName = "GetType" },
                {.type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                {.type = "SExp*", .memberVarName = "enumerable", .getterName = "GetEnumerable" },
                {.type = "SEmbeddableStmt*", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStmt_Yield(SExp value)
        ClassInfo {
            .name = "SStmt_Yield",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SExp*", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        #pragma endregion SStmts
        
        // SGlobalFuncDecl
        ClassInfo {
            .name = "SGlobalFuncDecl",
            .variantInterfaces { "SNamespaceDeclElement", "SScriptElement" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence" }, // seq 함수인가        
                { .type = "STypeExp*", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<SStmt*>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        #pragma region SClassDecls
        // SClassDecl
        ClassInfo {
            .name = "SClassDecl",
            .variantInterfaces { "SClassMemberDecl", "SStructMemberDecl", "SNamespaceDeclElement", "SScriptElement" },
            .memberInfos {
                {.type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                {.type = "std::vector<STypeExp*>", .memberVarName = "baseTypes", .getterName = "GetBaseTypes" },
                {.type = "std::vector<SClassMemberDecl*>", .memberVarName = "memberDecls", .getterName = "GetMemberDecls" },
            },
        },

        // SClassFuncDecl
        ClassInfo {
            .name = "SClassFuncDecl",
            .variantInterfaces { "SClassMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "bool", .memberVarName = "bStatic", .getterName = "IsStatic" },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence" },
                { .type = "STypeExp*", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<SStmt*>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SClassCtorDecl
        ClassInfo {
            .name = "SClassCtorDecl",
            .variantInterfaces { "SClassMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "SArguments*", .memberVarName = "baseArgs", .getterName = "GetBaseArgs" },
                { .type = "std::vector<SStmt*>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SClassVarDecl
        ClassInfo {
            .name = "SClassVarDecl",
            .variantInterfaces { "SClassMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "STypeExp*", .memberVarName = "varType", .getterName = "GetVarType" },
                { .type = "std::vector<std::string>", .memberVarName = "varNames", .getterName = "GetVarNames" },
            },
        },

        #pragma endregion ClassDecls

        #pragma region StructDecls

        // SStructDecl
        ClassInfo {
            .name = "SStructDecl",
            .variantInterfaces { "SClassMemberDecl", "SStructMemberDecl", "SNamespaceDeclElement", "SScriptElement" },
            .memberInfos {
                {.type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                {.type = "std::vector<STypeExp*>", .memberVarName = "baseTypes", .getterName = "GetBaseTypes" },
                {.type = "std::vector<SStructMemberDecl*>", .memberVarName = "memberDecls", .getterName = "GetMemberDecls" },
            },
        },

        // SStructFuncDecl
        ClassInfo {
            .name = "SStructFuncDecl",
            .variantInterfaces { "SStructMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAcessModifier" },
                { .type = "bool", .memberVarName = "bStatic", .getterName = "IsStatic" },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence" }, // seq 함수인가  
                { .type = "STypeExp*", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<SStmt*>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStructCtorDecl
        ClassInfo {
            .name = "SStructCtorDecl",
            .variantInterfaces { "SStructMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<SStmt*>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStructVarDecl
        ClassInfo {
            .name = "SStructVarDecl",
            .variantInterfaces { "SStructMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "STypeExp*", .memberVarName = "varType", .getterName = "GetVarType" },
                { .type = "std::vector<std::string>", .memberVarName = "varNames", .getterName = "GetVarNames" },
            },
        },
        
        #pragma endregion SStructDecls

        #pragma region SEnumDecl

        // SEnumElemVarDecl
        ClassInfo {
            .name = "SEnumElemVarDecl",
            .virtualBases { "SSyntax" },
            .memberInfos {
                { .type = "STypeExp*", .memberVarName = "type", .getterName = "GetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
            },
        },

        // SEnumElemDecl
        ClassInfo {
            .name = "SEnumElemDecl",
            .virtualBases { "SSyntax" },
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<SEnumElemVarDecl*>", .memberVarName = "vars", .getterName = "GetVars" },
            },
        },

        // SEnumDecl
        ClassInfo {
            .name = "SEnumDecl",
            .variantInterfaces { "SClassMemberDecl", "SStructMemberDecl", "SNamespaceDeclElement", "SScriptElement" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<SEnumElemDecl*>", .memberVarName = "elements", .getterName = "GetElements" },
            },
        },

        #pragma endregion SEnumDecl
        
        // SNamespaceDecl
        ClassInfo {
            .name = "SNamespaceDecl",
            .variantInterfaces { "SNamespaceDeclElement", "SScriptElement" },
            .memberInfos {
                { .type = "std::vector<std::string>", .memberVarName = "names", .getterName = "GetNames" },
                { .type = "std::vector<SNamespaceDeclElement*>", .memberVarName = "elements", .getterName = "GetElements" }
            },
        },
        
        // Script
        ClassInfo {
            .name = "SScript",
            .virtualBases { "SSyntax" },
            .memberInfos {
                { .type = "std::vector<SScriptElement*>", .memberVarName = "elements", .getterName = "GetElements" },
            },
        },
    };

    GenerateItems(commonInfo, hStream, cppStream, itemInfos);

    
    

    // footer(close namespaces)
    hStream << endl << '}' << endl;
    cppStream << '}' << endl;

    WriteAll(hPath, hStream.str());
    WriteAll(cppPath, cppStream.str());
}

} // namespace Citron