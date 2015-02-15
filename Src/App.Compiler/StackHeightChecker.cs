using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.App.Compiler.Exception;
using Gum.Core.IL;

namespace Gum.App.Compiler
{
    class StackHeightChecker : Gum.Core.IL.ICommandVisitor
    {
        public FuncInfo CurFunc { get; set; }
        public Program Program { get; set; }
        public int StackHeight { get; set; }

        public void Visit(Core.IL.StoreLocal p)
        {
            StackHeight--;
        }

        public void Visit(Core.IL.LoadLocal p)
        {
            StackHeight++;
        }

        public void Visit(Core.IL.New p)
        {
            StackHeight++;
        }

        public void Visit(Core.IL.LoadField p)
        {
            // 
            
        }

        public void Visit(Core.IL.StoreField p)
        {
            StackHeight -= 2;
        }

        public void Visit(Core.IL.Jump p)
        {
            
        }

        public void Visit(Core.IL.IfJump p)
        {
            StackHeight--;
        }

        public void Visit(Core.IL.IfNotJump p)
        {
            StackHeight--;
        }

        public void Visit(Core.IL.StaticCall p)
        {
            // FuncInfo
            StackHeight = StackHeight - p.FuncInfo.ArgCount + p.FuncInfo.RetValCount;
        }

        public void Visit(Core.IL.Return p)
        {
            // FuncInfo
            StackHeight -= CurFunc.RetValCount;
        }

        public void Visit(Core.IL.Yield p)
        {
            throw new NotImplementedException();
            // StackHeight -= CurFunc.RetValCount;
        }

        public void Visit(Core.IL.Push p)
        {
            StackHeight++;
        }

        public void Visit(Core.IL.Dup p)
        {
            StackHeight++;
        }

        public void Visit(Core.IL.Pop p)
        {
            StackHeight--;
        }

        public void Visit(Core.IL.Operator op)
        {
            StackHeight = StackHeight - op.Kind.ArgCount() + op.Kind.RetValCount();
        }

        class JumpTableEntry
        {
            public int StackHeight { get; set; }
            public JumpTableEntry()
            {
                StackHeight = -1;
            }
        }

        private void Merge(object[] stackHeights, int point, int stackHeight, Worklist worklist)
        {
            int height = (int)stackHeights[point];

            if (height == -1)
            {
                stackHeights[point] = stackHeight;
                worklist.Push(point);
                return;
            }

            if (height != stackHeight)
                throw new StackHeightMismatchException();
        }

        // Check
        public void Check(Program prog)
        {
            Program = prog;

            // 함수 단위로 체크
            foreach (var func in prog.Funcs)
            {
                if (func.Extern) continue;

                CurFunc = func;

                // command 수만큼 Entry가 있어야 한다. (하지만 정보는 다 채우지 않는다)
                // 해당 point를 시작하기 전의 StackHeight
                object[] stackHeights = new object[func.Commands.Count];

                foreach (var point in func.JumpTable)
                    stackHeights[point] = -1;                

                // entry는 0으로 시작
                stackHeights[0] = 0;                

                Worklist worklist = new Worklist();
                worklist.Push(0);

                while (worklist.Count != 0)
                {
                    int point = worklist.Pop();

                    StackHeight = (int)stackHeights[point];

                    // jumpPoint가 나올때까지 
                    do
                    {
                        ICommand cmd = func.Commands[point];
                        cmd.Visit(this);

                        if (cmd is IfJump)
                        {
                            int pt = func.JumpTable[(cmd as IfJump).Point];
                            Merge(stackHeights, pt, StackHeight, worklist);
                        }
                        else if (cmd is IfNotJump)
                        {
                            int pt = func.JumpTable[(cmd as IfNotJump).Point];
                            Merge(stackHeights, pt, StackHeight, worklist);
                        }                        
                            

                        point++;

                        if (stackHeights.Length == point)
                        {
                            if (StackHeight != 0)
                                throw new StackHeightMismatchException();

                            break;
                        }
                    } while (stackHeights[point] == null);

                    if (stackHeights.Length == point)
                        continue;

                    // jumpPoint에 도달했고, 비어있으면
                    Merge(stackHeights, point, StackHeight, worklist);
                }
            }
        }
        
    }
}
