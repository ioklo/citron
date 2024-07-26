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
    JsonItem operator()(std::unique_ptr<T>& t) { return t->ToJson(); }

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
                "SCommandStmt",
                "SVarDeclStmt",
                "SIfStmt",
                "SIfTestStmt",
                "SForStmt",
                "SContinueStmt",
                "SBreakStmt",
                "SReturnStmt",
                "SBlockStmt",
                "SBlankStmt",
                "SExpStmt",
                "STaskStmt",
                "SAwaitStmt",
                "SAsyncStmt",
                "SForeachStmt",
                "SYieldStmt",
                "SDirectiveStmt"
            }
        },

        // SExp
        ForwardClassDeclsInfo {
            .names {
                "SExp",
                "SIdentifierExp",
                "SStringExp",
                "SIntLiteralExp",
                "SBoolLiteralExp",
                "SNullLiteralExp",
                "SBinaryOpExp",
                "SUnaryOpExp",
                "SCallExp",
                "SLambdaExp",
                "SIndexerExp",
                "SMemberExp",
                "SIndirectMemberExp",
                "SListExp",
                "SNewExp",
                "SBoxExp",
                "SIsExp",
                "SAsExp"
            }
        },

        // TypeExp
        ForwardClassDeclsInfo {
            .names {
                "STypeExp",
                "SIdTypeExp",
                "SMemberTypeExp",
                "SNullableTypeExp",
                "SLocalPtrTypeExp",
                "SBoxPtrTypeExp",
                "SLocalTypeExp"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SStringExpElement",
                "STextStringExpElement",
                "SExpStringExpElement"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SLambdaExpBody",
                "SStmtsLambdaExpBody",
                "SExpLambdaExpBody"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SEmbeddableStmt",
                "SSingleEmbeddableStmt",
                "SBlockEmbeddableStmt"
            }
        },

        ForwardClassDeclsInfo {
            .names {
                "SForStmtInitializer",
                "SExpForStmtInitializer",
                "SVarDeclForStmtInitializer"
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

        PtrDeclsInfo {
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
            .argName = "stmt",
            .members {
                "SCommandStmt",
                "SVarDeclStmt",
                "SIfStmt",
                "SIfTestStmt",
                "SForStmt",
                "SContinueStmt",
                "SBreakStmt",
                "SReturnStmt",
                "SBlockStmt",
                "SBlankStmt",
                "SExpStmt",
                "STaskStmt",
                "SAwaitStmt",
                "SAsyncStmt",
                "SForeachStmt",
                "SYieldStmt",
                "SDirectiveStmt"
            }
        },

        VariantInterfaceInfo {
            .name = "SExp",
            .argName = "exp",
            .members {
                "SIdentifierExp",
                "SStringExp",
                "SIntLiteralExp",
                "SBoolLiteralExp",
                "SNullLiteralExp",
                "SBinaryOpExp",
                "SUnaryOpExp",
                "SCallExp",
                "SLambdaExp",
                "SIndexerExp",
                "SMemberExp",
                "SIndirectMemberExp",
                "SListExp",
                "SNewExp",
                "SBoxExp",
                "SIsExp",
                "SAsExp"
            }
        },

        VariantInterfaceInfo {
            .name = "STypeExp",
            .argName = "typeExp",
            .members {
                "SIdTypeExp",
                "SMemberTypeExp",
                "SNullableTypeExp",
                "SLocalPtrTypeExp",
                "SBoxPtrTypeExp",
                "SLocalTypeExp"
            }
        },

        VariantInterfaceInfo {
            .name = "SStringExpElement",
            .argName = "elem",
            .members {
                "STextStringExpElement",
                "SExpStringExpElement"
            }
        },

        VariantInterfaceInfo {
            .name = "SLambdaExpBody",
            .argName = "body",
            .members {
                "SStmtsLambdaExpBody",
                "SExpLambdaExpBody"
            }
        },

        VariantInterfaceInfo {
            .name = "SEmbeddableStmt",
            .argName = "stmt",
            .members {
                "SSingleEmbeddableStmt",
                "SBlockEmbeddableStmt"
            }
        },

        VariantInterfaceInfo {
            .name = "SForStmtInitializer",
            .argName = "initializer",
            .members {
                "SExpForStmtInitializer",
                "SVarDeclForStmtInitializer"
            }
        },

        // SClassMemberDecl
        VariantInterfaceInfo {
            .name = "SClassMemberDecl",
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
            .name = "SIdentifierExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "std::string", .memberVarName = "value", .getterName = "GetValue" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
            .extraConstructors {
                "SIdentifierExp(std::string value) : SIdentifierExp(std::move(value), {}) { }"
            }
        },

        ClassInfo {
            .name = "SStringExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "std::vector<SStringExpElementPtr>", .memberVarName = "elements", .getterName = "GetElements" },
            },
            .extraConstructors {
                "SYNTAX_API SStringExp(std::string str);"
            }
        },

        ClassInfo {
            .name = "SIntLiteralExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "int", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        ClassInfo {
            .name = "SBoolLiteralExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "bool", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        ClassInfo {
            .name = "SNullLiteralExp",
            .variantInterfaces { "SExp" },
            .memberInfos { },
        },

        ClassInfo {
            .name = "SListExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "std::vector<SExpPtr>", .memberVarName = "elements", .getterName = "GetElements" },
            },
        },

        ClassInfo {
            .name = "SNewExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
                {.type = "std::vector<SArgument>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        ClassInfo {
            .name = "SBinaryOpExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SBinaryOpKind", .memberVarName = "kind", .getterName = "GetKind" },
                {.type = "SExpPtr", .memberVarName = "operand0", .getterName = "GetOperand0" },
                {.type = "SExpPtr", .memberVarName = "operand1", .getterName = "GetOperand1" },
            },
        },

        ClassInfo {
            .name = "SUnaryOpExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SUnaryOpKind", .memberVarName = "kind", .getterName = "GetKind" },
                {.type = "SExpPtr", .memberVarName = "operand", .getterName = "GetOperand" },
            },
        },

        ClassInfo {
            .name = "SCallExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "callable", .getterName = "GetCallable" },
                {.type = "std::vector<SArgument>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },



        ClassInfo {
            .name = "SLambdaExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                { .type = "std::vector<SLambdaExpParam>", .memberVarName = "params", .getterName = "GetParams" },
                { .type = "SLambdaExpBodyPtr", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        ClassInfo {
            .name = "SIndexerExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "obj", .getterName = "GetObject" },
                {.type = "SExpPtr", .memberVarName = "index", .getterName = "GetIndex" },
            },
        },

        ClassInfo {
            .name = "SMemberExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "parent", .getterName = "GetParent" },
                {.type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API SMemberExp(SExpPtr parent, std::string memberName);"
            }
        },

        ClassInfo {
            .name = "SIndirectMemberExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "parent", .getterName = "GetParent" },
                {.type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API SIndirectMemberExp(SExpPtr parent, std::string memberName);"
            }
        },

        ClassInfo {
            .name = "SBoxExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "innerExp", .getterName = "GetInnerExp" },
            },
        },

        ClassInfo {
            .name = "SIsExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
            },
        },

        ClassInfo {
            .name = "SAsExp",
            .variantInterfaces { "SExp" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
            },
        },

        #pragma endregion SExps

        #pragma region TypeExps

        
        ClassInfo {
            .name = "SIdTypeExp",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API SIdTypeExp(std::string name);"
            }
        },

        // SMemberTypeExp(STypeExp typeExp, std::string name, std::vector<STypeExpPtr> typeArgs);
        ClassInfo {
            .name = "SMemberTypeExp",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "parentType", .getterName = "GetParentType" },
                {.type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                {.type = "std::vector<STypeExpPtr>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
        },

        // SNullableTypeExp(STypeExp typeExp)
        ClassInfo {
            .name = "SNullableTypeExp",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // SLocalPtrTypeExp(STypeExp typeExp)
        ClassInfo {
            .name = "SLocalPtrTypeExp",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // SBoxPtrTypeExp(STypeExp typeExp)
        ClassInfo {
            .name = "SBoxPtrTypeExp",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // SLocalTypeExp(STypeExp typeExp)
        ClassInfo {
            .name = "SLocalTypeExp",
            .variantInterfaces { "STypeExp" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        #pragma endregion STypeExps

        #pragma region SStringExpElements

        ClassInfo {
            .name = "STextStringExpElement",
            .variantInterfaces { "SStringExpElement" },
            .memberInfos {
                {.type = "std::string", .memberVarName = "text", .getterName = "GetText" },
            },
        },

        ClassInfo {
            .name = "SExpStringExpElement",
            .variantInterfaces { "SStringExpElement" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
            },
        },
        #pragma endregion SStringExpElements

        #pragma region SLambdaExpBodys
        
        ClassInfo {
            .name = "SStmtsLambdaExpBody",
            .variantInterfaces { "SLambdaExpBody" },
            .memberInfos {
                { .type = "std::vector<SStmtPtr>", .memberVarName = "stmts", .getterName = "GetStmts" }
            },
        },

        ClassInfo {
            .name = "SExpLambdaExpBody",
            .variantInterfaces { "SLambdaExpBody" },
            .memberInfos = {
                { .type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" }
            },
        },
        #pragma endregion SLambdaExpBodys

        #pragma region SEmbeddableStmts

        // SSingleEmbeddableStmt(SStmtPtr stmt), SStmt에 depends
        ClassInfo {
            .name = "SSingleEmbeddableStmt",
            .variantInterfaces { "SEmbeddableStmt" },
            .memberInfos {
                { .type = "SStmtPtr", .memberVarName = "stmt", .getterName = "GetStmt" },
            },
        },

        // SBlockEmbeddableStmt(std::vector<SStmtPtr> stmts)
        ClassInfo {
            .name = "SBlockEmbeddableStmt",
            .variantInterfaces { "SEmbeddableStmt" },
            .memberInfos {
                {.type = "std::vector<SStmtPtr>", .memberVarName = "stmts", .getterName = "GetStmts" },
            },
        },

        #pragma endregion SEmbeddableStmts

        #pragma region SForStmtInitializer
        
        ClassInfo {
            .name = "SExpForStmtInitializer",
            .variantInterfaces { "SForStmtInitializer" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" }
            },
        },

        ClassInfo {
            .name = "SVarDeclForStmtInitializer",
            .variantInterfaces { "SForStmtInitializer" },
            .memberInfos {
                {.type = "SVarDecl", .memberVarName = "varDecl", .getterName = "GetVarDecl" }
            },
        },

        #pragma endregion SForStmtInitializer

        #pragma region SStmts

        // SCommandStmt(std::vector<SStringExp> commands)
        ClassInfo {
            .name = "SCommandStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStringExp>", .memberVarName = "commands", .getterName = "GetCommands" },
            },
        },

        // SVarDeclStmt(SVarDecl varDecl)
        ClassInfo {
            .name = "SVarDeclStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SVarDecl", .memberVarName = "varDecl", .getterName = "GetVarDecl" },
            },
        },

        // SContinueStmt
        ClassInfo {
            .name = "SContinueStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {},
        },

        // SBreakStmt
        ClassInfo {
            .name = "SBreakStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {},
        },

        // SBlockStmt(std::vector<SStmtPtr> stmts)
        ClassInfo {
            .name = "SBlockStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmtPtr>", .memberVarName = "stmts", .getterName = "GetStmts" },
            },
        },

        // SBlankStmt
            ClassInfo {
                .name = "SBlankStmt",
                .variantInterfaces { "SStmt" },
                .memberInfos {},
        },

        // STaskStmt(std::vector<SStmtPtr> body)
        ClassInfo {
            .name = "STaskStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SAwaitStmt(std::vector<SStmtPtr> body);
        ClassInfo {
            .name = "SAwaitStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SAsyncStmt(std::vector<SStmtPtr> body);
            ClassInfo {
                .name = "SAsyncStmt",
                .variantInterfaces { "SStmt" },
                .memberInfos {
                    {.type = "std::vector<SStmtPtr>", .memberVarName = "body", .getterName = "GetBody" },
                },
        },

        // SDirectiveStmt(std::u32string name, std::vector<SExpPtr> args)
        ClassInfo {
            .name = "SDirectiveStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<SExpPtr>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        // SIfStmt(SExp cond, SEmbeddableStmt body, SEmbeddableStmtPtr elseBody)
        ClassInfo {
            .name = "SIfStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                { .type = "SExpPtr", .memberVarName = "cond", .getterName = "GetCond" },
                { .type = "SEmbeddableStmtPtr", .memberVarName = "body", .getterName = "GetBody" },
                { .type = "SEmbeddableStmtPtr", .memberVarName = "elseBody", .getterName = "GetElseBody" },
            },
        },

        // SIfTestStmt(STypeExp testTypeExp, std::string varName, SExp exp, SEmbeddableStmtPtr body, SEmbeddableStmtPtr elseBody);
                ClassInfo {
                    .name = "SIfTestStmt",
                    .variantInterfaces { "SStmt" },
                    .memberInfos {
                        {.type = "STypeExpPtr", .memberVarName = "testType", .getterName = "GetTestType" },
                        {.type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                        {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
                        {.type = "SEmbeddableStmtPtr", .memberVarName = "body", .getterName = "GetBody" },
                        {.type = "SEmbeddableStmtPtr", .memberVarName = "elseBody", .getterName = "GetElseBody" },
                    },
        },

        // SForStmt(SForStmtInitializerPtr initializer, 
        //      SExpPtr condExp, 
        //      SExpPtr continueExp, 
        //      SEmbeddableStmt body);
        ClassInfo {
            .name = "SForStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SForStmtInitializerPtr", .memberVarName = "initializer", .getterName = "GetInitializer" },
                {.type = "SExpPtr", .memberVarName = "cond", .getterName = "GetCond" },
                {.type = "SExpPtr", .memberVarName = "cont", .getterName = "GetCont" },
                {.type = "SEmbeddableStmtPtr", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        ClassInfo {
            .name = "SReturnStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        // SExpStmt(SExp exp)
        ClassInfo {
            .name = "SExpStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "SExpPtr", .memberVarName = "exp", .getterName = "GetExp" },
            },
        },

        // SForeachStmt(STypeExp type, std::u32string varName, SExp enumerable, SEmbeddableStmt body);
        ClassInfo {
            .name = "SForeachStmt",
            .variantInterfaces { "SStmt" },
            .memberInfos {
                {.type = "STypeExpPtr", .memberVarName = "type", .getterName = "GetType" },
                {.type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                {.type = "SExpPtr", .memberVarName = "enumerable", .getterName = "GetEnumerable" },
                {.type = "SEmbeddableStmtPtr", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // SYieldStmt(SExp value)
        ClassInfo {
            .name = "SYieldStmt",
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
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
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
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
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
                { .type = "std::vector<SEnumElemMemberVarDecl>", .memberVarName = "memberVars", .getterName = "GetMemberVars" },
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
                { .type = "std::vector<SEnumElemDecl>", .memberVarName = "elements", .getterName = "GetElements" },
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
