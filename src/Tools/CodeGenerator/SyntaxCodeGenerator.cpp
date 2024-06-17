#include "Misc.h"
#include <iostream>
#include <fmt/core.h>


using namespace std;
using namespace std::filesystem;

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
#include <memory>
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

    CommonInfo commonInfo = { .linkage = "SYNTAX_API" };

    vector<ItemInfo> itemInfos {

        VariantInfo {
            .name = "StmtSyntax",
            .argName = "stmt",
            .memberNames {
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
            .name = "ExpSyntax", 
            .argName = "exp",
            .memberNames {
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
                "class IndirectMemberExpSyntax",
                "class ListExpSyntax",
                "class NewExpSyntax",
                "class BoxExpSyntax",
                "class IsExpSyntax",
                "class AsExpSyntax"
            }
        },

        VariantInfo {
            .name = "TypeExpSyntax",
            .argName = "typeExp",
            .memberNames {
                "class IdTypeExpSyntax",
                "class MemberTypeExpSyntax",
                "class NullableTypeExpSyntax",
                "class LocalPtrTypeExpSyntax",
                "class BoxPtrTypeExpSyntax",
                "class LocalTypeExpSyntax"
            }
        },


        ClassInfo {
            .name = "IdTypeExpSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs", .bUsePimpl = false },
            },
            .bDefaultsInline = true,
            .extraConstructors {
                "IdTypeExpSyntax(std::string name) : IdTypeExpSyntax(std::move(name), {}) { }"
            }
        },

        // MemberTypeExpSyntax(TypeExpSyntax typeExp, std::string name, std::vector<TypeExpSyntax> typeArgs);
        ClassInfo {
            .name = "MemberTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "parentType", .getterName = "GetParentType", .bUsePimpl = true },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs", .bUsePimpl = false },
            },
            .bDefaultsInline = false,
        },

        // NullableTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            .name = "NullableTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "innerType", .getterName = "GetInnerType", .bUsePimpl = true },
            },
            .bDefaultsInline = false,
        },

        // LocalPtrTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            .name = "LocalPtrTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "innerType", .getterName = "GetInnerType", .bUsePimpl = true },
            },
            .bDefaultsInline = false,
        },

        // BoxPtrTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            .name = "BoxPtrTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "innerType", .getterName = "GetInnerType", .bUsePimpl = true },
            },
            .bDefaultsInline = false,
        },

        // LocalTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            .name = "LocalTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "innerType", .getterName = "GetInnerType", .bUsePimpl = true },
            },
            .bDefaultsInline = false,
        },

        ClassInfo {
            .name = "LambdaExpParamSyntax",
            .memberInfos {
                { .type = "std::optional<TypeExpSyntax>", .memberVarName = "type", .getterName = "GetType", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "hasOut", .getterName = "HasOut", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "hasParams", .getterName = "HasParams", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        EnumInfo {
            .name = "AccessModifierSyntax",
            .cases = { "Public", "Protected", "Private"}
        },

        EnumInfo {
            .name = "BinaryOpSyntaxKind",
            .cases {
                "Multiply", "Divide", "Modulo",
                "Add", "Subtract",
                "LessThan", "GreaterThan", "LessThanOrEqual", "GreaterThanOrEqual",
                "Equal", "NotEqual",
                "Assign",
            }
        },

        EnumInfo { 
            .name = "UnaryOpSyntaxKind",
            .cases {
                "PostfixInc", "PostfixDec",
                "Minus", "LogicalNot", "PrefixInc", "PrefixDec",
                "Ref", "Deref", // &, *, local인지 box인지는 분석을 통해서 알아내게 된다
            }
        },

        ClassInfo {
            .name = "ArgumentSyntax",
            .memberInfos {
                { .type = "bool", .memberVarName = "bOut", .getterName = "HasOut", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "bParams", .getterName = "GetParams", .bUsePimpl = false },
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp", .bUsePimpl = true },
            },
            .bDefaultsInline = false,
            .extraConstructors {
                "SYNTAX_API ArgumentSyntax(ExpSyntax exp);"
            }
        },

        ClassInfo {
            .name = "IdentifierExpSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "value", .getterName = "GetValue", .bUsePimpl = false },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs", .bUsePimpl = false },
            },
            .bDefaultsInline = true,
            .extraConstructors {
                "IdentifierExpSyntax(std::string value) : IdentifierExpSyntax(std::move(value), {}) { }"
            }
        },

        // StringExpSyntaxElement
        VariantInfo {
            .name = "StringExpSyntaxElement",
            .argName = "elem",
            .memberNames {
                "class TextStringExpSyntaxElement",
                "class ExpStringExpSyntaxElement"
            }
        },

        ClassInfo {
            .name = "TextStringExpSyntaxElement",
            .memberInfos {
                { .type = "std::string", .memberVarName = "text", .getterName = "GetText", .bUsePimpl = false },
            },
            .bDefaultsInline = true,
        },

        ClassInfo {
            .name = "ExpStringExpSyntaxElement",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp", .bUsePimpl = true },
            },
            .bDefaultsInline = false,
        },

        ClassInfo {
            .name = "StringExpSyntax",
            .memberInfos {
                { .type = "std::vector<StringExpSyntaxElement>", .memberVarName = "elements", .getterName = "GetElements", .bUsePimpl = false },
            },
            .bDefaultsInline = false,
            .extraConstructors {
                "SYNTAX_API StringExpSyntax(std::string str);"
            }
        },

        ClassInfo {
            .name = "IntLiteralExpSyntax",
            .memberInfos {
                { .type = "int", .memberVarName = "value", .getterName = "GetValue", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        ClassInfo {
            .name = "BoolLiteralExpSyntax",
            .memberInfos {
                { .type = "bool", .memberVarName = "value", .getterName = "GetValue", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        ClassInfo {
            .name = "NullLiteralExpSyntax",
            .memberInfos { },
            .bDefaultsInline = true
        },

        ClassInfo {
            .name = "BinaryOpExpSyntax",
            .memberInfos {
                { .type = "BinaryOpSyntaxKind", .memberVarName = "kind", .getterName = "GetKind", .bUsePimpl = false },
                { .type = "ExpSyntax", .memberVarName = "operand0", .getterName = "GetOperand0", .bUsePimpl = true },
                { .type = "ExpSyntax", .memberVarName = "operand1", .getterName = "GetOperand1", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "UnaryOpExpSyntax",
            .memberInfos {
                { .type = "UnaryOpSyntaxKind", .memberVarName = "kind", .getterName = "GetKind", .bUsePimpl = false },
                { .type = "ExpSyntax", .memberVarName = "operand", .getterName = "GetOperand", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "CallExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "callable", .getterName = "GetCallable", .bUsePimpl = true },
                { .type = "std::vector<ArgumentSyntax>", .memberVarName = "args", .getterName = "GetArgs", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        VariantInfo {
            .name = "LambdaExpBodySyntax",
            .argName = "body",
            .memberNames {
                "class StmtsLambdaExpBodySyntax",
                "class ExpLambdaExpBodySyntax"
            }
        },

        ClassInfo {
            .name = "StmtsLambdaExpBodySyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "stmts", .getterName = "GetStmts", .bUsePimpl = false }
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "ExpLambdaExpBodySyntax",
            .memberInfos = {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp", .bUsePimpl = true }
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "LambdaExpSyntax",
            .memberInfos {
                { .type = "std::vector<LambdaExpParamSyntax>", .memberVarName = "params", .getterName = "GetParams", .bUsePimpl = false },
                { .type = "LambdaExpBodySyntax", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "IndexerExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "obj", .getterName = "GetObject", .bUsePimpl = true },
                { .type = "ExpSyntax", .memberVarName = "index", .getterName = "GetIndex", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "MemberExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "parent", .getterName = "GetParent", .bUsePimpl = true },
                { .type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName", .bUsePimpl = false },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs", .bUsePimpl = false },
            },
            .bDefaultsInline = false,
            .extraConstructors {
                "SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::string memberName);"
            }
        },

        ClassInfo {
            .name = "IndirectMemberExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "parent", .getterName = "GetParent", .bUsePimpl = true },
                { .type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName", .bUsePimpl = false },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs", .bUsePimpl = false },
            },
            .bDefaultsInline = false,
            .extraConstructors {
                "SYNTAX_API IndirectMemberExpSyntax(ExpSyntax parent, std::string memberName);"
            }
        },

        ClassInfo {
            .name = "ListExpSyntax",
            .memberInfos {
                { .type = "std::vector<ExpSyntax>", .memberVarName = "elements", .getterName = "GetElements", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "NewExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType", .bUsePimpl = false },
                { .type = "std::vector<ArgumentSyntax>", .memberVarName = "args", .getterName = "GetArgs", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "BoxExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "innerExp", .getterName = "GetInnerExp", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        ClassInfo{
            .name = "IsExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp", .bUsePimpl = true },
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "AsExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp", .bUsePimpl = true },
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        // Embeddable 
        VariantInfo {
            .name = "EmbeddableStmtSyntax",
            .argName = "embeddableStmt",
            .memberNames {
                "class SingleEmbeddableStmtSyntax",
                "class BlockEmbeddableStmtSyntax",
            }
        },

        // SingleEmbeddableStmtSyntax(StmtSyntax stmt)
        ClassInfo {
            .name = "SingleEmbeddableStmtSyntax",
            .memberInfos {
                { .type = "StmtSyntax", .memberVarName = "stmt", .getterName = "GetStmt", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        // BlockEmbeddableStmtSyntax(std::vector<StmtSyntax> stmts)
        ClassInfo {
            .name = "BlockEmbeddableStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "stmts", .getterName = "GetStmts", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        // VarDeclSyntax
        ClassInfo {
            .name = "VarDeclSyntaxElement",
            .memberInfos {
                { .type = "std::string", .memberVarName = "varName", .getterName = "GetVarName", .bUsePimpl = false },
                { .type = "std::optional<ExpSyntax>", .memberVarName = "initExp", .getterName = "GetInitExp", .bUsePimpl = true }
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "VarDeclSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType", .bUsePimpl = false },
                { .type = "std::vector<VarDeclSyntaxElement>", .memberVarName = "elements", .getterName = "GetElements", .bUsePimpl = false }
            },
            .bDefaultsInline = true
        },

        // Stmt

        // CommandStmtSyntax(std::vector<StringExpSyntax> commands)
        ClassInfo {
            .name = "CommandStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StringExpSyntax>", .memberVarName = "commands", .getterName = "GetCommands", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        // VarDeclStmtSyntax(VarDeclSyntax varDecl)
        ClassInfo {
            .name = "VarDeclStmtSyntax",
            .memberInfos {
                { .type = "VarDeclSyntax", .memberVarName = "varDecl", .getterName = "GetVarDecl", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody)
        ClassInfo {
            .name = "IfStmtSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "cond", .getterName = "GetCond", .bUsePimpl = true },
                { .type = "EmbeddableStmtSyntax", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = true },
                { .type = "std::optional<EmbeddableStmtSyntax>", .memberVarName = "elseBody", .getterName = "GetElseBody", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        // IfTestStmtSyntax(TypeExpSyntax testTypeExp, std::string varName, ExpSyntax exp, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
        ClassInfo {
            .name = "IfTestStmtSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "testType", .getterName = "GetTestType", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "varName", .getterName = "GetVarName", .bUsePimpl = false },
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp", .bUsePimpl = true },
                { .type = "EmbeddableStmtSyntax", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = true },
                { .type = "std::optional<EmbeddableStmtSyntax>", .memberVarName = "elseBody", .getterName = "GetElseBody", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        // 
        VariantInfo {
            .name = "ForStmtInitializerSyntax",
            .argName = "forInit",
            .memberNames {
                "class ExpForStmtInitializerSyntax",
                "class VarDeclForStmtInitializerSyntax"
            }
        },

        ClassInfo {
            .name = "ExpForStmtInitializerSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp", .bUsePimpl = true }
            },
            .bDefaultsInline = false
        },

        ClassInfo {
            .name = "VarDeclForStmtInitializerSyntax",
            .memberInfos {
                { .type = "VarDeclSyntax", .memberVarName = "varDecl", .getterName = "GetVarDecl", .bUsePimpl = true }
            },
            .bDefaultsInline = false
        },

        // ForStmtSyntax(std::optional<ForStmtInitializerSyntax> initializer, 
        //      std::optional<ExpSyntax> condExp, 
        //      std::optional<ExpSyntax> continueExp, 
        //      EmbeddableStmtSyntax body);
        ClassInfo {
            .name = "ForStmtSyntax",
            .memberInfos {
                { .type = "std::optional<ForStmtInitializerSyntax>", .memberVarName = "initializer", .getterName = "GetInitializer", .bUsePimpl = true },
                { .type = "std::optional<ExpSyntax>", .memberVarName = "cond", .getterName = "GetCond", .bUsePimpl = true },
                { .type = "std::optional<ExpSyntax>", .memberVarName = "cont", .getterName = "GetCont", .bUsePimpl = true },
                { .type = "EmbeddableStmtSyntax", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        // ContinueStmtSyntax
        ClassInfo {
            .name = "ContinueStmtSyntax",
            .memberInfos {},
            .bDefaultsInline = true
        },

        // BreakStmtSyntax
        ClassInfo {
            .name = "BreakStmtSyntax",
            .memberInfos {},
            .bDefaultsInline = true
        },

        ClassInfo {
            .name = "ReturnStmtSyntax",
            .memberInfos {
                { .type = "std::optional<ExpSyntax>", .memberVarName = "value", .getterName = "GetValue", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        // BlockStmtSyntax(std::vector<StmtSyntax> stmts)
        ClassInfo {
            .name = "BlockStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "stmts", .getterName = "GetStmts", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        // BlankStmtSyntax
        ClassInfo {
            .name = "BlankStmtSyntax",
            .memberInfos {},
            .bDefaultsInline = true
        },

        // ExpStmtSyntax(ExpSyntax exp)
        ClassInfo {
            .name = "ExpStmtSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        // TaskStmtSyntax(std::vector<StmtSyntax> body)
        ClassInfo {
            .name = "TaskStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        // AwaitStmtSyntax(std::vector<StmtSyntax> body);
        ClassInfo {
            .name = "AwaitStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        // AsyncStmtSyntax(std::vector<StmtSyntax> body);
        ClassInfo {
            .name = "AsyncStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },

        // ForeachStmtSyntax(TypeExpSyntax type, std::u32string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body);
        ClassInfo {
            .name = "ForeachStmtSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "varName", .getterName = "GetVarName", .bUsePimpl = false },
                { .type = "ExpSyntax", .memberVarName = "enumerable", .getterName = "GetEnumerable", .bUsePimpl = true },
                { .type = "EmbeddableStmtSyntax", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        // YieldStmtSyntax(ExpSyntax value)
        ClassInfo {
            .name = "YieldStmtSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "value", .getterName = "GetValue", .bUsePimpl = true },
            },
            .bDefaultsInline = false
        },

        // DirectiveStmtSyntax(std::u32string name, std::vector<ExpSyntax> args)
        ClassInfo {
            .name = "DirectiveStmtSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<ExpSyntax>", .memberVarName = "args", .getterName = "GetArgs", .bUsePimpl = false },
            },

            .bDefaultsInline = false
        },

        // TypeParamSyntax
        ClassInfo {
            .name = "TypeParamSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // FuncParamSyntax
        ClassInfo {
            .name = "FuncParamSyntax",
            .memberInfos {
                { .type = "bool", .memberVarName = "hasOut", .getterName = "HasOut", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "hasParams", .getterName = "HasParams", .bUsePimpl = false },
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false }
            },
        },

        // GlobalFuncDeclSyntax
        ClassInfo {
            .name = "GlobalFuncDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence", .bUsePimpl = false }, // seq 함수인가        
                { .type = "TypeExpSyntax", .memberVarName = "retType", .getterName = "GetRetType", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams", .bUsePimpl = false },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters", .bUsePimpl = false },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // ClassMemberFuncDeclSyntax
        ClassInfo {
            .name = "ClassMemberFuncDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "bStatic", .getterName = "IsStatic", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence", .bUsePimpl = false },
                { .type = "TypeExpSyntax", .memberVarName = "retType", .getterName = "GetRetType", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams", .bUsePimpl = false },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters", .bUsePimpl = false },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // ClassConstructorDeclSyntax
        ClassInfo {
            .name = "ClassConstructorDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters", .bUsePimpl = false },
                { .type = "std::optional<std::vector<ArgumentSyntax>>", .memberVarName = "baseArgs", .getterName = "GetBaseArgs", .bUsePimpl = false },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // ClassMemberVarDeclSyntax
        ClassInfo {
            .name = "ClassMemberVarDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "TypeExpSyntax", .memberVarName = "varType", .getterName = "GetVarType", .bUsePimpl = false },
                { .type = "std::vector<std::string>", .memberVarName = "varNames", .getterName = "GetVarNames", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // ClassMemberDeclSyntax
        VariantInfo {
            .name = "ClassMemberDeclSyntax",
            .argName = "decl",
            .memberNames {
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
            .name = "ClassDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams", .bUsePimpl = false },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "baseTypes", .getterName = "GetBaseTypes", .bUsePimpl = false },
                { .type = "std::vector<ClassMemberDeclSyntax>", .memberVarName = "memberDecls", .getterName = "GetMemberDecls", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // StructMemberFuncDeclSyntax
        ClassInfo {
            .name = "StructMemberFuncDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAcessModifier", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "bStatic", .getterName = "IsStatic", .bUsePimpl = false },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence", .bUsePimpl = false }, // seq 함수인가  
                { .type = "TypeExpSyntax", .memberVarName = "retType", .getterName = "GetRetType", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams", .bUsePimpl = false },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters", .bUsePimpl = false },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // StructConstructorDeclSyntax
        ClassInfo {
            .name = "StructConstructorDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters", .bUsePimpl = false },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // StructMemberVarDeclSyntax
        ClassInfo {
            .name = "StructMemberVarDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "TypeExpSyntax", .memberVarName = "varType", .getterName = "GetVarType", .bUsePimpl = false },
                { .type = "std::vector<std::string>", .memberVarName = "varNames", .getterName = "GetVarNames", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // StructMemberDeclSyntax
        VariantInfo {
            .name = "StructMemberDeclSyntax",
            .argName = "decl",
            .memberNames {
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
            .name = "StructDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams", .bUsePimpl = false },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "baseTypes", .getterName = "GetBaseTypes", .bUsePimpl = false },
                { .type = "std::vector<StructMemberDeclSyntax>", .memberVarName = "memberDecls", .getterName = "GetMemberDecls", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // EnumElemMemberVarDeclSyntax
        ClassInfo {
            .name = "EnumElemMemberVarDeclSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // EnumElemDeclSyntax
        ClassInfo {
            .name = "EnumElemDeclSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<EnumElemMemberVarDeclSyntax>", .memberVarName = "memberVars", .getterName = "GetMemberVars", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // EnumDeclSyntax
        ClassInfo {
            .name = "EnumDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier", .bUsePimpl = false },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams", .bUsePimpl = false },
                { .type = "std::vector<EnumElemDeclSyntax>", .memberVarName = "elements", .getterName = "GetElements", .bUsePimpl = false },
            },
            .bDefaultsInline = true
        },

        // NamespaceDeclSyntaxElement
        VariantInfo {
            .name = "NamespaceDeclSyntaxElement",
            .argName = "elem",
            .memberNames {
                "GlobalFuncDeclSyntax",
                "class NamespaceDeclSyntax", // forward declaration
                "ClassDeclSyntax",
                "StructDeclSyntax",
                "EnumDeclSyntax",
            }
        },

        // NamespaceDeclSyntax
        ClassInfo {
            .name = "NamespaceDeclSyntax",
            .memberInfos {
                { .type = "std::vector<std::string>", .memberVarName = "names", .getterName = "GetNames", .bUsePimpl = false },
                { .type = "std::vector<NamespaceDeclSyntaxElement>", .memberVarName = "elements", .getterName = "GetElements", .bUsePimpl = false }
            },
            .bDefaultsInline = false
        },

        // ScriptSyntaxElement
        VariantInfo {
            .name = "ScriptSyntaxElement",
            .argName = "elem",
            .memberNames {
                "NamespaceDeclSyntax",
                "GlobalFuncDeclSyntax",
                "ClassDeclSyntax",
                "StructDeclSyntax",
                "EnumDeclSyntax",
            }
        },

        // Script
        ClassInfo {
            .name = "ScriptSyntax",
            .memberInfos {
                { .type = "std::vector<ScriptSyntaxElement>", .memberVarName = "elements", .getterName = "GetElements", .bUsePimpl = false },
            },
            .bDefaultsInline = false
        },
    };

    GenerateItems(commonInfo, hStream, cppStream, itemInfos);

    // footer(close namespaces)
    hStream << endl << '}' << endl;
    cppStream << '}' << endl;

    WriteAll(hPath, hStream.str());
    WriteAll(cppPath, cppStream.str());
}
