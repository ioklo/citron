using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL
{
    public interface ICommand
    {
        void Visit(ICommandVisitor visitor);
    }

    public interface ICommandVisitor
    {
        // Location에서 값을 저장/불러오기
        void Visit(StoreLocal p);
        void Visit(LoadLocal p);

        // class/struct support
        void Visit(LoadField p);
        void Visit(StoreField p);
        void Visit(New p);

        // reference value is introduced
        // 
        // location = local + reference
        // load field 하면 reference 혹은 value가 나온다
        // new creates reference
        // the value is stored to where the reference indicates.
        
        // 
        // a.b.c.d = 1;
        // load local "a" => a reference
        // load field "b" => a.b's reference
        // load field "c" => a.b.c's reference
        // push 1
        // store field "d";

        // load field "d" => if d is a value type, the value is at the top of the stack.
        // ok?
        
        // Jump
        void Visit(Jump p);
        void Visit(IfJump p);
        void Visit(IfNotJump p);

        // Function Call
        void Visit(StaticCall p);
        // void Visit(VirtualCall p);
        void Visit(Return p);
        void Visit(Yield p);

        // Stack manipulation
        void Visit(Push p);
        void Visit(Dup p);
        void Visit(Pop p);
        void Visit(Operator op);        
    }

    public class Push : ICommand
    {
        public object Value { get; private set; }

        public Push(object val)
        {
            Value = val;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Dup : ICommand
    {
        public Dup() { }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Pop : ICommand
    {
        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // Branch
    public class Jump : ICommand
    {
        public int Point { get; private set; }

        public Jump(int pt)
        {
            Point = pt;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // Branch
    public class IfJump : ICommand
    {
        public int Point { get; private set; }

        public IfJump(int pt)
        {
            Point = pt;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class IfNotJump : ICommand
    {
        public int Point { get; private set; }

        public IfNotJump(int pt)
        {
            Point = pt;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // Static call/non-virtual call
    public class StaticCall : ICommand
    {
        public FuncInfo FuncInfo { get; private set; }

        public StaticCall(FuncInfo fi)
        {
            FuncInfo = fi;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    
    // 스택 맨 위에 있는 값을 돌려준다.
    // Value
    public class Return : ICommand
    {
        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    
    // Loc
    public class StoreLocal : ICommand
    {
        public int Index { get; private set; }

        public StoreLocal(int index)
        {
            Index = index;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // 로컬 변수를 얻어온다
    public class LoadLocal : ICommand
    {
        public int Index { get; private set; }

        public LoadLocal(int index)
        {
            Index = index;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Operator : ICommand
    {
        public OperatorKind Kind { get; private set; }

        public Operator(OperatorKind k)
        {
            Kind = k;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Yield : ICommand
    {
        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }        
    }

    // class/struct support
    public class New : ICommand
    {
        public TypeInfo TypeInfo { get; private set; }

        public New(TypeInfo info)
        {
            TypeInfo = info;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // load field's location at the top of the stack.
    public class LoadField : ICommand
    {
        public int FieldIndex { get; private set; }

        public LoadField(int fieldIndex)
        {
            FieldIndex = fieldIndex;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);            
        }
    }

    public class StoreField : ICommand
    {
        public int FieldIndex { get; private set; }

        public StoreField(int fieldIndex)
        {
            FieldIndex = fieldIndex;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }    
}


