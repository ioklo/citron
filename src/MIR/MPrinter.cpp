#include "MPrinter.h"

#include <format>
#include <string>
#include <variant>
#include <span>

#include "Infra/Exceptions.h"
#include "Infra/IWriter.h"
#include "NSymbol/NFuncDecl.h"
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

class MPrinter {
    IWriter& writer;
    RFactory& rFactory;

public:
    MPrinter(IWriter& writer, RFactory& rFactory) : writer(writer), rFactory(rFactory) {}

    void PrintFuncBody(MFuncBody& funcBody)
    {
        writer.Write("Func ");
        PrintRName(funcBody.nFuncDecl->GetNDecl()->GetRDecl()->GetIdentifier().name);
        writer.WriteLine();
        writer.AddIndent();
        PrintStmt(funcBody.body);
        writer.RemoveIndent();
    }

private:
    void PrintLine(const string& line) { writer.Write(line); writer.WriteLine(); }

    void PrintRName(const RName& name)
    {
        if (auto* normal = get_if<RName_Normal>(&name)) writer.Write(normal->text);
        else if (auto* reserved = get_if<RName_Reserved>(&name)) writer.Write(format("${}", reserved->text));
        else if (auto* ctorParam = get_if<RName_CtorParam>(&name)) writer.Write(format("$ctor.{}", ctorParam->index));
        else if (auto* lambda = get_if<RName_Lambda>(&name)) writer.Write(format("$lambda.{}", lambda->index));
        else throw NotImplementedException{};
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
        if (auto* normal = get_if<RName_Normal>(&id.name)) return normal->text;
        return format("decl#{}", reinterpret_cast<uintptr_t>(decl));
    }

    string ReadText(MRead& read)
    {
        return visit([this](auto& read) -> string {
            using T = remove_cvref_t<decltype(read)>;
            if constexpr (same_as<T, MRead_Exp>) return format("read_exp({})", ExpText(read.exp));
            else return format("read_loc({})", LocText(read.loc));
        }, read);
    }

    string CreateText(MCreate& create)
    {
        return visit([this](auto& create) -> string {
            using T = remove_cvref_t<decltype(create)>;
            if constexpr (same_as<T, MCreate_BC>) return format("create_bc({})", ExpText(create.exp));
            else return format("create_nbc({})", InitExpText(create.initExp));
        }, create);
    }

    string MoveSourceText(MMoveSource& src)
    {
        return visit([this](auto& src) -> string {
            using T = remove_cvref_t<decltype(src)>;
            if constexpr (same_as<T, MMoveSource_MovedLoc>) return format("move({})", LocText(src.loc));
            else return format("move_materialized({})", LocText(src.loc));
        }, src);
    }

    string ArgumentText(MArgument& arg)
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
        }, arg);
    }

    string ArgsText(span<MArgument> args)
    {
        string s;
        for (size_t i = 0; i < args.size(); i++)
        {
            if (i != 0) s += ", ";
            s += ArgumentText(args[i]);
        }
        return s;
    }

    string CallableText(MCallable& callable)
    {
        return visit([this](auto& c) -> string {
            using T = remove_cvref_t<decltype(c)>;
            if constexpr (same_as<T, MCallable_GlobalFunc>) return DeclText(c.decl);
            else if constexpr (same_as<T, MCallable_ClassFunc>) return format("{}.{}", LocText(c.instance), DeclText(c.decl));
            else if constexpr (same_as<T, MCallable_StructFunc>) return format("{}.{}", LocText(c.instance), DeclText(c.decl));
            else return format("lambda {}", LocText(c.callable));
        }, callable);
    }

    string PatternLeafText(MPatternLeaf& pattern)
    {
        return visit([this](auto& p) -> string {
            using T = remove_cvref_t<decltype(p)>;
            if constexpr (same_as<T, MPattern_Alias>) {
                if (auto* normal = get_if<RName_Normal>(&p.name)) return normal->text;
                return "alias";
            }
            else return "_";
        }, pattern);
    }

    string PatternText(MPattern& pattern)
    {
        return visit([this](auto& p) -> string { return PatternTextImpl(p); }, pattern);
    }

    string TopLevelPatternText(MTopLevelPattern& pattern)
    {
        return visit([this](auto& p) -> string { return PatternTextImpl(p); }, pattern);
    }

    string PatternTextImpl(MPattern_Alias& p) { if (auto* normal = get_if<RName_Normal>(&p.name)) return normal->text; return "alias"; }
    string PatternTextImpl(MPattern_Ignore&) { return "_"; }
    string PatternTextImpl(MPattern_Null&) { return "null"; }
    string PatternTextImpl(MPattern_Some& p) { return format("some {}", PatternLeafText(p.pattern)); }
    string PatternTextImpl(MPattern_Class& p) { return format("{} {}", TypeText(p.type), PatternLeafText(p.pattern)); }
    string PatternTextImpl(MPattern_EnumElem& p) { return TypeText(p.type); }

    string LocText(MLoc* loc)
    {
        if (auto* x = dynamic_cast<MLoc_Materialize*>(loc)) return format("materialize({})", CreateText(x->create));
        if (auto* x = dynamic_cast<MLoc_LocalVar*>(loc)) { if (auto* n = get_if<RName_Normal>(&x->name)) return format("local {}", n->text); return "local"; }
        if (auto* x = dynamic_cast<MLoc_LocalRef*>(loc)) { if (auto* n = get_if<RName_Normal>(&x->name)) return format("localref {}", n->text); return "localref"; }
        if (auto* x = dynamic_cast<MLoc_LambdaVar*>(loc)) return format("lambda_var {}", DeclText(x->decl));
        if (auto* x = dynamic_cast<MLoc_ListIndexer*>(loc)) return format("list_index({}, {})", LocText(x->list.loc), ReadText(x->index));
        if (auto* x = dynamic_cast<MLoc_StructVar*>(loc)) return format("struct_var({}, {})", x->instance ? LocText(x->instance) : string("<static>"), DeclText(x->decl));
        if (auto* x = dynamic_cast<MLoc_ClassVar*>(loc)) return format("class_var({}, {})", x->instance ? LocText(x->instance) : string("<static>"), DeclText(x->decl));
        if (auto* x = dynamic_cast<MLoc_EnumElemVar*>(loc)) return format("enum_elem_var({}, {})", x->instance ? LocText(x->instance) : string("<static>"), DeclText(x->decl));
        if (auto* x = dynamic_cast<MLoc_This*>(loc)) return format("this:{}", TypeText(x->type));
        if (auto* x = dynamic_cast<MLoc_PtrDeref*>(loc)) return format("ptr_deref({})", ReadText(x->srcPtr));
        if (auto* x = dynamic_cast<MLoc_SharedDeref*>(loc)) return format("shared_deref({})", LocText(x->srcShared.loc));
        if (auto* x = dynamic_cast<MLoc_NullableValue*>(loc)) return format("nullable_value({})", LocText(x->loc));
        return format("loc#{}", reinterpret_cast<uintptr_t>(loc));
    }

    string ExpText(MExp* exp)
    {
        if (auto* x = dynamic_cast<MExp_Load*>(exp)) return format("load({})", LocText(x->loc));
        if (auto* x = dynamic_cast<MExp_Store*>(exp)) return format("store({}, {})", LocText(x->dest), ReadText(x->src));
        if (dynamic_cast<MExp_Stmt*>(exp)) return "stmt_exp(...)";
        if (auto* x = dynamic_cast<MExp_PtrRef*>(exp)) return format("ptr_ref({})", LocText(x->innerLoc));
        if (auto* x = dynamic_cast<MExp_BoolLiteral*>(exp)) return x->value ? "true" : "false";
        if (auto* x = dynamic_cast<MExp_IntLiteral*>(exp)) return to_string(x->value);
        if (auto* x = dynamic_cast<MExp_CallIntrinsic*>(exp)) return format("intrinsic#{}({})", static_cast<int>(x->kind), ArgsText(x->args));
        if (auto* x = dynamic_cast<MExp_Call*>(exp)) return format("{}({})", CallableText(x->callable), ArgsText(x->args));
        if (auto* x = dynamic_cast<MExp_NewStruct*>(exp)) return format("new_struct {}({})", DeclText(x->ctor), ArgsText(x->args));
        if (auto* x = dynamic_cast<MExp_NewEnumElem*>(exp)) return format("new_enum_elem {}({})", DeclText(x->enumElemDecl), ArgsText(x->args));
        if (auto* x = dynamic_cast<MExp_Nullable*>(exp)) return format("nullable({})", ExpText(x->innerExp.exp));
        if (auto* x = dynamic_cast<MExp_NullableNullLiteral*>(exp)) return format("nullable_null({})", TypeText(x->innerType));
        if (auto* x = dynamic_cast<MExp_Cast*>(exp)) return format("cast#{}({}, {})", static_cast<int>(x->kind), ReadText(x->src), TypeText(x->targetType));
        if (auto* x = dynamic_cast<MExp_Lambda*>(exp)) return format("lambda({})", ArgsText(x->args));
        if (auto* x = dynamic_cast<MExp_InlineBlock*>(exp)) return format("inline_block -> {}", TypeText(x->returnType));
        if (auto* x = dynamic_cast<MExp_Is*>(exp)) return format("{} is {}", ReadText(x->operand), TopLevelPatternText(x->pattern));
        return format("exp#{}", reinterpret_cast<uintptr_t>(exp));
    }

    string InitExpText(MInitExp* exp)
    {
        if (auto* x = dynamic_cast<MInitExp_Shared*>(exp)) return format("shared({})", CreateText(x->create));
        if (dynamic_cast<MInitExp_SharedRef*>(exp)) return "shared_ref(...)";
        if (dynamic_cast<MInitExp_Stmt*>(exp)) return "stmt_init(...)";
        if (dynamic_cast<MInitExp_String*>(exp)) return "string(...)";
        if (dynamic_cast<MInitExp_List*>(exp)) return "list(...)";
        if (auto* x = dynamic_cast<MInitExp_CallIntrinsic*>(exp)) return format("init_intrinsic#{}({})", static_cast<int>(x->kind), ArgsText(x->args));
        if (auto* x = dynamic_cast<MInitExp_NewClass*>(exp)) return format("new_class {}({})", DeclText(x->ctorDecl), ArgsText(x->args));
        if (dynamic_cast<MInitExp_StructCtor*>(exp)) return "struct_ctor(...)";
        if (auto* x = dynamic_cast<MInitExp_Call*>(exp)) return format("{}({})", CallableText(x->callable), ArgsText(x->args));
        if (auto* x = dynamic_cast<MInitExp_NewEnumElem*>(exp)) return format("new_enum_elem {}({})", DeclText(x->enumElemDecl), ArgsText(x->args));
        if (auto* x = dynamic_cast<MInitExp_Nullable*>(exp)) return format("nullable({})", InitExpText(x->inner.initExp));
        if (auto* x = dynamic_cast<MInitExp_NullableNullLiteral*>(exp)) return format("nullable_null({})", TypeText(x->innerType));
        if (auto* x = dynamic_cast<MInitExp_NullableInplaceNullLiteral*>(exp)) return format("nullable_inplace_null({})", TypeText(x->innerType));
        if (auto* x = dynamic_cast<MInitExp_Cast*>(exp)) return format("init_cast#{}({}, {})", static_cast<int>(x->kind), ReadText(x->src), TypeText(x->targetType));
        if (auto* x = dynamic_cast<MInitExp_Lambda*>(exp)) return format("lambda({})", ArgsText(x->args));
        if (auto* x = dynamic_cast<MInitExp_InlineBlock*>(exp)) return format("inline_block -> {}", TypeText(x->returnType));
        if (auto* x = dynamic_cast<MInitExp_As*>(exp)) return format("as#{}({}, {})", static_cast<int>(x->kind), ReadText(x->target), TypeText(x->type));
        return format("initexp#{}", reinterpret_cast<uintptr_t>(exp));
    }

    void PrintStmtBlock(span<MStmt*> stmts)
    {
        writer.WriteLine();
        writer.AddIndent();
        for (auto* stmt : stmts) PrintStmt(stmt);
        writer.RemoveIndent();
    }

    string CatchText(MCatch& mCatch)
    {
        return visit([this](auto& mCatch) -> string {
            using T = remove_cvref_t<decltype(mCatch)>;
            if constexpr (same_as<T, MCatch_Resume>) return format("catch_resume({})", TypeText(mCatch.errorType));
            else if constexpr (same_as<T, MCatch_Return>) return format("catch_return({})", TypeText(mCatch.errorType));
            else if constexpr (same_as<T, MCatch_Error>) return format("catch_error({})", TypeText(mCatch.errorType));
            else if constexpr (same_as<T, MCatch_Break>) return "catch_break";
            else return "catch_continue";
        }, mCatch);
    }

    void PrintStmt(MStmt* stmt)
    {
        if (auto* x = dynamic_cast<MStmt_Command*>(stmt))
        {
            string line = "command ";
            for (size_t i = 0; i < x->commands.size(); i++)
            {
                if (i != 0) line += ", ";
                line += LocText(x->commands[i].loc);
            }
            return PrintLine(line);
        }
        if (auto* x = dynamic_cast<MStmt_LocalVarDecl*>(stmt))
        {
            string line = format("var {} ", TypeText(x->type));
            if (auto* n = get_if<RName_Normal>(&x->name)) line += n->text; else line += "var";
            visit([&](auto& init) {
                using T = remove_cvref_t<decltype(init)>;
                if constexpr (same_as<T, MStmt_LocalVarDeclInit_Create>) line += format(" = {}", CreateText(init.create));
            }, x->init);
            return PrintLine(line);
        }
        if (auto* x = dynamic_cast<MStmt_LocalRefDecl*>(stmt)) return PrintLine(format("ref {} {} = {}", TypeText(x->type), get_if<RName_Normal>(&x->name) ? get<RName_Normal>(x->name).text : string("ref"), LocText(x->loc)));
        if (auto* x = dynamic_cast<MStmt_If*>(stmt))
        {
            writer.Write(format("if {}", ReadText(x->cond)));
            PrintStmt(x->trueBody);
            if (x->falseBody)
            {
                writer.Write("else");
                PrintStmt(x->falseBody);
            }
            return;
        }
        if (auto* x = dynamic_cast<MStmt_For*>(stmt))
        {
            writer.Write("for"); writer.WriteLine(); writer.AddIndent();
            PrintLine(x->cond ? format("cond: {}", ReadText(*x->cond)) : string("cond: <none>"));
            if (x->contStmt) { writer.Write("cont:"); writer.WriteLine(); writer.AddIndent(); PrintStmt(x->contStmt); writer.RemoveIndent(); }
            writer.Write("body:"); PrintStmt(x->body);
            writer.RemoveIndent();
            return;
        }
        if (dynamic_cast<MStmt_Continue*>(stmt)) return PrintLine("continue");
        if (dynamic_cast<MStmt_Break*>(stmt)) return PrintLine("break");
        if (auto* x = dynamic_cast<MStmt_Return*>(stmt)) return PrintLine(x->create ? format("return {}", CreateText(*x->create)) : string("return"));
        if (auto* x = dynamic_cast<MStmt_Scope*>(stmt)) { writer.Write("scope"); PrintStmtBlock(x->stmts); return; }
        if (dynamic_cast<MStmt_Blank*>(stmt)) return PrintLine("blank");
        if (auto* x = dynamic_cast<MStmt_Exp*>(stmt)) return PrintLine(CreateText(x->create));
        if (auto* x = dynamic_cast<MStmt_Task*>(stmt)) return PrintLine(format("task({})", ArgsText(x->captureArgs)));
        if (auto* x = dynamic_cast<MStmt_Await*>(stmt)) { writer.Write("await"); PrintStmt(x->body); return; }
        if (auto* x = dynamic_cast<MStmt_Async*>(stmt)) return PrintLine(format("async({})", ArgsText(x->captureArgs)));
        if (auto* x = dynamic_cast<MStmt_Foreach*>(stmt)) { writer.Write(format("foreach {}", CreateText(x->iterCreate))); PrintStmt(x->body); return; }
        if (auto* x = dynamic_cast<MStmt_Yield*>(stmt)) return PrintLine(format("yield {}", CreateText(x->valueCreate)));
        if (auto* x = dynamic_cast<MStmt_CallBaseClassCtor*>(stmt)) return PrintLine(format("base_class_ctor {}({})", DeclText(x->ctor), ArgsText(x->args)));
        if (auto* x = dynamic_cast<MStmt_CallBaseStructCtor*>(stmt)) return PrintLine(format("base_struct_ctor({})", ArgsText(x->args)));
        if (dynamic_cast<MStmt_Directive*>(stmt)) return PrintLine("directive(...)");
        if (auto* x = dynamic_cast<MStmt_Call*>(stmt)) return PrintLine(format("{}({})", CallableText(x->callable), ArgsText(x->args)));
        if (auto* x = dynamic_cast<MStmt_Assign*>(stmt)) return PrintLine(format("assign {} = {}", LocText(x->dest), LocText(x->src.loc)));
        if (auto* x = dynamic_cast<MStmt_Do*>(stmt))
        {
            writer.Write("do"); PrintStmt(x->body);
            for (auto& c : x->catches) PrintLine(CatchText(c));
            return;
        }
        PrintLine(format("stmt#{}", reinterpret_cast<uintptr_t>(stmt)));
    }
};

} // namespace

void PrintMData(MData* data, IWriter& writer, RFactory& rFactory)
{
    MPrinter printer{writer, rFactory};
    for (auto& body : data->GetAllFuncBodies())
    {
        printer.PrintFuncBody(body);
        writer.WriteLine();
    }
}

} // namespace Citron


