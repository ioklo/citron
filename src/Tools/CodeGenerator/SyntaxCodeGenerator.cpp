#include "Misc.h"
#include <iostream>
#include <fmt/core.h>

using namespace std;
using namespace std::filesystem;

void GenerateSyntax(path srcPath)
{   
    // [src]/Syntax/Public/Syntax/Syntaxes.g.h
    // [src]/Syntax/Syntaxes.g.cpp
    path hPath = [srcPath]() mutable { return srcPath.append("Syntax").append("Public").append("Syntax").append("Syntaxes.g.h"); }();
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

#include <Infra/Json.h>
#include <Infra/Unreachable.h>

namespace Citron {
class ArgumentSyntax;

)---";

    cppStream << R"---(#include "pch.h"

#include "Syntaxes.g.h"
#include <Infra/Json.h>

using namespace std;

namespace Citron {

namespace {
struct ToJsonVisitor {
    template<typename T>
    JsonItem operator()(std::shared_ptr<T>& t) { return t->ToJson(); }

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
                "STypeExp_BoxPtr",
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
                "SClassMemberFuncDecl",
                "SClassConstructorDecl",
                "SClassMemberVarDecl",
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SStructMemberDecl",
                "SStructMemberFuncDecl",
                "SStructConstructorDecl",
                "SStructMemberVarDecl",
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
                "SScript"
            }
        },

        SharedPtrDeclsInfo {
            .names {
                "SStmt",
                "SExp",
                "STypeExp",
                "SStringExpElement",
                "SLambdaExpBody",
                "SEmbeddableStmt",
                "SForStmtInitializer",
                "SClassMemberDecl",
                "SStructMemberDecl",
                "SNamespaceDeclElement",
                "SScriptElement"
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
        },

        ClassInfo {
            .name = "SArgument",
            .memberInfos {
                    {.type = "bool", .memberVarName = "bOut", .getterName = "HasOut" },
                    {.type = "bool", .memberVarName = "bParams", .getterName = "GetParams" },
                    {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
                },
                .extraConstructors {
                    "SYNTAX_API SArgument(SExpPtr exp);"
                }
        },

        ClassInfo {
            .name = "SLambdaExpParam",
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
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
                    {.type = "SExpPtr", .memberVarName = "initExp", .getterName = "GetInitExp" }
                },
        },

            ClassInfo {
                .name = "SVarDecl",
                .memberInfos {
                    {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
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
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" }
            },
        },


        // Variants 
        #pragma region VariantInterfaceInfo
        VariantInterfaceInfo {
            .name = "SStmt",
            .bases { "SSyntax" },
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
            .bases { "SSyntax" },
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
            .bases { "SSyntax" },
            .argName = "typeExp",
            .members {
                "STypeExp_Id",
                "STypeExp_Member",
                "STypeExp_Nullable",
                "STypeExp_LocalPtr",
                "STypeExp_BoxPtr",
                "STypeExp_Local"
            }
        },

        VariantInterfaceInfo {
            .name = "SStringExpElement",
            .bases { "SSyntax" },
            .argName = "elem",
            .members {
                "SStringExpElement_Text",
                "SStringExpElement_Exp"
            }
        },

        VariantInterfaceInfo {
            .name = "SLambdaExpBody",
            .bases { "SSyntax" },
            .argName = "body",
            .members {
                "SLambdaExpBody_Stmts",
                "SLambdaExpBody_Exp"
            }
        },

        VariantInterfaceInfo {
            .name = "SEmbeddableStmt",
            .bases { "SSyntax" },
            .argName = "stmt",
            .members {
                "SEmbeddableStmt_Single",
                "SEmbeddableStmt_Block"
            }
        },

        VariantInterfaceInfo {
            .name = "SForStmtInitializer",
            .bases { "SSyntax" },
            .argName = "initializer",
            .members {
                "SForStmtInitializer_Exp",
                "SForStmtInitializer_VarDecl"
            }
        },

        // SClassMemberDecl
        VariantInterfaceInfo {
            .name = "SClassMemberDecl",
            .bases { "SSyntax" },
            .argName = "decl",
            .members {
                "SClassDecl",
                "SStructDecl",
                "SEnumDecl",
                "SClassMemberFuncDecl",
                "SClassConstructorDecl",
                "SClassMemberVarDecl",
            }
        },

        // SStructMemberDecl
        VariantInterfaceInfo {
            .name = "SStructMemberDecl",
            .bases { "SSyntax" },
            .argName = "decl",
            .members {
                "SClassDecl",
                "SStructDecl",
                "SEnumDecl",
                "SStructMemberFuncDecl",
                "SStructConstructorDecl",
                "SStructMemberVarDecl",
            }
        },

        // SNamespaceDeclElement
        VariantInterfaceInfo {
            .name = "SNamespaceDeclElement",
            .bases { "SSyntax" },
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
            .bases { "SSyntax" },
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
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
            .extraConstructors {
                "SExp_Identifier(std::string value) : SExp_Identifier(std::move(value), {}) { }"
            }
        },

        ClassInfo {
            .name = "SExp_String",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "std::vector<SStringExpElementPtr>", .memberVarName = "elements", .getterName = "GetElements" },
            },
            .extraConstructors {
                "SYNTAX_API SExp_String(std::string str);"
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
                { .type = "std::vector<SExpPtr>", .memberVarName = "elements", .getterName = "GetElements" },
            },
        },

        ClassInfo {
            .name = "SExp_New",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
                {.type = "std::vector<SArgument>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        ClassInfo {
            .name = "SExp_BinaryOp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SBinaryOpKind", .memberVarName = "kind", .getterName = "GetKind" },
                {.type = "SExpPtr", .memberVarName = "operand0", .getterName = "GetOperand0" },
                {.type = "SExpPtr", .memberVarName = "operand1", .getterName = "GetOperand1" },
            },
        },

        ClassInfo {
            .name = "SExp_UnaryOp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SUnaryOpKind", .memberVarName = "kind", .getterName = "GetKind" },
                {.type = "SExpPtr", .memberVarName = "operand", .getterName = "GetOperand" },
            },
        },

        ClassInfo {
            .name = "SExp_Call",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "callable", .getterName = "GetCallable" },
                {.type = "std::vector<SArgument>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        ClassInfo {
            .name = "SExp_Lambda",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "std::vector<SLambdaExpParam>", .memberVarName = "params", .getterName = "GetParams" },
                { .type = "SLambdaExpBodyPtr", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        ClassInfo {
            .name = "SExp_Indexer",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "obj", .getterName = "GetObject" },
                {.type = "SExpPtr", .memberVarName = "index", .getterName = "GetIndex" },
            },
        },

        ClassInfo {
            .name = "SExp_Member",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "parent", .getterName = "GetParent" },
                {.type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API SExp_Member(SExpPtr parent, std::string memberName);"
            }
        },

        ClassInfo {
            .name = "SExp_IndirectMember",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "parent", .getterName = "GetParent" },
                {.type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API SExp_IndirectMember(SExpPtr parent, std::string memberName);"
            }
        },

        ClassInfo {
            .name = "SExp_Box",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "innerExp", .getterName = "GetInnerExp" },
            },
        },

        ClassInfo {
            .name = "SExp_Is",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
            },
        },

        ClassInfo {
            .name = "SExp_As",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
            },
        },

        #pragma endregion SExps

        #pragma region TypeExps

        
        ClassInfo {
            .name = "STypeExp_Id",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API STypeExp_Id(std::string name);"
            }
        },

        // STypeExp_Member(STypeExp typeExp, std::string name, std::vector<STypeExpPtr> typeArgs);
        ClassInfo {
            .name = "STypeExp_Member",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "parentType", .getterName = "GetParentType" },
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
        },

        // STypeExp_Nullable(STypeExp typeExp)
        ClassInfo {
            .name = "STypeExp_Nullable",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // STypeExp_LocalPtr(STypeExp typeExp)
        ClassInfo {
            .name = "STypeExp_LocalPtr",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // STypeExp_BoxPtr(STypeExp typeExp)
        ClassInfo {
            .name = "STypeExp_BoxPtr",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // STypeExp_Local(STypeExp typeExp)
        ClassInfo {
            .name = "STypeExp_Local",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "innerType", .getterName = "GetInnerType" },
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
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
            },
        },
        #pragma endregion SStringExpElements

        #pragma region SLambdaExpBodys
        
        ClassInfo {
            .name = "SLambdaExpBody_Stmts",
            .variantInterfaces { "SLambdaExpBody" },
            .memberInfos {
                { .type = "std::vector<SStmtPtr>", .memberVarName = "stmts", .getterName = "GetStmts" }
            },
        },

        ClassInfo {
            .name = "SLambdaExpBody_Exp",
            .variantInterfaces { "SLambdaExpBody" },
            .memberInfos = {
                { .type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" }
            },
        },
        #pragma endregion SLambdaExpBodys

        #pragma region SEmbeddableStmts

        // SEmbeddableStmt_Single(SStmtPtr stmt), SStmt에 depends
        ClassInfo {
            .name = "SEmbeddableStmt_Single",
            .variantInterfaces { "SEmbeddableStmt" },
            .memberInfos {
                { .type = "SStmtPtr", .memberVarName = "stmt", .getterName = "GetStmt" },
            },
        },

        // SEmbeddableStmt_Block(std::vector<SStmtPtr> stmts)
        ClassInfo {
            .name = "SEmbeddableStmt_Block",
            .variantInterfaces { "SEmbeddableStmt" },
            .memberInfos {
                {.type = "std::vector<SStmtPtr>", .memberVarName = "stmts", .getterName = "GetStmts" },
            },
        },

        #pragma endregion SEmbeddableStmts

        #pragma region SForStmtInitializer
        
        ClassInfo {
            .name = "SForStmtInitializer_Exp",
            .variantInterfaces { "SForStmtInitializer" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" }
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
                {.type = "std::vector<std::shared_ptr<SExp_String>>", .memberVarName = "commands", .getterName = "GetCommands" },
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

        // SStmt_Block(std::vector<SStmtPtr> stmts)
        ClassInfo {
            .name = "SStmt_Block",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmtPtr>", .memberVarName = "stmts", .getterName = "GetStmts" },
            },
        },

        // SStmt_Blank
            ClassInfo {
                .name = "SStmt_Blank",
                .variantInterfaces { "SStmt" },
                .memberInfos {},
        },

        // SStmt_Task(std::vector<SStmtPtr> body)
        ClassInfo {
            .name = "SStmt_Task",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStmt_Await(std::vector<SStmtPtr> body);
        ClassInfo {
            .name = "SStmt_Await",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStmt_Async(std::vector<SStmtPtr> body);
            ClassInfo {
                .name = "SStmt_Async",
                .variantInterfaces { "SStmt" },
                .memberInfos {
                    {.type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
                },
        },

        // SStmt_Directive(std::u32string name, std::vector<SExpPtr> args)
        ClassInfo {
            .name = "SStmt_Directive",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<SExpPtr>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        // SStmt_If(SExp cond, SEmbeddableStmt body, SEmbeddableStmtPtr elseBody)
        ClassInfo {
            .name = "SStmt_If",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                { .type = "SExpPtr", .memberVarName = "cond", .getterName = "GetCond" },
                { .type = "SEmbeddableStmtPtr", .memberVarName = "body", .getterName = "GetBody" },
                { .type = "SEmbeddableStmtPtr", .memberVarName = "elseBody", .getterName = "GetElseBody" },
            },
        },

        // SStmt_IfTest(STypeExp testTypeExp, std::string varName, SExp exp, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody);
            ClassInfo {
                .name = "SStmt_IfTest",
                .variantInterfaces { "SStmt" },
                .memberInfos {
                    {.type = "STypeExpPtr", .memberVarName = "testType", .getterName = "GetTestType" },
                    {.type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                    {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
                    {.type = "SEmbeddableStmtPtr", .memberVarName = "body", .getterName = "GetBody" },
                    {.type = "SEmbeddableStmtPtr", .memberVarName = "elseBody", .getterName = "GetElseBody" },
                },
        },

        // SStmt_For(SForStmtInitializerPtr initializer, 
        //      SExpPtr condExp, 
        //      SExpPtr continueExp, 
        //      SEmbeddableStmt body);
        ClassInfo {
            .name = "SStmt_For",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SForStmtInitializerPtr", .memberVarName = "initializer", .getterName = "GetInitializer" },
                {.type = "SExpPtr", .memberVarName = "cond", .getterName = "GetCond" },
                {.type = "SExpPtr", .memberVarName = "cont", .getterName = "GetCont" },
                {.type = "SEmbeddableStmtPtr", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        ClassInfo {
            .name = "SStmt_Return",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        // SStmt_Exp(SExp exp)
        ClassInfo {
            .name = "SStmt_Exp",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
            },
        },

        // SStmt_Foreach(STypeExp type, std::u32string varName, SExp enumerable, SEmbeddableStmt body);
        ClassInfo {
            .name = "SStmt_Foreach",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
                {.type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                {.type = "SExpPtr", .memberVarName = "enumerable", .getterName = "GetEnumerable" },
                {.type = "SEmbeddableStmtPtr", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStmt_Yield(SExp value)
        ClassInfo {
            .name = "SStmt_Yield",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "value", .getterName = "GetValue" },
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
                { .type = "STypeExpPtr", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
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
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "baseTypes", .getterName = "GetBaseTypes" },
                {.type = "std::vector<SClassMemberDeclPtr>", .memberVarName = "memberDecls", .getterName = "GetMemberDecls" },
            },
        },

        // SClassMemberFuncDecl
        ClassInfo {
            .name = "SClassMemberFuncDecl",
            .variantInterfaces { "SClassMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "bool", .memberVarName = "bStatic", .getterName = "IsStatic" },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence" },
                { .type = "STypeExpPtr", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SClassConstructorDecl
        ClassInfo {
            .name = "SClassConstructorDecl",
            .variantInterfaces { "SClassMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::optional<std::vector<SArgument>>", .memberVarName = "baseArgs", .getterName = "GetBaseArgs" },
                { .type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SClassMemberVarDecl
        ClassInfo {
            .name = "SClassMemberVarDecl",
            .variantInterfaces { "SClassMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "STypeExpPtr", .memberVarName = "varType", .getterName = "GetVarType" },
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
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "baseTypes", .getterName = "GetBaseTypes" },
                {.type = "std::vector<SStructMemberDeclPtr>", .memberVarName = "memberDecls", .getterName = "GetMemberDecls" },
            },
        },

        // SStructMemberFuncDecl
        ClassInfo {
            .name = "SStructMemberFuncDecl",
            .variantInterfaces { "SStructMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAcessModifier" },
                { .type = "bool", .memberVarName = "bStatic", .getterName = "IsStatic" },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence" }, // seq 함수인가  
                { .type = "STypeExpPtr", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<STypeParam>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStructConstructorDecl
        ClassInfo {
            .name = "SStructConstructorDecl",
            .variantInterfaces { "SStructMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::vector<SFuncParam>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SStructMemberVarDecl
        ClassInfo {
            .name = "SStructMemberVarDecl",
            .variantInterfaces { "SStructMemberDecl" },
            .memberInfos {
                { .type = "std::optional<SAccessModifier>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "STypeExpPtr", .memberVarName = "varType", .getterName = "GetVarType" },
                { .type = "std::vector<std::string>", .memberVarName = "varNames", .getterName = "GetVarNames" },
            },
        },
        
        #pragma endregion SStructDecls

        #pragma region SEnumDecl

        // SEnumElemMemberVarDecl
        ClassInfo {
            .name = "SEnumElemMemberVarDecl",
            .memberInfos {
                { .type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
            },
        },

        // SEnumElemDecl
        ClassInfo {
            .name = "SEnumElemDecl",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<std::shared_ptr<SEnumElemMemberVarDecl>>", .memberVarName = "memberVars", .getterName = "GetMemberVars" },
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
                { .type = "std::vector<std::shared_ptr<SEnumElemDecl>>", .memberVarName = "elements", .getterName = "GetElements" },
            },
        },

        #pragma endregion SEnumDecl
        
        // SNamespaceDecl
        ClassInfo {
            .name = "SNamespaceDecl",
            .variantInterfaces { "SNamespaceDeclElement", "SScriptElement" },
            .memberInfos {
                { .type = "std::vector<std::string>", .memberVarName = "names", .getterName = "GetNames" },
                { .type = "std::vector<SNamespaceDeclElementPtr>", .memberVarName = "elements", .getterName = "GetElements" }
            },
        },
        
        // Script
        ClassInfo {
            .name = "SScript",
            .memberInfos {
                { .type = "std::vector<SScriptElementPtr>", .memberVarName = "elements", .getterName = "GetElements" },
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
