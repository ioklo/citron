using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Gum.IR0
{
    public abstract class Exp
    {
        internal Exp() { }
    }

    public class ExternalGlobalVarExp : Exp
    {
        public ExternalGlobalVarId VarId { get; } // ExternalGlobalVarId
        public ExternalGlobalVarExp(ExternalGlobalVarId varId) { VarId = varId; }
    }

    public class PrivateGlobalVarExp : Exp
    {
        public string Name { get; }
        public PrivateGlobalVarExp(string name) { Name = name; }
    }

    public class LocalVarExp : Exp
    {
        public string Name { get; } // LocalVarId;
        public LocalVarExp(string name) { Name = name; }
    }    
    
    // "dskfjslkf $abc "
    public class StringExp : Exp
    {
        public ImmutableArray<StringExpElement> Elements { get; }
        
        public StringExp(IEnumerable<StringExpElement> elements)
        {
            Elements = elements.ToImmutableArray();
        }

        public StringExp(params StringExpElement[] elements)
        {
            Elements = ImmutableArray.Create(elements);
        }
    }

    // 1
    public class IntLiteralExp : Exp
    {
        public int Value { get; }
        public IntLiteralExp(int value) { Value = value; }
    }

    // false
    public class BoolLiteralExp : Exp
    {
        public bool Value { get; }
        public BoolLiteralExp(bool value) { Value = value; }
    }

    public enum InternalUnaryOperator
    {
        LogicalNot_Bool_Bool,
        UnaryMinus_Int_Int,

        ToString_Bool_String,
        ToString_Int_String,
    }

    public enum InternalUnaryAssignOperator
    {
        PrefixInc_Int_Int,
        PrefixDec_Int_Int,
        PostfixInc_Int_Int,
        PostfixDec_Int_Int,
    }

    public enum InternalBinaryOperator
    {
        Multiply_Int_Int_Int,
        Divide_Int_Int_Int,
        Modulo_Int_Int_Int,
        Add_Int_Int_Int,
        Add_String_String_String,
        Subtract_Int_Int_Int,
        LessThan_Int_Int_Bool,
        LessThan_String_String_Bool,
        GreaterThan_Int_Int_Bool,
        GreaterThan_String_String_Bool,
        LessThanOrEqual_Int_Int_Bool,
        LessThanOrEqual_String_String_Bool,
        GreaterThanOrEqual_Int_Int_Bool,
        GreaterThanOrEqual_String_String_Bool,
        Equal_Int_Int_Bool,
        Equal_Bool_Bool_Bool,
        Equal_String_String_Bool
    }   

    public class CallInternalUnaryOperatorExp : Exp
    {
        public InternalUnaryOperator Operator { get; }
        public ExpInfo Operand { get; }

        public CallInternalUnaryOperatorExp(InternalUnaryOperator op, ExpInfo operand)
        {
            Operator = op;
            Operand = operand;
        }
    }

    public class CallInternalUnaryAssignOperator : Exp
    {
        public InternalUnaryAssignOperator Operator { get; }
        public Exp Operand { get; }
        public CallInternalUnaryAssignOperator(InternalUnaryAssignOperator op, Exp operand)
        {
            Operator = op;
            Operand = operand;
        }
    }

    public class CallInternalBinaryOperatorExp : Exp
    {
        public InternalBinaryOperator Operator { get; }
        public ExpInfo Operand0 { get; }
        public ExpInfo Operand1 { get; }

        public CallInternalBinaryOperatorExp(InternalBinaryOperator op, ExpInfo operand0, ExpInfo operand1)
        {
            Operator = op;
            Operand0 = operand0;
            Operand1 = operand1;
        }
    }

    // a = b
    public class AssignExp : Exp
    {
        public Exp Dest { get; }
        public Exp Src { get; }        

        public AssignExp(Exp dest, Exp src)
        {
            Dest = dest;
            Src = src;
        }
    }

    // F(2, 3)
    public class CallFuncExp : Exp
    {
        public FuncId FuncId { get; }
        public ImmutableArray<TypeId> TypeArgs { get; }
        public ExpInfo? Instance { get; }
        public ImmutableArray<ExpInfo> Args { get; }

        public CallFuncExp(FuncId funcId, ExpInfo? instance, IEnumerable<ExpInfo> args)
        {
            FuncId = funcId;
            Instance = instance;
            Args = args.ToImmutableArray();
        }
    }

    public class CallSeqFuncExp : Exp
    {
        public SeqFuncId SeqFuncId { get; }
        public ExpInfo? Instance { get; }
        public ImmutableArray<ExpInfo> Args { get; }

        public CallSeqFuncExp(SeqFuncId seqFuncId, ExpInfo? instance, IEnumerable<ExpInfo> args)
        {
            SeqFuncId = seqFuncId;
            Instance = instance;
            Args = args.ToImmutableArray();
        }
    }

    // f(2, 3)
    public class CallValueExp : Exp
    {
        public ExpInfo Callable { get; }        
        public ImmutableArray<ExpInfo> Args { get; }

        public CallValueExp(ExpInfo callable, IEnumerable<ExpInfo> args)
        {
            Callable = callable;
            Args = args.ToImmutableArray();
        }
    }
    
    // () => { return 1; }
    public class LambdaExp : Exp
    {
        public CaptureInfo CaptureInfo { get; }
        public ImmutableArray<string> ParamNames { get; }
        public Stmt Body { get; }

        public LambdaExp(CaptureInfo captureInfo, IEnumerable<string> paramNames, Stmt body)
        {
            CaptureInfo = captureInfo;
            ParamNames = paramNames.ToImmutableArray();
            Body = body;
        }
    }
    
    // l[b], l is list
    public class ListIndexerExp : Exp
    {
        public ExpInfo ListInfo { get; }
        public ExpInfo IndexInfo { get; }

        public ListIndexerExp(ExpInfo listInfo, ExpInfo indexInfo)
        {
            ListInfo = listInfo;
            IndexInfo = indexInfo;
        }
    }

    public class StaticMemberExp : Exp
    {
        public TypeId TypeId { get; }
        public string MemberName { get; }

        public StaticMemberExp(TypeId typeId, string memberName)
        {
            TypeId = typeId;
            MemberName = memberName;
        }
    }

    public class StructMemberExp : Exp
    {
        public Exp Instance { get; }
        public string MemberName { get; }

        public StructMemberExp(Exp instance, string memberName)
        {
            Instance = instance;
            MemberName = memberName;
        }
    }

    public class ClassMemberExp : Exp
    {
        public Exp Instance { get; }
        public string MemberName { get; }
        public ClassMemberExp(Exp instance, string memberName)
        {
            Instance = instance;
            MemberName = memberName;
        }
    }

    public class EnumMemberExp : Exp
    {
        public Exp Instance { get; }
        public string MemberName { get; }
        public EnumMemberExp(Exp instance, string memberName)
        {
            Instance = instance;
            MemberName = memberName;
        }

    }

    // [1, 2, 3]
    public class ListExp : Exp
    {
        public TypeId ElemTypeId { get; }
        public ImmutableArray<Exp> Elems { get; }

        public ListExp(TypeId elemTypeId, IEnumerable<Exp> elems)
        {
            ElemTypeId = elemTypeId;
            Elems = elems.ToImmutableArray();
        }
    }

    // enum construction, E.First or E.Second(2, 3)
    public class NewEnumExp : Exp
    {
        public struct Elem
        {
            public string Name { get; }
            public ExpInfo ExpInfo { get; }

            public Elem(string name, ExpInfo expInfo)
            {
                Name = name;
                ExpInfo = expInfo;
            }
        }

        public string Name { get; }

        public ImmutableArray<Elem> Members { get; }

        public NewEnumExp(string name, IEnumerable<Elem> members)
        {
            Name = name;
            Members = members.ToImmutableArray();
        }
    }

    // new S(2, 3, 4);
    public class NewStructExp : Exp
    {
        public TypeId Type { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<ExpInfo> Args { get; }

        public NewStructExp(TypeId type, IEnumerable<ExpInfo> args)
        {
            Type = type;
            Args = args.ToImmutableArray();
        }
    }

    // new C(2, 3, 4);
    public class NewClassExp : Exp
    {
        public TypeId Type { get; }

        // TODO: params, out, 등 처리를 하려면 Exp가 아니라 다른거여야 한다
        public ImmutableArray<ExpInfo> Args { get; }
        
        public NewClassExp(TypeId type, IEnumerable<ExpInfo> args)
        {
            Type = type;
            Args = args.ToImmutableArray();
        }
    }
}
