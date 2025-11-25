#include "QPrinter.h"
#include <queue>
#include <format>
#include <regex>
#include <unordered_set>

#include "Infra/IWriter.h"
#include "Infra/Variants.h"
#include "Infra/Exceptions.h"

#include "NSymbol/NFuncDecl.h"
#include "RSymbol/RDecl.h"

#include "QData.h"
#include "QFuncBody.h"
#include "QBlock.h"
#include "QArgs.h"
#include "QFactory.h"

using namespace std;

namespace Citron {

string quote(const string& s)
{
    static regex r{"\""};
    return format("\"{}\"", regex_replace(s, r, "\\\""));
}

class QPrinter
{
    IWriter& writer;
    QFuncBody& funcBody;
    QFactory& qFactory;

    queue<QBlock*> nextBlocks;
    std::unordered_set<QBlock*> queued;
    

    struct ArgPrinter
    {
        QPrinter& printer;

        void operator()(QArg_Register& arg)
        {
            printer.PrintQArg_Register(arg);
        }

        void operator()(QArg_StackSlot& arg)
        {
            printer.PrintQArg_StackSlot(arg);
        }

        void operator()(QArg_ConstBool& arg)
        {
            printer.writer.Write(arg.value ? "true" : "false");
        }

        void operator()(QArg_ConstInt32& arg)
        {
            printer.writer.Write(to_string(arg.value));
        }
    };

    struct InstPrinter
    {
        QPrinter& printer;

        void operator()(QInst_InitString& inst)
        {
            // %a = init_string "hello"
            printer.PrintQArg_StackSlot(inst.dest);
            printer.Print(" = init_string ");
            printer.PrintStringLiteral(inst.text);
            printer.PrintLine();
        }

        void operator()(QInst_Load& inst)
        {
            // %v = load [%lv]
            printer.PrintQArg_Register(inst.dest);
            printer.Print(" = ");
            printer.Print("load ");
            printer.PrintAddrQArg_Loc(inst.src);
            printer.PrintLine();
        }

        void operator()(QInst_Store& inst)
        {
            // store <ty> [%lv], %v
            printer.Print("store ");
            printer.PrintQRegisterType(inst.type);
            printer.Print(", ");
            printer.PrintAddrQArg_Loc(inst.dest);
            printer.Print(", ");
            printer.PrintQArg_Input(inst.src);
            printer.PrintLine();
        }


        void operator()(QInst_Assign& inst)
        {
            // %dest = <ty> %src
            printer.PrintQArg_Register(inst.dest);
            printer.Print(" = ");
            printer.PrintQRegisterType(inst.type);
            printer.Print(", ");
            printer.PrintQArg_Input(inst.src);
            printer.PrintLine();
        }

        void operator()(QInst_Call& inst)
        {
            /*printer.Print("call ");
            printer.Print(inst.funcDecl->GetFullName());
            printer.Print("(");
            for (size_t i = 0; i < inst.args.size(); i++)
            {
                if (i > 0)
                    printer.Print(", ");
                printer.PrintQArg(inst.args[i]);
            }
            printer.Print(")");
            printer.PrintLine();*/
            throw NotImplementedException{};
        }

        void operator()(QInst_Intrinsic& inst)
        {
            if (inst.result)
            {
                printer.PrintQArg_Register(*inst.result);
                printer.Print(" = ");
            }

            printer.Print("intrinsic ");
            printer.PrintInstructionKind(inst.kind);

            for (auto& arg : inst.args)
            {
                printer.Print(", ");
                printer.PrintQArg_Input(arg);
            }

            printer.PrintLine();
        }

        void operator()(QInst_CondJump& inst)
        {
            printer.Print("condjump ");
            printer.PrintQArg_Register(inst.cond);
            printer.Print(", ");
            printer.PrintBlockLabel(inst.trueBlock);
            printer.Print(", ");
            printer.PrintBlockLabel(inst.falseBlock);
            printer.PrintLine();

            printer.AddNextBlock(inst.trueBlock);
            printer.AddNextBlock(inst.falseBlock);
        }

        void operator()(QInst_Jump& inst)
        {
            printer.Print("jump ");
            printer.PrintBlockLabel(inst.block);
            printer.PrintLine();

            printer.AddNextBlock(inst.block);
        }

        void operator()(QInst_Return& inst)
        {
            printer.Print("return");

            if (inst.value)
            {
                printer.Print(", ");
                printer.PrintQArg_Input(*inst.value);
            }

            printer.PrintLine();
        }
    };

public:
    QPrinter(IWriter& writer, QFuncBody& funcBody, QFactory& qFactory)
        : writer{writer}, funcBody{funcBody}, qFactory{qFactory}
    {
    }

    void PrintStringLiteral(const string& str)
    {
        writer.Write(quote(str));
    }

    void PrintLine()
    {
        writer.WriteLine();
    }

    void PrintQRegisterType(QRegisterType type)
    {
        using enum QRegisterType;
        switch (type)
        {
        case Int1:
            writer.Write("<i1>");
            break;
        case Int32:
            writer.Write("<i32>");
            break;
        case Ptr:
            writer.Write("<ptr>");
            break;
        default:
            unreachable();
        }
    }

    void PrintAddrQArg_Loc(QArg_Loc& arg)
    {
        writer.Write("[");
        PrintQArg_Loc(arg);
        writer.Write("]");
    }

    void PrintAddrQArg_Register(QArg_Register& arg)
    {
        writer.Write("[");
        PrintQArg_Register(arg);
        writer.Write("]");
    }

    void Print(std::string&& str)
    {
        writer.Write(str);
    }

    void PrintQArg_Input(QArg_Input& arg)
    {
        visit(ArgPrinter{*this}, arg);
    }

    void PrintQArg_StackSlot(QArg_StackSlot& arg)
    {
        writer.Write(funcBody.slotInfos[arg.index].name);
    }

    void PrintQArg_Register(QArg_Register& arg)
    {
        writer.Write(funcBody.regInfos[arg.index].name);
    }

    void PrintQArg_Loc(QArg_Loc& arg)
    {
        visit(ArgPrinter{*this}, arg);
    }

    void AddNextBlock(QBlock* block)
    {
        auto i = queued.find(block);
        if (i != queued.end()) return;

        queued.insert(block);
        nextBlocks.push(block);
    }

    string ToString(QInst_IntrinsicKind kind)
    {
        using enum QInst_IntrinsicKind;

        switch (kind)
        {
        case DebugPrint_Items: return "DebugPrint_Items";
        case Command_Items: return "Command_Items";
        case Alloc_Int: return "Alloc_Int";
        case Memcpy_Ptr_Ptr_Int: return "Memcpy_Ptr_Ptr_Int";
        case NewList_Items: return "NewList_Items";
        case GetListIterator_List: return "GetListIterator_List";
        case LogicalNot_Bool: return "LogicalNot_Bool";
        case UnaryMinus_Int: return "UnaryMinus_Int";
        case ToString_Bool: return "ToString_Bool";
        case ToString_Int: return "ToString_Int";
        case PrefixInc_Int: return "PrefixInc_Int";
        case PrefixDec_Int: return "PrefixDec_Int";
        case PostfixInc_Int: return "PostfixInc_Int";
        case PostfixDec_Int: return "PostfixDec_Int";
        case Multiply_Int_Int: return "Multiply_Int_Int";
        case Divide_Int_Int: return "Divide_Int_Int";
        case Modulo_Int_Int: return "Modulo_Int_Int";
        case Add_Int_Int: return "Add_Int_Int";
        case Add_String_String: return "Add_String_String";
        case Subtract_Int_Int: return "Subtract_Int_Int";
        case LessThan_Int_Int: return "LessThan_Int_Int";
        case LessThan_String_String: return "LessThan_String_String";
        case GreaterThan_Int_Int: return "GreaterThan_Int_Int";
        case GreaterThan_String_String: return "GreaterThan_String_String";
        case LessThanOrEqual_Int_Int: return "LessThanOrEqual_Int_Int";
        case LessThanOrEqual_String_String: return "LessThanOrEqual_String_String";
        case GreaterThanOrEqual_Int_Int: return "GreaterThanOrEqual_Int_Int";
        case GreaterThanOrEqual_String_String: return "GreaterThanOrEqual_String_String";
        case Equal_Int_Int: return "Equal_Int_Int";
        case Equal_Bool_Bool: return "Equal_Bool_Bool";
        case Equal_String_String: return "Equal_String_String";
        default:
            unreachable();
        }
    }

    void PrintInstructionKind(QInst_IntrinsicKind kind)
    {
        writer.Write(ToString(kind));
    }

    void PrintBlockLabel(QBlock* block)
    {
        writer.Write(block->debugText);
        writer.Write(":");
    }

    void PrintQType(QType* type)
    {
        // TODO: HARD CODED
        if (type == qFactory.MakeVoidType())
        {
            writer.Write("void");
        }
        if (type == qFactory.MakeBoolType())
        {
            writer.Write("bool");
        }
        else if (type == qFactory.MakeIntType())
        {
            writer.Write("int");
        }
        else if (type == qFactory.MakeStringType())
        {
            writer.Write("string");
        }
        else
        {
            writer.Write(format("#{}", *(int*)&type));
        }
    }
    
    // entry
    void Print()
    {   
        writer.Write("Func");
        writer.AddIndent();
        writer.WriteLine();
        
        // TODO: register info, slot info를 출력해줘야 할 것 같다
        for (auto& regInfo : funcBody.regInfos)
        {
            writer.Write(format("// register {}: ", regInfo.name));
            PrintQRegisterType(regInfo.type);
            writer.WriteLine();
        }

        for (auto& slotInfo : funcBody.slotInfos)
        {
            writer.Write(format("// slot {}: ", slotInfo.name));
            PrintQType(slotInfo.qType);
            writer.WriteLine();
        }
        
        writer.WriteLine();

        AddNextBlock(funcBody.entry);

        while (!nextBlocks.empty())
        {
            QBlock* curBlock = nextBlocks.front();
            nextBlocks.pop();

            // debugText:
            writer.WriteLine();
            writer.Write(format("{}:", curBlock->debugText));

            writer.AddIndent();
            writer.WriteLine();

            for (auto& inst : curBlock->insts)
            {
                visit(InstPrinter{*this}, inst);
            }

            writer.RemoveIndent();
        }
        writer.RemoveIndent();
    }
};

void PrintQData(QData* data, IWriter& writer, QFactory& qFactory)
{
    for (auto& body : data->GetAllBodies())
    {
        QPrinter printer{writer, body, qFactory};
        printer.Print();
    }
}

} // namespace Citron