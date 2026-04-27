#include "QPrinter.h"
#include <format>
#include <regex>
#include <unordered_set>

#include "Infra/IWriter.h"
#include "Infra/Variants.h"
#include "Infra/Exceptions.h"

#include "RSymbol/RDecl.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RFactory.h"
#include "NSymbol/NFuncDecl.h"

#include "QData.h"
#include "QFuncBody.h"
#include "QBlock.h"
#include "QArgs.h"

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
    RFactory& rFactory;

    struct QArg_ValuePrinter
    {
        QPrinter& printer;
        void operator()(QArg_Value_Slot& arg)
        {
            printer.PrintSlot(arg.index);
        }
        void operator()(QArg_Value_ConstBool& arg)
        {
            printer.writer.Write(arg.value ? "true" : "false");
        }
        void operator()(QArg_Value_ConstInt32& arg)
        {
            printer.writer.Write(to_string(arg.value));
        }
    };

    struct QArg_CallArgPrinter
    {
        QPrinter& printer;
        void operator()(QArg_CallArg_Slot& arg)
        {
            printer.PrintSlot(arg.index);
        }
        void operator()(QArg_CallArg_ConstBool& arg)
        {
            printer.writer.Write(arg.value ? "true" : "false");
        }
        void operator()(QArg_CallArg_ConstInt32& arg)
        {
            printer.writer.Write(to_string(arg.value));
        }
        void operator()(QArg_CallArg_AddrOfSlot& arg)
        {
            printer.PrintAddrOfSlot(arg.index);
        }
    };

    struct QArg_AddrPrinter
    {
        QPrinter& printer;
        void operator()(QArg_Addr_OfSlot& arg)
        {
            printer.PrintAddrOfSlot(arg.index);
        }
        void operator()(QArg_Addr_PtrSlot& arg)
        {
            printer.PrintSlot(arg.index);
        }
    };


    struct InstPrinter
    {
        QPrinter& printer;
        void operator()(auto& inst) { Print(inst); }

        void Print(QInst_Ctor_String& inst)
        {
            // construct_string %a, "hello"            
            printer.Print("construct_string ");
            printer.PrintQArg_Addr(inst._this);
            printer.Print(", ");
            printer.PrintStringLiteral(inst.text);
            printer.PrintLine();
        }
        
        void Print(QInst_Load& inst)
        {
            // %v = load [%lv]
            printer.PrintQArg_Dest(inst.dest);
            printer.Print(" = ");
            printer.Print("load ");
            printer.PrintRType(inst.type);
            printer.Print(", ");
            printer.PrintQArg_Addr_PtrSlot(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_Store& inst)
        {
            // store <ty> [%lv], %v
            printer.Print("store ");
            printer.PrintRType(inst.type);
            printer.Print(", ");
            printer.PrintQArg_Addr_PtrSlot(inst.dest);
            printer.Print(", ");
            printer.PrintQArg_Value(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_AddrOf& inst)
        {
            // %v = addr_of [%s]
            printer.PrintQArg_Dest(inst.dest);
            printer.Print(" = addr_of ");
            printer.PrintSlot(inst.slot);
            printer.PrintLine();
        }

        void Print(QInst_FieldOf& inst)
        {
            // %dest = field_of [%src], fieldIndex
            printer.PrintQArg_Dest(inst.dest);
            printer.Print(" = field_of ");
            printer.PrintQArg_Addr(inst.src);
            printer.Print(", ");
            printer.Print(to_string(inst.fieldIndex));
            printer.PrintLine();
        }

        void Print(QInst_Assign& inst)
        {
            // %dest = <ty> %src
            printer.PrintQArg_Dest(inst.dest);
            printer.Print(" = ");
            printer.PrintRType(inst.type);
            printer.Print(", ");
            printer.PrintQArg_Value(inst.src);
            printer.PrintLine();
        }

        void Print(QInst_Call& inst)
        {
            // %s = call @F, %s2
            if (inst.o_dest)
            {
                printer.PrintQArg_Dest(*inst.o_dest);
                printer.Print(" = ");
            }

            printer.Print("call ");
            auto rId = inst.rFuncDecl->GetRDecl()->GetIdentifier();
            printer.PrintRName(rId.name);
            for (size_t i = 0; i < inst.args.size(); i++)
            {   
                printer.Print(", ");
                printer.PrintQArg_CallArg(inst.args[i]);
            }
            printer.PrintLine();            
        }

        void Print(QInst_Intrinsic& inst)
        {
            if (inst.o_dest)
            {
                printer.PrintQArg_Dest(*inst.o_dest);
                printer.Print(" = ");
            }

            printer.Print("intrinsic ");
            printer.PrintInstructionKind(inst.kind);

            for (auto& arg : inst.args)
            {
                printer.Print(", ");
                printer.PrintQArg_CallArg(arg);
            }

            printer.PrintLine();
        }

        void Print(QInst_CondJump& inst)
        {
            printer.Print("condjump ");
            printer.PrintQArg_Value_Slot(inst.cond);
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

                printer.PrintRType(inst.o_value->type);
                
                printer.Print(", ");

                printer.PrintQArg_Value(inst.o_value->value);
            }

            printer.PrintLine();
        }
    };

public:
    QPrinter(IWriter& writer, QFuncBody& funcBody, RFactory& rFactory)
        : writer{writer}, funcBody{funcBody}, rFactory{rFactory}
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

    void PrintQArg_Value(QArg_Value& arg)
    {
        visit(QArg_ValuePrinter{*this}, arg);
    }

    void PrintQArg_CallArg(QArg_CallArg& arg)
    {
        visit(QArg_CallArgPrinter{*this}, arg);
    }

    void PrintQArg_Addr(QArg_Addr& arg)
    {
        visit(QArg_AddrPrinter{*this}, arg);
    }

    void PrintQArg_Addr_PtrSlot(QArg_Addr_PtrSlot& arg)
    {
        PrintSlot(arg.index);
    }

    void PrintQArg_Value_Slot(QArg_Value_Slot& arg)
    {
        PrintSlot(arg.index);
    }

    void PrintQArg_Dest(QArg_Dest& arg)
    {
        PrintSlot(arg.index);
    }

    void PrintSlot(size_t index)
    {
        writer.Write(funcBody.slotInfos[index].name);
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
        case Command_Item: return "Command_Item";
        case Alloc_Int: return "Alloc_Int";
        case Memcpy_Void_Ptr_Ptr_Int: return "Memcpy_Void_Ptr_Ptr_Int";
        case NewList_Items: return "NewList_Items";
        case GetIterator_ListPtr_ListIterator: return "GetIterator_ListPtr_ListIterator";
        case LogicalNot_Bool_Bool: return "LogicalNot_Bool_Bool";
        case UnaryMinus_Int_Int: return "UnaryMinus_Int_Int";
        case ToString_String_Bool: return "ToString_String_Bool";
        case ToString_String_Int: return "ToString_String_Int";
        case PrefixInc_Int_IntRef: return "PrefixInc_Int_IntRef";
        case PrefixDec_Int_IntRef: return "PrefixDec_Int_IntRef";
        case PostfixInc_Int_IntRef: return "PostfixInc_Int_IntRef";
        case PostfixDec_Int_IntRef: return "PostfixDec_Int_IntRef";
        case Multiply_Int_Int_Int: return "Multiply_Int_Int_Int";
        case Divide_Int_Int_Int: return "Divide_Int_Int_Int";
        case Modulo_Int_Int_Int: return "Modulo_Int_Int_Int";
        case Add_Int_Int_Int: return "Add_Int_Int_Int";
        case Add_String_StringInRef_StringInRef: return "Add_String_StringInRef_StringInRef";
        case Subtract_Int_Int_Int: return "Subtract_Int_Int_Int";
        case LessThan_Bool_Int_Int: return "LessThan_Bool_Int_Int";
        case LessThan_Bool_StringInRef_StringInRef: return "LessThan_Bool_StringInRef_StringInRef";
        case GreaterThan_Bool_Int_Int: return "GreaterThan_Bool_Int_Int";
        case GreaterThan_Bool_StringInRef_StringInRef: return "GreaterThan_Bool_StringInRef_StringInRef";
        case LessThanOrEqual_Bool_Int_Int: return "LessThanOrEqual_Bool_Int_Int";
        case LessThanOrEqual_Bool_StringInRef_StringInRef: return "LessThanOrEqual_Bool_StringInRef_StringInRef";
        case GreaterThanOrEqual_Bool_Int_Int: return "GreaterThanOrEqual_Bool_Int_Int";
        case GreaterThanOrEqual_Bool_StringInRef_StringInRef: return "GreaterThanOrEqual_Bool_StringInRef_StringInRef";
        case Equal_Bool_Int_Int: return "Equal_Bool_Int_Int";
        case Equal_Bool_Bool_Bool: return "Equal_Bool_Bool_Bool";
        case Equal_Bool_StringInRef_StringInRef: return "Equal_Bool_StringInRef_StringInRef";
        case CopyCtor_Void_StringRef_StringInRef: return "CopyCtor_Void_StringRef_StringInRef";
        case MoveCtor_Void_StringRef_StringMoveRef: return "MoveCtor_Void_StringRef_StringMoveRef";
        case Dtor_Void_StringRef: return "Dtor_Void_StringRef";
        case CopyAssign_Void_StringRef_StringInRef: return "CopyAssign_Void_StringRef_StringInRef";
        case MoveAssign_Void_StringRef_StringMoveRef: return "MoveAssign_Void_StringRef_StringMoveRef";
        case Max: unreachable();
        }

        unreachable();
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

    void PrintAddrOfSlot(size_t index)
    {
        writer.Write("[");
        PrintSlot(index);
        writer.Write("]");
    }

    void PrintRType(RType* type)
    {
        // TODO: HARD CODED
        if (type == rFactory.MakeVoidType())
        {
            writer.Write("void");
        }
        if (type == rFactory.MakeBoolType())
        {
            writer.Write("bool");
        }
        else if (type == rFactory.MakeIntType())
        {
            writer.Write("int");
        }
        else if (type == rFactory.MakeStringType())
        {
            writer.Write("string");
        }
        else if (type == rFactory.MakePtrType(rFactory.MakeVoidType()))
        {
            writer.Write("ptr");
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
            PrintRType(slotInfo.type);
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

void PrintQData(QData* data, IWriter& writer, RFactory& rFactory)
{
    for (auto& body : data->GetAllBodies())
    {
        QPrinter printer{writer, body, rFactory};
        printer.Print();
    }
}

} // namespace Citron