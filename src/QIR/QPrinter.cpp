#include "QPrinter.h"
#include <format>
#include <regex>
#include <unordered_set>

#include "Infra/IWriter.h"
#include "Infra/Variants.h"
#include "Infra/Exceptions.h"

#include "RSymbol/RDecl.h"
#include "RSymbol/RFuncDecl.h"
#include "NSymbol/NFuncDecl.h"

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

    struct ArgPrinter
    {
        QPrinter& printer;
        
        void operator()(QArg_Slot& arg)
        {
            printer.PrintQArg_Slot(arg);
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
        void operator()(auto& inst) { Print(inst); }

        void Print(QInst_Ctor_String& inst)
        {
            // construct_string %a, "hello"            
            printer.Print("construct_string");
            printer.PrintQArg_Slot(inst.slot);
            printer.Print(", ");
            printer.PrintStringLiteral(inst.text);
            printer.PrintLine();
        }

        void Print(QInst_CopyCtor_String& inst)
        {
            // copy_construct_string %dest, %src
            printer.Print("copy_construct_string ");
            printer.PrintQArg_Slot(inst.slot);
            printer.Print(", ");
            printer.PrintQArg_Slot(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_MoveCtor_String& inst)
        {
            // move_construct_string %dest, %src
            printer.Print("move_construct_string ");
            printer.PrintQArg_Slot(inst.slot);
            printer.Print(", ");
            printer.PrintQArg_Slot(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_CopyAssign_String& inst)
        {
            // copy_assign_string %a
            printer.Print("copy_assign_string ");
            printer.PrintQArg_Slot(inst.dest);
            printer.Print(", ");
            printer.PrintQArg_Slot(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_MoveAssign_String& inst)
        {
            // move_assign_string %a
            printer.Print("move_assign_string ");
            printer.PrintQArg_Slot(inst.dest);
            printer.Print(", ");
            printer.PrintQArg_Slot(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_Dtor_String& inst)
        {
            // destruct_string %a
            printer.Print("destruct_string ");
            printer.PrintQArg_Slot(inst.slot);
            printer.PrintLine();
        }

        void Print(QInst_Load& inst)
        {
            // %v = load [%lv]
            printer.PrintQArg_Slot(inst.dest);
            printer.Print(" = ");
            printer.Print("load ");
            printer.PrintQType(inst.type);
            printer.Print(", ");
            printer.PrintAddrQArg_Slot(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_Store& inst)
        {
            // store <ty> [%lv], %v
            printer.Print("store ");
            printer.PrintQType(inst.type);
            printer.Print(", ");
            printer.PrintAddrQArg_Slot(inst.dest);
            printer.Print(", ");
            printer.PrintQArg_Input(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_AddrOf& inst)
        {
            // %v = addr_of [%s]
            printer.PrintQArg_Slot(inst.dest);
            printer.Print(" = addr_of ");
            printer.PrintAddrQArg_Slot(inst.slot);
            printer.PrintLine();
        }

        void Print(QInst_FieldOf& inst)
        {
            // %dest = field_of [%src], fieldIndex
            printer.PrintQArg_Slot(inst.dest);
            printer.Print(" = field_of ");
            printer.PrintAddrQArg_Slot(inst.src);
            printer.Print(", ");
            printer.Print(to_string(inst.fieldIndex));
            printer.PrintLine();
        }

        void Print(QInst_Assign& inst)
        {
            // %dest = <ty> %src
            printer.PrintQArg_Slot(inst.dest);
            printer.Print(" = ");
            printer.PrintQType(inst.type);
            printer.Print(", ");
            printer.PrintQArg_Input(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_Call& inst)
        {
            // %s = call @F, %s2
            if (inst.o_dest)
            {
                printer.PrintQArg_Slot(*inst.o_dest);
                printer.Print(" = ");
            }

            printer.Print("call ");
            auto rId = inst.rFuncDecl->GetRDecl()->GetIdentifier();
            printer.PrintRName(rId.name);
            for (size_t i = 0; i < inst.args.size(); i++)
            {   
                printer.Print(", ");
                printer.PrintQArg_Input(inst.args[i]);
            }
            printer.PrintLine();            
        }

        void Print(QInst_Intrinsic& inst)
        {
            if (inst.o_dest)
            {
                printer.PrintQArg_Slot(*inst.o_dest);
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

        void Print(QInst_CondJump& inst)
        {
            printer.Print("condjump ");
            printer.PrintQArg_Slot(inst.cond);
            printer.Print(", ");
            printer.PrintBlockLabel(inst.trueBlock);
            printer.Print(", ");
            printer.PrintBlockLabel(inst.falseBlock);
            printer.PrintLine();
        }

        void Print(QInst_Jump& inst)
        {
            printer.Print("jump ");
            printer.PrintBlockLabel(inst.block);
            printer.PrintLine();
        }

        void Print(QInst_Return& inst)
        {
            printer.Print("return");

            if (inst.o_value)
            {
                printer.Print(" ");

                printer.PrintQType(inst.o_value->qType);
                
                printer.Print(", ");

                printer.PrintQArg_Input(inst.o_value->value);
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
    
    void Print(std::string&& str)
    {
        writer.Write(str);
    }

    void PrintQArg_Input(QArg_Input& arg)
    {
        visit(ArgPrinter{*this}, arg);
    }

    void PrintQArg_Slot(QArg_Slot& arg)
    {
        writer.Write(funcBody.slotInfos[arg.index].name);
    }

    void PrintRName(RName& name)
    {
        if (auto* normalName = get_if<RName_Normal>(&name))
        {
            writer.Write(normalName->text);
        }
        else throw NotImplementedException{};
    }
    
    string ToString(QInst_IntrinsicKind kind)
    {
        using enum QInst_IntrinsicKind;

        switch (kind)
        {
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

    void PrintAddrQArg_Slot(QArg_Slot& slot)
    {
        writer.Write("[");
        PrintQArg_Slot(slot);
        writer.Write("]");
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
        
        for (auto& slotInfo : funcBody.slotInfos)
        {
            writer.Write(format("// slot {}: ", slotInfo.name));
            PrintQType(slotInfo.qType);
            writer.WriteLine();
        }
        
        writer.WriteLine();

        for(auto* block : funcBody.blocks)
        {
            // debugText:
            writer.WriteLine();
            writer.Write(format("{}:", block->debugText));

            writer.AddIndent();
            writer.WriteLine();

            for (auto& inst : block->insts)
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