using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Lang.CoreIL
{




    public class GlobalRefCmd : ICommand
    {
        public int DestReg { get; private set; }
        public int Index { get; private set; }

        public GlobalRefCmd(int destReg, int index)
        {
            this.DestReg = destReg;
            this.Index = index;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LocalRefCmd : ICommand
    {
        public int DestReg { get; private set; }
        public int Index { get; private set; }

        public LocalRefCmd(int destReg, int index)
        {
            this.DestReg = destReg;
            this.Index = index;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class FieldRefCmd : ICommand
    {
        public int DestReg { get; private set; }
        public int SrcRefReg { get; private set; }
        public int Index { get; private set; }

        public FieldRefCmd(int destReg, int srcRefReg, int index)
        {
            this.DestReg = destReg;
            this.SrcRefReg = srcRefReg;
            this.Index = index;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NewCmd : ICommand
    {
        public int DestReg { get; private set; }
        public int Type { get; private set; }
        public IReadOnlyList<int> TypeArgs { get; private set; }

        public NewCmd(int destReg, int type, IEnumerable<int> typeArgs)
        {
            this.DestReg = destReg;
            this.Type = type;
            this.TypeArgs = typeArgs.ToList();
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LoadCmd : ICommand
    {
        public int Dest { get; private set; }
        public int SrcRef { get; private set; }

        public LoadCmd(int dest, int srcRef)
        {
            this.Dest = dest;
            this.SrcRef = srcRef;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class StoreCmd : ICommand
    {
        public int DestRef { get; private set; }
        public int Src { get; private set; }

        public StoreCmd(int destRef, int src)
        {
            this.DestRef = destRef;
            this.Src = src;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class MoveCmd : ICommand
    {
        public int Dest { get; private set; }
        public IValue Value { get; private set; }

        public MoveCmd(int dest, IValue value)
        {
            this.Dest = dest;
            this.Value = value;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class MoveRegCmd : ICommand
    {
        public int DestReg { get; private set; }
        public int SrcReg { get; private set; }

        public MoveRegCmd(int destReg, int srcReg)
        {
            this.DestReg = destReg;
            this.SrcReg = srcReg;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class JumpCmd : ICommand
    {
        public int Block { get; private set; }

        public JumpCmd(int block)
        {
            this.Block = block;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class IfNotJumpCmd : ICommand
    {
        public int Cond { get; private set; }
        public int Block { get; private set; }

        public IfNotJumpCmd(int cond, int block)
        {
            this.Cond = cond;
            this.Block = block;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class StaticCallCmd : ICommand
    {
        public int Ret { get; private set; }
        public int Func { get; private set; }
        public IReadOnlyList<int> Args { get; private set; }

        public StaticCallCmd(int ret, int func, IEnumerable<int> args)
        {
            this.Ret = ret;
            this.Func = func;
            this.Args = args.ToList();
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class VirtualCallCmd : ICommand
    {
        public int Ret { get; private set; }
        public int Func { get; private set; }
        public IReadOnlyList<int> Args { get; private set; }

        public VirtualCallCmd(int ret, int func, IEnumerable<int> args)
        {
            this.Ret = ret;
            this.Func = func;
            this.Args = args.ToList();
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ReturnCmd : ICommand
    {
        public int Value { get; private set; }

        public ReturnCmd(int value)
        {
            this.Value = value;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public interface ICommand
    {
        void Visit(ICommandVisitor visitor);
    }

    public interface ICommandVisitor
    {
        void Visit(GlobalRefCmd globalRefCmd);
        void Visit(LocalRefCmd localRefCmd);
        void Visit(FieldRefCmd fieldRefCmd);
        void Visit(NewCmd newCmd);
        void Visit(LoadCmd loadCmd);
        void Visit(StoreCmd storeCmd);
        void Visit(MoveCmd moveCmd);
        void Visit(MoveRegCmd moveRegCmd);
        void Visit(JumpCmd jumpCmd);
        void Visit(IfNotJumpCmd ifNotJumpCmd);
        void Visit(StaticCallCmd staticCallCmd);
        void Visit(VirtualCallCmd virtualCallCmd);
        void Visit(ReturnCmd returnCmd);
    }

}
