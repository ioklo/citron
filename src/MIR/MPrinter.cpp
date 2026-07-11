#include "MPrinter.h"

#include <format>
#include <string>
#include <variant>
#include <span>

#include "Infra/Exceptions.h"
#include "Infra/Ref.h"
#include "Infra/IWriter.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "RSymbol/RLambdaDecl.h"
#include "RSymbol/RLambdaVarDecl.h"
#include "MData.h"
#include "MStmt.h"
#include "MExp.h"
#include "MInitExp.h"
#include "MLoc.h"
#include "MSharedExp.h"

using namespace std;

namespace Citron {
namespace {

class MPrinterImpl {
    IWriter& writer;
    RFactory& rFactory;

public:
    MPrinterImpl(InRef<IWriter> writer, InRef<RFactory> rFactory) : writer(*writer), rFactory(*rFactory) {}

    void PrintFuncBody(InRef<MFuncBody> funcBody)
    {
        writer.Write("Func ");
        PrintRName(funcBody->rFuncDecl->RFuncDecl_GetDecl()->GetIdentifier().name);
        writer.WriteLine();
        writer.AddIndent();
        PrintStmt(funcBody->body);
        writer.RemoveIndent();
    }

private:
    void PrintLine(InRef<string> line) { writer.Write(*line); writer.WriteLine(); }

    static string Join(InRef<vector<string>> items, string_view sep = ", ")
    {
        string s;
        for (size_t i = 0; i < items->size(); ++i)
        {
            if (i != 0) s += sep;
            s += (*items)[i];
        }
        return s;
    }

    static string Quote(string_view text)
    {
        return format("\"{}\"", text);
    }

    void PrintRName(InRef<RName> name)
    {
        writer.Write(RNameText(name));
    }

    string RNameText(InRef<RName> name)
    {
        return name->Visit([this](auto& name) -> string {
            using T = remove_cvref_t<decltype(name)>;

            if constexpr (same_as<T, RName_None>) return "<none>";
            else if constexpr (same_as<T, RName_Normal>) return name.text;
            else if constexpr (same_as<T, RName_Reserved>) return format("${}", RName_ReservedNameToString(name.name));
            else if constexpr (same_as<T, RName_CtorParam>) return format("$ctor.{}", name.index);
            else if constexpr (same_as<T, RName_Lambda>) return format("$lambda.{}", name.index);
            else static_assert(false);
        });
    }

    string TypeText(RType* type)
    {
        if (type == rFactory.MakeVoidType()) return "void";
        if (type == rFactory.MakeBoolType()) return "bool";
        if (type == rFactory.MakeIntType()) return "int";
        if (type == rFactory.MakeStringType()) return "string";
        if (type == rFactory.MakePtrType(rFactory.MakeVoidType())) return "ptr";
        return format("#{}", reinterpret_cast<uintptr_t>(type));
    }

    string DeclText(RDecl* decl)
    {
        if (!decl) return "<null-decl>";
        auto id = decl->GetIdentifier();
        return id.name.Visit([decl](auto& name) -> string {
            using T = remove_cvref_t<decltype(name)>;
            if constexpr (same_as<T, RName_Normal>) return name.text;
            else return format("decl#{}", reinterpret_cast<uintptr_t>(decl));
        });
    }

    string FuncDeclText(RFuncDecl* funcDecl)
    {
        return DeclText(funcDecl->RFuncDecl_GetDecl());
    }

    string LambdaDeclText(RLambdaDecl* decl)
    {
        if (!decl) return "<null-lambda-decl>";
        return DeclText(decl->RFuncDecl_GetDecl());
    }

    string ScopeKindText(InRef<MScopeKind> scopeKind)
    {
        return visit([](auto& kind) -> string {
            using T = remove_cvref_t<decltype(kind)>;
            if constexpr (same_as<T, MScopeKind_Default>) return "default";
            else if constexpr (same_as<T, MScopeKind_Loop>) return format("loop:{}", kind.labelId);
            else if constexpr (same_as<T, MScopeKind_Switch>) return format("switch:{}", kind.labelId);
            else return format("inline:{}", kind.labelId);
        }, *scopeKind);
    }

    string ReadText(InRef<MRead> read)
    {
        return visit([this](auto& read) -> string {
            using T = remove_cvref_t<decltype(read)>;
            if constexpr (same_as<T, MRead_Exp>) return format("read_exp({})", ExpText(read.exp));
            else return format("read_loc({})", LocText(read.loc));
        }, *read);
    }

    string TopLevelReadText(InRef<MTopLevel_Read> read)
    {
        return ReadText(read->read);
    }

    string CreateText(InRef<MCreate> create)
    {
        return visit([this](auto& create) -> string {
            using T = remove_cvref_t<decltype(create)>;
            if constexpr (same_as<T, MCreate_BC>) return format("create_bc({})", ExpText(create.exp));
            else return format("create_nbc({})", InitExpText(create.initExp));
        }, *create);
    }

    string TopLevelCreateText(InRef<MTopLevel_Create> create)
    {
        return CreateText(create->create);
    }

    string TopLevelLocText(InRef<MTopLevel_Loc> loc)
    {
        return LocText(loc->loc);
    }

    string TopLevelAssignText(InRef<MTopLevel_Assign> assign)
    {
        return visit([this, &assign](auto& assignKind) -> string {
            using T = remove_cvref_t<decltype(assignKind)>;
            if constexpr (same_as<T, MStmt_AssignKind_Copy>)
                return format("{} = {}", LocText(assign->dest), LocText(assignKind.src.loc));
            else if constexpr (same_as<T, MStmt_AssignKind_Move>)
                return format("{} = {}", LocText(assign->dest), MoveSourceText(assignKind.src));
            else static_assert(false);
        }, assign->kind);
    }

    string MoveSourceText(InRef<MMoveSource> src)
    {
        return visit([this](auto& src) -> string {
            using T = remove_cvref_t<decltype(src)>;
            if constexpr (same_as<T, MMoveSource_MovedLoc>) return format("move({})", LocText(src.loc));
            else return format("move_materialized({})", LocText(src.loc));
        }, *src);
    }

    string ArgumentText(InRef<MArgument> arg)
    {
        return visit([this](auto& arg) -> string {
            using T = remove_cvref_t<decltype(arg)>;
            if constexpr (same_as<T, MArgument_Create>) return CreateText(arg.create);
            else if constexpr (same_as<T, MArgument_Loc>) return LocText(arg.loc);
            else if constexpr (same_as<T, MArgument_Move>) return MoveSourceText(arg.src);
            else if constexpr (same_as<T, MArgument_Forward>) return visit([this](auto& x) -> string {
                using TF = remove_cvref_t<decltype(x)>;
                if constexpr (same_as<TF, MArgument_Forward_LValue>) return format("forward_lvalue({})", LocText(x.loc));
                else return format("forward_rvalue({})", MoveSourceText(x.src));
            }, arg);
            else return format("params({}, {})", ExpText(arg.exp), arg.elemCount);
        }, *arg);
    }

    string ArgsText(span<MArgument> args)
    {
        vector<string> items;
        items.reserve(args.size());
        for (auto& arg : args)
            items.push_back(ArgumentText(arg));
        return Join(items);
    }

    string CallableText(InRef<MCallable> callable)
    {
        if (callable->o_instance)
            return format("{}.{}", LocText(callable->o_instance), FuncDeclText(callable->decl));
        return FuncDeclText(callable->decl);
    }

    string TopLevelCallText(InRef<MTopLevel_Call> call)
    {
        auto text = format("{}({})", CallableText(call->callable), ArgsText(call->args));
        if (call->o_catch) text += format(" {}", CatchText(*call->o_catch));
        return text;
    }

    string PatternLeafText(InRef<MPatternLeaf> pattern)
    {
        return visit([this](auto& p) -> string {
            using T = remove_cvref_t<decltype(p)>;
            if constexpr (same_as<T, MPattern_Alias>) return RNameText(p.name);
            else return "_";
        }, *pattern);
    }

    string PatternText(InRef<MPattern> pattern)
    {
        return visit([this](auto& p) -> string { return PatternTextImpl(p); }, *pattern);
    }

    string TopLevelPatternText(InRef<MTopLevelPattern> pattern)
    {
        return visit([this](auto& p) -> string { return PatternTextImpl(p); }, *pattern);
    }

    string PatternTextImpl(InRef<MPattern_Alias> p) { return RNameText(p->name); }
    string PatternTextImpl(InRef<MPattern_Ignore>) { return "_"; }
    string PatternTextImpl(InRef<MPattern_Null>) { return "null"; }
    string PatternTextImpl(InRef<MPattern_Some> p) { return format("some {}", PatternLeafText(p->pattern)); }
    string PatternTextImpl(InRef<MPattern_Class> p) { return format("{} {}", TypeText(p->type), PatternLeafText(p->pattern)); }
    string PatternTextImpl(InRef<MPattern_EnumElem> p)
    {
        vector<string> elems;
        elems.reserve(p->patterns.size());
        for (auto& pattern : p->patterns)
            elems.push_back(PatternText(pattern));
        return format("{}({})", TypeText(p->type), Join(elems));
    }

    struct LocTextVisitor {
        using ResultType = string;
        MPrinterImpl& p;

        string Visit(MLoc_Materialize* loc) { return format("materialize({})", p.CreateText(loc->create)); }
        string Visit(MLoc_LocalVar* loc) { return format("local {}: {}", p.RNameText(loc->name), p.TypeText(loc->declType)); }
        string Visit(MLoc_LocalRef* loc) { return format("localref {}: {}", p.RNameText(loc->name), p.TypeText(loc->declType)); }
        string Visit(MLoc_LambdaVar* loc) { return format("lambda_var {}", p.DeclText(loc->decl)); }
        string Visit(MLoc_ListIndexer* loc) { return format("list_index({}, {}, item={})", p.LocText(loc->list.loc), p.ReadText(loc->index), p.TypeText(loc->itemType)); }
        string Visit(MLoc_StructVar* loc) { return format("struct_var({}, {})", loc->instance ? p.LocText(loc->instance) : string("<static>"), p.DeclText(loc->decl)); }
        string Visit(MLoc_ClassVar* loc) { return format("class_var({}, {})", loc->instance ? p.LocText(loc->instance) : string("<static>"), p.DeclText(loc->decl)); }
        string Visit(MLoc_EnumElemVar* loc) { return format("enum_elem_var({}, {})", loc->instance ? p.LocText(loc->instance) : string("<static>"), p.DeclText(loc->decl)); }
        string Visit(MLoc_This* loc) { return format("this:{}", p.TypeText(loc->type)); }
        string Visit(MLoc_PtrDeref* loc) { return format("ptr_deref({})", p.ReadText(loc->srcPtr)); }
        string Visit(MLoc_SharedDeref* loc) { return format("shared_deref({})", p.LocText(loc->srcShared.loc)); }
        string Visit(MLoc_NullableValue* loc) { return format("nullable_value({})", p.LocText(loc->loc)); }
    };

    string LocText(MLoc* loc)
    {
        return Accept(LocTextVisitor{*this}, loc);
    }

    string SharedSegmentsText(span<MSharedExpStructSegment> segments)
    {
        vector<string> items;
        items.reserve(segments.size());
        for (auto& segment : segments)
            items.push_back(DeclText(segment.decl));
        return Join(items, ".");
    }

    struct SharedExpTextVisitor {
        using ResultType = string;
        MPrinterImpl& p;

        string Visit(MSharedExp_Static* sharedExp)
        {
            auto suffix = sharedExp->segments.empty() ? string() : format(".{}", p.SharedSegmentsText(sharedExp->segments));
            return format("shared_static({}{})", p.LocText(sharedExp->loc), suffix);
        }

        string Visit(MSharedExp_ClassVar* sharedExp)
        {
            auto suffix = sharedExp->segments.empty() ? string() : format(".{}", p.SharedSegmentsText(sharedExp->segments));
            return format("shared_class_var({}.{}{})", p.LocText(sharedExp->base), p.DeclText(sharedExp->decl), suffix);
        }

        string Visit(MSharedExp_SharedStructVar* sharedExp)
        {
            auto suffix = sharedExp->segments.empty() ? string() : format(".{}", p.SharedSegmentsText(sharedExp->segments));
            return format("shared_struct_var({}.{}{})", p.LocText(sharedExp->base), p.DeclText(sharedExp->decl), suffix);
        }
    };

    string SharedExpText(MSharedExp* sharedExp)
    {
        return Accept(SharedExpTextVisitor{*this}, sharedExp);
    }

    string StringElemText(InRef<MInitExp_StringElem> elem)
    {
        return visit([this](auto& elem) -> string {
            using T = remove_cvref_t<decltype(elem)>;
            if constexpr (same_as<T, MInitExp_StringElem_Text>) return Quote(elem.text);
            else if constexpr (same_as<T, MInitExp_StringElem_InitExp>) return InitExpText(elem.initExp);
            else return LocText(elem.loc);
        }, *elem);
    }

    string StructCtorKindText(InRef<MInitExp_StructCtorKind> kind)
    {
        return visit([this](auto& kind) -> string {
            using T = remove_cvref_t<decltype(kind)>;
            if constexpr (same_as<T, MInitExp_StructCtorKind_Copy>)
                return format("struct_ctor_copy(type={}, src={})", TypeText(kind.structType), LocText(kind.src.loc));
            else if constexpr (same_as<T, MInitExp_StructCtorKind_Move>)
                return format("struct_ctor_move(type={}, src={})", TypeText(kind.structType), MoveSourceText(kind.src));
            else
                return format("struct_ctor_general({}, {})", DeclText(kind.decl), ArgsText(kind.args));
        }, *kind);
    }

    struct ExpTextVisitor {
        using ResultType = string;
        MPrinterImpl& p;

        string Visit(MExp_Load* exp) { return format("load({})", p.LocText(exp->loc)); }
        string Visit(MExp_Store* exp) { return format("store({}, {})", p.LocText(exp->dest), p.ReadText(exp->src)); }
        string Visit(MExp_Stmt* exp)
        {
            vector<string> stmts;
            stmts.reserve(exp->stmts.size());
            for (auto* stmt : exp->stmts)
                stmts.push_back(p.StmtInlineText(stmt));
            return format("stmt_exp(stmts=[{}], final={})", MPrinterImpl::Join(stmts), p.ExpText(exp->finalExp));
        }
        string Visit(MExp_PtrRef* exp) { return format("ptr_ref({})", p.LocText(exp->innerLoc)); }
        string Visit(MExp_BoolLiteral* exp) { return exp->value ? "true" : "false"; }
        string Visit(MExp_IntLiteral* exp) { return to_string(exp->value); }
        string Visit(MExp_CallIntrinsic* exp) { return format("intrinsic#{}({})", static_cast<int>(exp->kind), p.ArgsText(exp->args)); }
        string Visit(MExp_Call* exp)
        {
            auto text = format("{}({})", p.CallableText(exp->callable), p.ArgsText(exp->args));
            if (exp->o_catch) text += format(" {}", p.CatchText(*exp->o_catch));
            return text;
        }
        string Visit(MExp_NewStruct* exp) { return format("new_struct {}({})", p.DeclText(exp->ctor), p.ArgsText(exp->args)); }
        string Visit(MExp_NewEnumElem* exp) { return format("new_enum_elem {}({})", p.DeclText(exp->enumElemDecl), p.ArgsText(exp->args)); }
        string Visit(MExp_Nullable* exp) { return format("nullable({})", p.ExpText(exp->innerExp.exp)); }
        string Visit(MExp_NullableNullLiteral* exp) { return format("nullable_null({})", p.TypeText(exp->innerType)); }
        string Visit(MExp_Cast* exp) { return format("cast#{}({}, {})", static_cast<int>(exp->kind), p.ReadText(exp->src), p.TypeText(exp->targetType)); }
        string Visit(MExp_Lambda* exp) { return format("lambda {} captures({})", p.LambdaDeclText(exp->lambdaDecl), p.ArgsText(exp->args)); }
        string Visit(MExp_InlineBlock* exp) { return format("inline_block(return={})", p.TypeText(exp->returnType)); }
        string Visit(MExp_Is* exp) { return format("{} is {}", p.ReadText(exp->operand), p.TopLevelPatternText(exp->pattern)); }
    };

    string ExpText(MExp* exp)
    {
        return Accept(ExpTextVisitor{*this}, exp);
    }

    struct InitExpTextVisitor {
        using ResultType = string;
        MPrinterImpl& p;

        string Visit(MInitExp_Shared* exp) { return format("shared({})", p.CreateText(exp->create)); }
        string Visit(MInitExp_SharedRef* exp) { return format("shared_ref({})", p.SharedExpText(exp->sharedExp)); }
        string Visit(MInitExp_Stmt* exp)
        {
            vector<string> stmts;
            stmts.reserve(exp->stmts.size());
            for (auto* stmt : exp->stmts)
                stmts.push_back(p.StmtInlineText(stmt));
            return format("stmt_init(stmts=[{}], final={})", MPrinterImpl::Join(stmts), p.InitExpText(exp->finalExp));
        }
        string Visit(MInitExp_String* exp)
        {
            vector<string> elems;
            elems.reserve(exp->elements.size());
            for (auto& elem : exp->elements)
                elems.push_back(p.StringElemText(elem));
            return format("string([{}])", Join(elems));
        }
        string Visit(MInitExp_List* exp)
        {
            vector<string> elems;
            elems.reserve(exp->elems.size());
            for (auto& elem : exp->elems)
                elems.push_back(p.CreateText(elem));
            return format("list(item={}, [{}])", p.TypeText(exp->itemType), Join(elems));
        }
        string Visit(MInitExp_CallIntrinsic* exp) { return format("init_intrinsic#{}({})", static_cast<int>(exp->kind), p.ArgsText(exp->args)); }
        string Visit(MInitExp_NewClass* exp) { return format("new_class {}({})", p.DeclText(exp->ctorDecl), p.ArgsText(exp->args)); }
        string Visit(MInitExp_StructCtor* exp) { return p.StructCtorKindText(exp->kind); }
        string Visit(MInitExp_Call* exp)
        {
            auto text = format("{}({})", p.CallableText(exp->callable), p.ArgsText(exp->args));
            if (exp->o_catch) text += format(" {}", p.CatchText(*exp->o_catch));
            return text;
        }
        string Visit(MInitExp_NewEnumElem* exp) { return format("new_enum_elem {}({})", p.DeclText(exp->enumElemDecl), p.ArgsText(exp->args)); }
        string Visit(MInitExp_Nullable* exp) { return format("nullable({})", p.InitExpText(exp->inner.initExp)); }
        string Visit(MInitExp_NullableNullLiteral* exp) { return format("nullable_null({})", p.TypeText(exp->innerType)); }
        string Visit(MInitExp_NullableInplaceNullLiteral* exp) { return format("nullable_inplace_null({})", p.TypeText(exp->innerType)); }
        string Visit(MInitExp_Cast* exp) { return format("init_cast#{}({}, {})", static_cast<int>(exp->kind), p.ReadText(exp->src), p.TypeText(exp->targetType)); }
        string Visit(MInitExp_Lambda* exp) { return format("lambda {} captures({})", p.LambdaDeclText(exp->lambdaDecl), p.ArgsText(exp->args)); }
        string Visit(MInitExp_InlineBlock* exp) { return format("inline_block(return={})", p.TypeText(exp->returnType)); }
        string Visit(MInitExp_As* exp) { return format("as#{}({}, {})", static_cast<int>(exp->kind), p.ReadText(exp->target), p.TypeText(exp->type)); }
    };

    string InitExpText(MInitExp* exp)
    {
        return Accept(InitExpTextVisitor{*this}, exp);
    }

    void PrintStmtBlock(span<MStmt*> stmts)
    {
        writer.WriteLine();
        writer.AddIndent();
        for (auto* stmt : stmts) PrintStmt(stmt);
        writer.RemoveIndent();
    }

    string LocalVarDeclInitText(InRef<MStmt_LocalVarDeclInit> init)
    {
        return visit([this](auto& init) -> string {
            using T = remove_cvref_t<decltype(init)>;
            if constexpr (same_as<T, MStmt_LocalVarDeclInit_Uninit>) return "<uninit>";
            else return TopLevelCreateText(init.create);
        }, *init);
    }

    string CatchText(InRef<MCatch> mCatch)
    {
        return visit([this](auto& mCatch) -> string {
            using T = remove_cvref_t<decltype(mCatch)>;
            if constexpr (same_as<T, MCatch_Resume>) return format("catch_resume({})", TypeText(mCatch.errorType));
            else if constexpr (same_as<T, MCatch_Return>) return format("catch_return({})", TypeText(mCatch.errorType));
            else if constexpr (same_as<T, MCatch_Error>) return format("catch_error({})", TypeText(mCatch.errorType));
            else if constexpr (same_as<T, MCatch_Break>) return "catch_break";
            else return "catch_continue";
        }, *mCatch);
    }

    string DirectiveText(InRef<MDirective> directive)
    {
        return visit([this](auto& directive) -> string {
            using T = remove_cvref_t<decltype(directive)>;
            if constexpr (same_as<T, MDirective_NullDirective>) return format("null_directive({})", ReadText(directive.loc));
            else if constexpr (same_as<T, MDirective_NotNullDirective>) return format("not_null_directive({})", ReadText(directive.loc));
            else if constexpr (same_as<T, MDirective_StaticNullDirective>) return format("static_null_directive({})", ReadText(directive.loc));
            else if constexpr (same_as<T, MDirective_StaticNotNullDirective>) return format("static_not_null_directive({})", ReadText(directive.loc));
            else return format("static_unknown_directive({})", ReadText(directive.loc));
        }, *directive);
    }

    struct StmtInlineVisitor {
        using ResultType = string;
        MPrinterImpl& p;

        string Visit(MStmt_Scope* stmt) { return format("scope(kind={}, stmts={})", p.ScopeKindText(stmt->scopeKind), stmt->stmts.size()); }
        string Visit(MStmt_Command* stmt)
        {
            vector<string> cmds;
            cmds.reserve(stmt->command.commands.size());
            for (auto& command : stmt->command.commands)
                cmds.push_back(p.LocText(command.loc));
            return format("command {}", MPrinterImpl::Join(cmds));
        }
        string Visit(MStmt_LocalVarDecl* stmt)
        {
            return format("var {} {} = {}", p.TypeText(stmt->type), p.RNameText(stmt->name), p.LocalVarDeclInitText(stmt->init));
        }
        string Visit(MStmt_LocalRefDecl* stmt) { return format("ref {} {} = {}", p.TypeText(stmt->type), p.RNameText(stmt->name), p.TopLevelLocText(stmt->loc)); }
        string Visit(MStmt_If* stmt) { return format("if {}", p.TopLevelReadText(stmt->cond)); }
        string Visit(MStmt_For* stmt) { return format("for(cond={}, cont={}, body={})", stmt->o_cond ? p.TopLevelReadText(*stmt->o_cond) : string("<none>"), stmt->contStmt ? p.StmtInlineText(stmt->contStmt) : string("<none>"), stmt->body ? "scope" : "<null>"); }
        string Visit(MStmt_While* stmt) { return format("while({})", stmt->cond ? p.ReadText(*stmt->cond) : string("<none>")); }
        string Visit(MStmt_Continue* stmt) { return format("continue {}", stmt->labelId); }
        string Visit(MStmt_Break* stmt) { return format("break {}", stmt->labelId); }
        string Visit(MStmt_Leave* stmt) { return format("leave {} {}", stmt->labelId, p.TopLevelCreateText(stmt->create)); }
        string Visit(MStmt_Return* stmt) { return stmt->create ? format("return {}", p.TopLevelCreateText(*stmt->create)) : string("return"); }
        string Visit(MStmt_Blank*) { return "blank"; }
        string Visit(MStmt_Exp* stmt) { return p.TopLevelCreateText(stmt->create); }
        string Visit(MStmt_Task* stmt) { return format("task {} captures({})", p.LambdaDeclText(stmt->lambdaDecl), p.ArgsText(stmt->captureArgs)); }
        string Visit(MStmt_Await* stmt) { return format("await(body={})", stmt->body ? "scope" : "<null>"); }
        string Visit(MStmt_Async* stmt) { return format("async {} captures({})", p.LambdaDeclText(stmt->lambdaDecl), p.ArgsText(stmt->captureArgs)); }
        string Visit(MStmt_Foreach* stmt) { return format("foreach {} {} in {} => {} {}", p.TypeText(stmt->iterType), p.RNameText(stmt->iterName), p.TopLevelCreateText(stmt->iterCreate), p.TypeText(stmt->itemType), p.RNameText(stmt->itemName)); }
        string Visit(MStmt_Yield* stmt) { return format("yield {}", p.TopLevelCreateText(stmt->valueCreate)); }
        string Visit(MStmt_Directive* stmt) { return p.DirectiveText(stmt->directive); }
        string Visit(MStmt_Call* stmt) { return p.TopLevelCallText(stmt->call); }
        string Visit(MStmt_Assign* stmt) { return p.TopLevelAssignText(stmt->assign); }
        string Visit(MStmt_Do* stmt)
        {
            vector<string> catches;
            catches.reserve(stmt->catches.size());
            for (auto& c : stmt->catches)
                catches.push_back(p.CatchText(c));
            return format("do catches=[{}]", MPrinterImpl::Join(catches));
        }
    };

    string StmtInlineText(MStmt* stmt)
    {
        return Accept(StmtInlineVisitor{*this}, stmt);
    }

    struct StmtPrintVisitor {
        using ResultType = void;
        MPrinterImpl& p;

        void Visit(MStmt_Scope* stmt)
        {
            p.writer.Write(format("scope(kind={})", p.ScopeKindText(stmt->scopeKind)));
            p.PrintStmtBlock(stmt->stmts);
        }

        void Visit(MStmt_Command* stmt)
        {
            vector<string> cmds;
            cmds.reserve(stmt->command.commands.size());
            for (auto& command : stmt->command.commands)
                cmds.push_back(p.LocText(command.loc));
            p.PrintLine(format("command {}", MPrinterImpl::Join(cmds)));
        }

        void Visit(MStmt_LocalVarDecl* stmt)
        {
            p.PrintLine(format("var {} {} = {}", p.TypeText(stmt->type), p.RNameText(stmt->name), p.LocalVarDeclInitText(stmt->init)));
        }

        void Visit(MStmt_LocalRefDecl* stmt)
        {
            p.PrintLine(format("ref {} {} = {}", p.TypeText(stmt->type), p.RNameText(stmt->name), p.TopLevelLocText(stmt->loc)));
        }

        void Visit(MStmt_If* stmt)
        {
            p.writer.Write(format("if {}", p.TopLevelReadText(stmt->cond)));
            p.PrintStmt(stmt->trueBody);
            if (stmt->falseBody)
            {
                p.writer.Write("else");
                p.PrintStmt(stmt->falseBody);
            }
        }

        void Visit(MStmt_For* stmt)
        {
            p.writer.Write("for");
            p.writer.WriteLine();
            p.writer.AddIndent();
            p.PrintLine(stmt->o_cond ? format("cond: {}", p.TopLevelReadText(*stmt->o_cond)) : string("cond: <none>"));
            if (stmt->contStmt)
            {
                p.writer.Write("cont:");
                p.writer.WriteLine();
                p.writer.AddIndent();
                p.PrintStmt(stmt->contStmt);
                p.writer.RemoveIndent();
            }
            p.writer.Write("body:");
            p.PrintStmt(stmt->body);
            p.writer.RemoveIndent();
        }

        void Visit(MStmt_While* stmt)
        {
            p.writer.Write(format("while {}", stmt->cond ? p.ReadText(*stmt->cond) : string("<none>")));
            p.PrintStmt(stmt->body);
        }

        void Visit(MStmt_Continue* stmt) { p.PrintLine(format("continue {}", stmt->labelId)); }
        void Visit(MStmt_Break* stmt) { p.PrintLine(format("break {}", stmt->labelId)); }
        void Visit(MStmt_Leave* stmt) { p.PrintLine(format("leave {} {}", stmt->labelId, p.TopLevelCreateText(stmt->create))); }
        void Visit(MStmt_Return* stmt) { p.PrintLine(stmt->create ? format("return {}", p.TopLevelCreateText(*stmt->create)) : string("return")); }
        void Visit(MStmt_Blank*) { p.PrintLine(string("blank")); }
        void Visit(MStmt_Exp* stmt) { p.PrintLine(p.TopLevelCreateText(stmt->create)); }
        void Visit(MStmt_Task* stmt) { p.PrintLine(format("task {} captures({})", p.LambdaDeclText(stmt->lambdaDecl), p.ArgsText(stmt->captureArgs))); }

        void Visit(MStmt_Await* stmt)
        {
            p.writer.Write("await");
            p.PrintStmt(stmt->body);
        }

        void Visit(MStmt_Async* stmt) { p.PrintLine(format("async {} captures({})", p.LambdaDeclText(stmt->lambdaDecl), p.ArgsText(stmt->captureArgs))); }

        void Visit(MStmt_Foreach* stmt)
        {
            p.writer.Write(format("foreach {} {} in {} => {} {}", p.TypeText(stmt->iterType), p.RNameText(stmt->iterName), p.TopLevelCreateText(stmt->iterCreate), p.TypeText(stmt->itemType), p.RNameText(stmt->itemName)));
            p.PrintStmt(stmt->body);
        }

        void Visit(MStmt_Yield* stmt) { p.PrintLine(format("yield {}", p.TopLevelCreateText(stmt->valueCreate))); }
        void Visit(MStmt_Directive* stmt) { p.PrintLine(p.DirectiveText(stmt->directive)); }
        void Visit(MStmt_Call* stmt) { p.PrintLine(p.TopLevelCallText(stmt->call)); }
        void Visit(MStmt_Assign* stmt) { p.PrintLine(p.TopLevelAssignText(stmt->assign)); }

        void Visit(MStmt_Do* stmt)
        {
            p.writer.Write("do");
            p.PrintStmt(stmt->body);
            p.writer.AddIndent();
            for (auto& c : stmt->catches)
                p.PrintLine(p.CatchText(c));
            p.writer.RemoveIndent();
        }
    };

    void PrintStmt(MStmt* stmt)
    {
        Accept(StmtPrintVisitor{*this}, stmt);
    }
};

} // namespace

void PrintMData(MData* data, IWriter& writer, RFactory& rFactory)
{
    MPrinterImpl printer{writer, rFactory};
    for (auto& body : data->GetAllFuncBodies())
    {
        printer.PrintFuncBody(body);
        writer.WriteLine();
    }
}

} // namespace Citron






