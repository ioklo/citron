#include "VisitorGenerator.h"
#include <format>
#include "Misc.h"

using namespace std;
using path = std::filesystem::path;

namespace Citron {

// visitor는 
struct VisitorInfo
{
    path relPath; // MIR/Public/MExp/MExp.g.h
    string name; // MExp
    string memberName; // mExp
    vector<string> members; // MExp_IntLiteral, ...
};

void GenerateVisitor(std::filesystem::path rootPath, VisitorInfo& info)
{
    path hPath = rootPath / info.relPath;
    ostringstream oss;

    string convertibleToResultType = format("{}ConvertibleToResultType", info.name);
    string visitable = format("{}Visitable", info.name);
    string visitor = format("{}Visitor", info.name);

    string text_1 = format(R"---(#pragma once

#include <optional>

namespace Citron {{
struct {}
{{
    virtual ~{}() {{}}
)---", visitor, visitor);

    oss.str("");
    for (auto& member : info.members)
    {
        oss << format(R"---(    virtual void Visit({}* {}) = 0;
)---", member, info.memberName);
    }

    string generalVisitorsText = oss.str();

    string text0 = format(R"---(}};

template<class TFrom, class TVisitor>
concept {} = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept {} = requires(TVisitor&& v, TVisitorArgs&&... args)
{{
    typename std::remove_cvref_t<TVisitor>::ResultType;
)---", convertibleToResultType, visitable);

    oss.str("");
    for (auto& member : info.members)
    {
        oss << format(R"---(    {{ v.Visit(std::declval<{}*>(), std::forward<TVisitorArgs>(args)...) }} -> {}<TVisitor>;
)---", member, convertibleToResultType);
    }
    string conceptsText = oss.str();

    string text1 = format(R"---(
}};

template<typename TVisitor, typename... TVisitorArgs> requires {}<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, {}* {}, TVisitorArgs&&... args)
{{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) {{ return v.Visit(e, std::forward<TVisitorArgs>(args)...); }};

    if constexpr (std::is_void_v<TResult>)
    {{
        struct Bridge : {} {{
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {{}})---", visitable, info.name, info.memberName, visitor);

    oss.str("");
    for (auto& member : info.members)
    {
        oss << format(R"---(            void Visit({}* {}) override {{ call({}); }}
)---", member, info.memberName, info.memberName);
    }
    string voidVisitorsText = oss.str();

    string text2 = format(R"---(        }};

        Bridge bridge{{caller}};
        {}->Accept(bridge);
    }}
    else
    {{
        struct Bridge : {} {{
            decltype(caller)& call;
            std::optional<TResult> result{{}};
            Bridge(decltype(caller)& call) : call(call) {{}})---", info.memberName, visitor);

    oss.str("");
    for (auto& member : info.members)
    {
        oss << format(R"---(            void Visit({}* {}) override {{ result.emplace(call({})); }}
)---", member, info.memberName, info.memberName);
    }
    string nonVoidVisitorsText = oss.str();

    string text3 = format(R"---(        }};

        Bridge bridge{{caller}};
        {}->Accept(bridge);
        return *bridge.result;
    }}
}}

)---", info.memberName);
    
    string text4 = R"---(} // namespace Citron)---";

    oss.str("");
    oss << text_1
        << generalVisitorsText
        << text0
        << conceptsText
        << text1
        << voidVisitorsText
        << text2
        << nonVoidVisitorsText
        << text3
        << text4;

    WriteAll(hPath, oss.str());
}

void GenerateVisitors(std::filesystem::path rootPath)
{
    vector<VisitorInfo> visitorInfos = {
        VisitorInfo {
            .relPath = path("src") / "MIR" / "Public" / "MIR" / "MExpVisitor.g.h",
            .name = "MExp",
            .memberName = "mExp",
            .members {
                "MExp_Load",
                "MExp_Store",
                "MExp_Stmt",
                "MExp_PtrRef",
                "MExp_BoolLiteral",
                "MExp_IntLiteral",
                "MExp_CallIntrinsic",
                "MExp_Call",
                "MExp_NewStruct",
                "MExp_NewEnumElem",
                "MExp_Nullable",
                "MExp_NullableNullLiteral",
                "MExp_Cast",
                "MExp_Lambda",
                "MExp_InlineBlock",
                "MExp_Is",
            },
        }, 

        VisitorInfo {
            .relPath = path("src") / "MIR" / "Public" / "MIR" / "MInitExpVisitor.g.h",
            .name = "MInitExp",
            .memberName = "mInitExp",
            .members {
                "MInitExp_Shared",
                "MInitExp_SharedRef",
                "MInitExp_Stmt",
                "MInitExp_String",
                "MInitExp_List",
                "MInitExp_CallIntrinsic",
                "MInitExp_NewClass",
                "MInitExp_StructCtor",
                "MInitExp_Call",
                "MInitExp_NewEnumElem",
                "MInitExp_Nullable",
                "MInitExp_NullableNullLiteral",
                "MInitExp_NullableInplaceNullLiteral",
                "MInitExp_Cast",
                "MInitExp_Lambda",
                "MInitExp_InlineBlock",
                "MInitExp_As",
            },
        },

        VisitorInfo {
            .relPath = path("src") / "MIR" / "Public" / "MIR" / "MLocVisitor.g.h",
            .name = "MLoc",
            .memberName = "mLoc",
            .members {
                "MLoc_Materialize",
                "MLoc_LocalVar",
                "MLoc_LocalRef",
                "MLoc_LambdaVar",
                "MLoc_ListIndexer",
                "MLoc_StructVar",
                "MLoc_ClassVar",
                "MLoc_EnumElemVar",
                "MLoc_This",
                "MLoc_PtrDeref",
                "MLoc_SharedDeref",
                "MLoc_NullableValue",
            },
        },

        VisitorInfo {
            .relPath = path("src") / "MIR" / "Public" / "MIR" / "MStmtVisitor.g.h",
            .name = "MStmt",
            .memberName = "mStmt",
            .members {
                "MStmt_Scope",
                "MStmt_Command",
                "MStmt_LocalVarDecl",
                "MStmt_LocalRefDecl",
                "MStmt_If",
                "MStmt_For",
                "MStmt_While",
                "MStmt_Continue",
                "MStmt_Break",
                "MStmt_Leave",
                "MStmt_Return",
                "MStmt_Blank",
                "MStmt_Exp",
                "MStmt_Task",
                "MStmt_Await",
                "MStmt_Async",
                "MStmt_Foreach",
                "MStmt_Yield",
                "MStmt_Directive",
                "MStmt_Call",
                "MStmt_Assign",
                "MStmt_Do",
            },
        },

        VisitorInfo{
            .relPath = path("src") / "MIR" / "Public" / "MIR" / "MSharedExpVisitor.g.h",
            .name = "MSharedExp",
            .memberName = "sharedExp",
            .members {
                "MSharedExp_Static",
                "MSharedExp_ClassVar",
                "MSharedExp_SharedStructVar",
            },
        },

        VisitorInfo {
            .relPath = path("src") / "NSymbol" / "Public" / "NSymbol" / "NFuncDeclOuterVisitor.g.h",
            .name = "NFuncDeclOuter",
            .memberName = "outer",
            .members {
                "NNamespaceDecl",
                "NGlobalFuncDecl",
                "NClassDecl",
                "NClassCtorDecl",
                "NClassFuncDecl",
                "NStructDecl",
                "NStructCtorDecl",
                "NStructDtorDecl",
                "NStructFuncDecl",
                "NLambdaDecl",
            },
        },

        VisitorInfo {
            .relPath = path("src") / "RSymbol" / "Public" / "RSymbol" / "RFuncDeclVisitor.g.h",
            .name = "RFuncDecl",
            .memberName = "rFuncDecl",
            .members {
                "RGlobalFuncDecl",
                "RClassCtorDecl",
                "RClassFuncDecl",
                "RStructCtorDecl",
                "RStructDtorDecl",
                "RStructFuncDecl",
                "RLambdaDecl",
            }
        },

        VisitorInfo {
            .relPath = path("src") / "RSymbol" / "Public" / "RSymbol" / "RTypeDeclVisitor.g.h",
            .name = "RTypeDecl",
            .memberName = "rTypeDecl",
            .members {
                "RClassDecl",
                "RStructDecl",
                "REnumDecl",
                "REnumElemDecl",
                "RInterfaceDecl",
                "RLambdaDecl",
                "RTypeParamDecl",
            },
        },

        VisitorInfo{
            .relPath = path("src") / "RSymbol" / "Public" / "RSymbol" / "RTypeVisitor.g.h",
            .name = "RType",
            .memberName = "rType",
            .members = {
                "RType_Nullable",
                "RType_NullableInplace",
                "RType_TypeVar",
                "RType_Void",
                "RType_Primitive",
                "RType_Tuple",
                "RType_Func",
                "RType_Ptr",
                "RType_Shared",
                "RType_Box",
                "RType_Class",
                "RType_Struct",
                "RType_Enum",
                "RType_EnumElem",
                "RType_Interface",
                "RType_Lambda",
            },
        },

        VisitorInfo {
            .relPath = path("src") / "SyntaxIR0Translator" / "ImExpVisitor.g.h",
            .name = "ImExp",
            .memberName = "imExp",
            .members {
                "ImExp_Namespace",
                "ImExp_GlobalFuncs",
                "ImExp_TypeVar",
                "ImExp_Class",
                "ImExp_ClassFuncs",
                "ImExp_Struct",
                "ImExp_StructFuncs",
                "ImExp_Enum",
                "ImExp_EnumElem",
                "ImExp_ClassVar",
                "ImExp_StructVar",
                "ImExp_ReExp",
            },
        },

        VisitorInfo {
            .relPath = path("src") / "SyntaxIR0Translator" / "IrExpVisitor.g.h",
            .name = "IrExp",
            .memberName = "irExp",
            .members {
                "IrExp_Namespace",
                "IrExp_Class",
                "IrExp_Struct",
                "IrExp_Static",
                "IrExp_ClassVar",
                "IrExp_SharedStructVar",
                "IrExp_StructVar",
                "IrExp_SharedDeref",
                "IrExp_Loc",
            },
        },

    };

    for (auto& visitorInfo : visitorInfos)
        GenerateVisitor(rootPath, visitorInfo);
}

} // namespace Citron