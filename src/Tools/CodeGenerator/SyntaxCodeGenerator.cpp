#include "Misc.h"
#include <iostream>
#include <fmt/core.h>

using namespace std;
using namespace std::filesystem;

void GenerateSyntax(path srcPath)
{
    // [src]/../include/Syntax/Syntaxes.g.h
    // [src]/Syntax/Syntaxes.g.cpp
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
class ArgumentSyntax;

)---";

    cppStream << R"---(#include "pch.h"

#include <Syntax/Syntaxes.g.h>
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

        VariantInfo {
            .name = "StmtSyntax",
            .argName = "stmt",
            .memberNames {
                "class CommandStmtSyntax",
                "class VarDeclStmtSyntax",
                "std::unique_ptr<class IfStmtSyntax>",
                "std::unique_ptr<class IfTestStmtSyntax>",
                "std::unique_ptr<class ForStmtSyntax>",

                "class ContinueStmtSyntax",
                "class BreakStmtSyntax",
                "std::unique_ptr<class ReturnStmtSyntax>",
                "class BlockStmtSyntax",
                "class BlankStmtSyntax",
                "std::unique_ptr<class ExpStmtSyntax>",

                "class TaskStmtSyntax",
                "class AwaitStmtSyntax",
                "class AsyncStmtSyntax",
                "std::unique_ptr<class ForeachStmtSyntax>",
                "std::unique_ptr<class YieldStmtSyntax>",

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
                "std::unique_ptr<class BinaryOpExpSyntax>",
                "std::unique_ptr<class UnaryOpExpSyntax>",
                "std::unique_ptr<class CallExpSyntax>",
                "std::unique_ptr<class LambdaExpSyntax>",
                "std::unique_ptr<class IndexerExpSyntax>",
                "std::unique_ptr<class MemberExpSyntax>",
                "std::unique_ptr<class IndirectMemberExpSyntax>",
                "class ListExpSyntax",
                "class NewExpSyntax",
                "std::unique_ptr<class BoxExpSyntax>",
                "std::unique_ptr<class IsExpSyntax>",
                "std::unique_ptr<class AsExpSyntax>"
            }
        },

        VariantInfo {
            .name = "TypeExpSyntax",
            .argName = "typeExp",
            .memberNames {
                "class IdTypeExpSyntax",
                "std::unique_ptr<class MemberTypeExpSyntax>",
                "std::unique_ptr<class NullableTypeExpSyntax>",
                "std::unique_ptr<class LocalPtrTypeExpSyntax>",
                "std::unique_ptr<class BoxPtrTypeExpSyntax>",
                "std::unique_ptr<class LocalTypeExpSyntax>"
            }
        },


        ClassInfo {
            .name = "IdTypeExpSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API IdTypeExpSyntax(std::string name);"
            }
        },

        // MemberTypeExpSyntax(TypeExpSyntax typeExp, std::string name, std::vector<TypeExpSyntax> typeArgs);
        ClassInfo {
            .name = "MemberTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "parentType", .getterName = "GetParentType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
        },

        // NullableTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            .name = "NullableTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // LocalPtrTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            .name = "LocalPtrTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // BoxPtrTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            .name = "BoxPtrTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        // LocalTypeExpSyntax(TypeExpSyntax typeExp)
        ClassInfo {
            .name = "LocalTypeExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "innerType", .getterName = "GetInnerType" },
            },
        },

        ClassInfo {
            .name = "LambdaExpParamSyntax",
            .memberInfos {
                { .type = "std::optional<TypeExpSyntax>", .memberVarName = "type", .getterName = "GetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "bool", .memberVarName = "hasOut", .getterName = "HasOut" },
                { .type = "bool", .memberVarName = "hasParams", .getterName = "HasParams" },
            },
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
            .name = "IdentifierExpSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "value", .getterName = "GetValue" },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "typeArgs", .getterName = "GetTypeArgs" },
            },
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
                "std::unique_ptr<class ExpStringExpSyntaxElement>"
            }
        },

        ClassInfo {
            .name = "TextStringExpSyntaxElement",
            .memberInfos {
                { .type = "std::string", .memberVarName = "text", .getterName = "GetText" },
            },
        },

        ClassInfo {
            .name = "StringExpSyntax",
            .memberInfos {
                { .type = "std::vector<StringExpSyntaxElement>", .memberVarName = "elements", .getterName = "GetElements" },
            },
            .extraConstructors {
                "SYNTAX_API StringExpSyntax(std::string str);"
            }
        },

        ClassInfo {
            .name = "IntLiteralExpSyntax",
            .memberInfos {
                { .type = "int", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        ClassInfo {
            .name = "BoolLiteralExpSyntax",
            .memberInfos {
                { .type = "bool", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        ClassInfo {
            .name = "NullLiteralExpSyntax",
            .memberInfos { },
        },

        ClassInfo {
            .name = "ListExpSyntax",
            .memberInfos {
                    { .type = "std::vector<ExpSyntax>", .memberVarName = "elements", .getterName = "GetElements" },
                },
            },

        ClassInfo {
            .name = "NewExpSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType" },
                { .type = "std::vector<ArgumentSyntax>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        // 여기서부터 ExpSyntax가 complete

        ClassInfo {
            .name = "BinaryOpExpSyntax",
            .memberInfos {
                { .type = "BinaryOpSyntaxKind", .memberVarName = "kind", .getterName = "GetKind" },
                { .type = "ExpSyntax", .memberVarName = "operand0", .getterName = "GetOperand0" },
                { .type = "ExpSyntax", .memberVarName = "operand1", .getterName = "GetOperand1" },
            },
        },

        ClassInfo {
            .name = "UnaryOpExpSyntax",
            .memberInfos {
                { .type = "UnaryOpSyntaxKind", .memberVarName = "kind", .getterName = "GetKind" },
                { .type = "ExpSyntax", .memberVarName = "operand", .getterName = "GetOperand" },
            },
        },

        ClassInfo {
            .name = "CallExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "callable", .getterName = "GetCallable" },
                { .type = "std::vector<ArgumentSyntax>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        VariantInfo {
            .name = "LambdaExpBodySyntax",
            .argName = "body",
            .memberNames {
                "class StmtsLambdaExpBodySyntax",
                "std::unique_ptr<class ExpLambdaExpBodySyntax>"
            }
        },

        ClassInfo {
            .name = "StmtsLambdaExpBodySyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "stmts", .getterName = "GetStmts" }
            },
        },

        ClassInfo {
            .name = "ExpLambdaExpBodySyntax",
            .memberInfos = {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp" }
            },
        },

        ClassInfo {
            .name = "LambdaExpSyntax",
            .memberInfos {
                { .type = "std::vector<LambdaExpParamSyntax>", .memberVarName = "params", .getterName = "GetParams" },
                { .type = "LambdaExpBodySyntax", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        ClassInfo {
            .name = "IndexerExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "obj", .getterName = "GetObject" },
                { .type = "ExpSyntax", .memberVarName = "index", .getterName = "GetIndex" },
            },
        },

        ClassInfo {
            .name = "MemberExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "parent", .getterName = "GetParent" },
                { .type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName" },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API MemberExpSyntax(ExpSyntax parent, std::string memberName);"
            }
        },

        ClassInfo {
            .name = "IndirectMemberExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "parent", .getterName = "GetParent" },
                { .type = "std::string", .memberVarName = "memberName", .getterName = "GetMemberName" },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "memberTypeArgs", .getterName = "GetMemberTypeArgs" },
            },
            .extraConstructors {
                "SYNTAX_API IndirectMemberExpSyntax(ExpSyntax parent, std::string memberName);"
            }
        },


        ClassInfo {
            .name = "BoxExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "innerExp", .getterName = "GetInnerExp" },
            },
        },

        ClassInfo{
            .name = "IsExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp" },
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType" },
            },
        },

        ClassInfo {
            .name = "AsExpSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp" },
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType" },
            },
        },

        // ExpSyntax가 complete type일때까지 선언을 하면 안된다
        ClassInfo {
            .name = "ArgumentSyntax",
            .memberInfos {
                    { .type = "bool", .memberVarName = "bOut", .getterName = "HasOut" },
                    { .type = "bool", .memberVarName = "bParams", .getterName = "GetParams" },
                    { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp" },
                },
                .extraConstructors {
                    "SYNTAX_API ArgumentSyntax(ExpSyntax exp);"
                }
        },

        // ExpSyntax가 complete type일때까지 선언을 하면 안된다
        ClassInfo {
            .name = "ExpStringExpSyntaxElement",
            .memberInfos {
                    { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp" },
                },
            },

        // Embeddable 
        VariantInfo {
            .name = "EmbeddableStmtSyntax",
            .argName = "embeddableStmt",
            .memberNames {
                "std::unique_ptr<class SingleEmbeddableStmtSyntax>",
                "class BlockEmbeddableStmtSyntax",
            }
        },

        // BlockEmbeddableStmtSyntax(std::vector<StmtSyntax> stmts)
        ClassInfo {
            .name = "BlockEmbeddableStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "stmts", .getterName = "GetStmts" },
            },
        },

        // VarDeclSyntax
        ClassInfo {
            .name = "VarDeclSyntaxElement",
            .memberInfos {
                { .type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                { .type = "std::optional<ExpSyntax>", .memberVarName = "initExp", .getterName = "GetInitExp" }
            },
        },

        ClassInfo {
            .name = "VarDeclSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType" },
                { .type = "std::vector<VarDeclSyntaxElement>", .memberVarName = "elements", .getterName = "GetElements" }
            },
        },

        // Stmt

        // CommandStmtSyntax(std::vector<StringExpSyntax> commands)
        ClassInfo {
            .name = "CommandStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StringExpSyntax>", .memberVarName = "commands", .getterName = "GetCommands" },
            },
        },

        // VarDeclStmtSyntax(VarDeclSyntax varDecl)
        ClassInfo {
            .name = "VarDeclStmtSyntax",
            .memberInfos {
                { .type = "VarDeclSyntax", .memberVarName = "varDecl", .getterName = "GetVarDecl" },
            },
        },

        // ContinueStmtSyntax
        ClassInfo {
           .name = "ContinueStmtSyntax",
           .memberInfos {},
        },

        // BreakStmtSyntax
        ClassInfo {
           .name = "BreakStmtSyntax",
           .memberInfos {},
        },

        // BlockStmtSyntax(std::vector<StmtSyntax> stmts)
        ClassInfo {
            .name = "BlockStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "stmts", .getterName = "GetStmts" },
            },
        },

        // BlankStmtSyntax
        ClassInfo {
            .name = "BlankStmtSyntax",
            .memberInfos {},
        },

        // TaskStmtSyntax(std::vector<StmtSyntax> body)
        ClassInfo {
            .name = "TaskStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // AwaitStmtSyntax(std::vector<StmtSyntax> body);
        ClassInfo {
            .name = "AwaitStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // AsyncStmtSyntax(std::vector<StmtSyntax> body);
        ClassInfo {
            .name = "AsyncStmtSyntax",
            .memberInfos {
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // DirectiveStmtSyntax(std::u32string name, std::vector<ExpSyntax> args)
        ClassInfo {
            .name = "DirectiveStmtSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<ExpSyntax>", .memberVarName = "args", .getterName = "GetArgs" },
            },
        },

        // 여기부터 StmtSyntax가 complete

        // SingleEmbeddableStmtSyntax(StmtSyntax stmt), StmtSyntax에 depends
        ClassInfo {
            .name = "SingleEmbeddableStmtSyntax",
            .memberInfos {
                { .type = "StmtSyntax", .memberVarName = "stmt", .getterName = "GetStmt" },
            },
        },

        // IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody)
        ClassInfo {
            .name = "IfStmtSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "cond", .getterName = "GetCond" },
                { .type = "EmbeddableStmtSyntax", .memberVarName = "body", .getterName = "GetBody" },
                { .type = "std::optional<EmbeddableStmtSyntax>", .memberVarName = "elseBody", .getterName = "GetElseBody" },
            },
        },

        // IfTestStmtSyntax(TypeExpSyntax testTypeExp, std::string varName, ExpSyntax exp, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
        ClassInfo {
            .name = "IfTestStmtSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "testType", .getterName = "GetTestType" },
                { .type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp" },
                { .type = "EmbeddableStmtSyntax", .memberVarName = "body", .getterName = "GetBody" },
                { .type = "std::optional<EmbeddableStmtSyntax>", .memberVarName = "elseBody", .getterName = "GetElseBody" },
            },
        },

        // 
        VariantInfo {
            .name = "ForStmtInitializerSyntax",
            .argName = "forInit",
            .memberNames {
                "std::unique_ptr<class ExpForStmtInitializerSyntax>",
                "std::unique_ptr<class VarDeclForStmtInitializerSyntax>"
            }
        },

        ClassInfo {
            .name = "ExpForStmtInitializerSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp" }
            },
        },

        ClassInfo {
            .name = "VarDeclForStmtInitializerSyntax",
            .memberInfos {
                { .type = "VarDeclSyntax", .memberVarName = "varDecl", .getterName = "GetVarDecl" }
            },
        },

        // ForStmtSyntax(std::optional<ForStmtInitializerSyntax> initializer, 
        //      std::optional<ExpSyntax> condExp, 
        //      std::optional<ExpSyntax> continueExp, 
        //      EmbeddableStmtSyntax body);
        ClassInfo {
            .name = "ForStmtSyntax",
            .memberInfos {
                { .type = "std::optional<ForStmtInitializerSyntax>", .memberVarName = "initializer", .getterName = "GetInitializer" },
                { .type = "std::optional<ExpSyntax>", .memberVarName = "cond", .getterName = "GetCond" },
                { .type = "std::optional<ExpSyntax>", .memberVarName = "cont", .getterName = "GetCont" },
                { .type = "EmbeddableStmtSyntax", .memberVarName = "body", .getterName = "GetBody" },
            },
        },


        ClassInfo {
            .name = "ReturnStmtSyntax",
            .memberInfos {
                { .type = "std::optional<ExpSyntax>", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        // ExpStmtSyntax(ExpSyntax exp)
        ClassInfo {
            .name = "ExpStmtSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "exp", .getterName = "GetExp" },
            },
        },

        // ForeachStmtSyntax(TypeExpSyntax type, std::u32string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body);
        ClassInfo {
            .name = "ForeachStmtSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType" },
                { .type = "std::string", .memberVarName = "varName", .getterName = "GetVarName" },
                { .type = "ExpSyntax", .memberVarName = "enumerable", .getterName = "GetEnumerable" },
                { .type = "EmbeddableStmtSyntax", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // YieldStmtSyntax(ExpSyntax value)
        ClassInfo {
            .name = "YieldStmtSyntax",
            .memberInfos {
                { .type = "ExpSyntax", .memberVarName = "value", .getterName = "GetValue" },
            },
        },

        // TypeParamSyntax
        ClassInfo {
            .name = "TypeParamSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
            },
        },

        // FuncParamSyntax
        ClassInfo {
            .name = "FuncParamSyntax",
            .memberInfos {
                { .type = "bool", .memberVarName = "hasOut", .getterName = "HasOut" },
                { .type = "bool", .memberVarName = "hasParams", .getterName = "HasParams" },
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" }
            },
        },

        // GlobalFuncDeclSyntax
        ClassInfo {
            .name = "GlobalFuncDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence" }, // seq 함수인가        
                { .type = "TypeExpSyntax", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // ClassMemberFuncDeclSyntax
        ClassInfo {
            .name = "ClassMemberFuncDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "bool", .memberVarName = "bStatic", .getterName = "IsStatic" },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence" },
                { .type = "TypeExpSyntax", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // ClassConstructorDeclSyntax
        ClassInfo {
            .name = "ClassConstructorDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::optional<std::vector<ArgumentSyntax>>", .memberVarName = "baseArgs", .getterName = "GetBaseArgs" },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // ClassMemberVarDeclSyntax
        ClassInfo {
            .name = "ClassMemberVarDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "TypeExpSyntax", .memberVarName = "varType", .getterName = "GetVarType" },
                { .type = "std::vector<std::string>", .memberVarName = "varNames", .getterName = "GetVarNames" },
            },
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
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "baseTypes", .getterName = "GetBaseTypes" },
                { .type = "std::vector<ClassMemberDeclSyntax>", .memberVarName = "memberDecls", .getterName = "GetMemberDecls" },
            },
        },

        // StructMemberFuncDeclSyntax
        ClassInfo {
            .name = "StructMemberFuncDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAcessModifier" },
                { .type = "bool", .memberVarName = "bStatic", .getterName = "IsStatic" },
                { .type = "bool", .memberVarName = "bSequence", .getterName = "IsSequence" }, // seq 함수인가  
                { .type = "TypeExpSyntax", .memberVarName = "retType", .getterName = "GetRetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // StructConstructorDeclSyntax
        ClassInfo {
            .name = "StructConstructorDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<FuncParamSyntax>", .memberVarName = "parameters", .getterName = "GetParameters" },
                { .type = "std::vector<StmtSyntax>", .memberVarName = "body", .getterName = "GetBody" },
            },
        },

        // StructMemberVarDeclSyntax
        ClassInfo {
            .name = "StructMemberVarDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "TypeExpSyntax", .memberVarName = "varType", .getterName = "GetVarType" },
                { .type = "std::vector<std::string>", .memberVarName = "varNames", .getterName = "GetVarNames" },
            },
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
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<TypeExpSyntax>", .memberVarName = "baseTypes", .getterName = "GetBaseTypes" },
                { .type = "std::vector<StructMemberDeclSyntax>", .memberVarName = "memberDecls", .getterName = "GetMemberDecls" },
            },
        },

        // EnumElemMemberVarDeclSyntax
        ClassInfo {
            .name = "EnumElemMemberVarDeclSyntax",
            .memberInfos {
                { .type = "TypeExpSyntax", .memberVarName = "type", .getterName = "GetType" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
            },
        },

        // EnumElemDeclSyntax
        ClassInfo {
            .name = "EnumElemDeclSyntax",
            .memberInfos {
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<EnumElemMemberVarDeclSyntax>", .memberVarName = "memberVars", .getterName = "GetMemberVars" },
            },
        },

        // EnumDeclSyntax
        ClassInfo {
            .name = "EnumDeclSyntax",
            .memberInfos {
                { .type = "std::optional<AccessModifierSyntax>", .memberVarName = "accessModifier", .getterName = "GetAccessModifier" },
                { .type = "std::string", .memberVarName = "name", .getterName = "GetName" },
                { .type = "std::vector<TypeParamSyntax>", .memberVarName = "typeParams", .getterName = "GetTypeParams" },
                { .type = "std::vector<EnumElemDeclSyntax>", .memberVarName = "elements", .getterName = "GetElements" },
            },
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
                { .type = "std::vector<std::string>", .memberVarName = "names", .getterName = "GetNames" },
                { .type = "std::vector<NamespaceDeclSyntaxElement>", .memberVarName = "elements", .getterName = "GetElements" }
            },
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
                { .type = "std::vector<ScriptSyntaxElement>", .memberVarName = "elements", .getterName = "GetElements" },
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
