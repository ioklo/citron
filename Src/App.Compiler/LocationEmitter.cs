using Gum.Core.AbstractSyntax;
using Gum.Core.IL;
using Gum.Core.IL.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler
{
    // 
    // LocationEmitter: (Domain, Exp) -> (Reg, Blocks, Domain)
    public class LocationEmitter : IExpVisitor
    {
        int resultIndex;
        EmitResult emitResult;
        
        CompilerContext context;
        
        public static EmitResult Emit(
            out int resultIndex,
            IExp exp, 
            CompilerContext context
            )
        {
            var emitter = new LocationEmitter();
            emitter.context = context;            
            
            exp.Visit(emitter);

            resultIndex = emitter.resultIndex;
            return emitter.emitResult;
        }       
        
        public void Visit(AssignExp exp)
        {                           
            throw new InvalidOperationException();
        }

        // a = ..;
        // variable이 가리키는 위치를 돌려 줍니다. 
        public void Visit(VariableExp exp)
        {
            // 현재 컨텍스트에서 Name이 무엇을 가리키는지
            int localIndex;
            if( context.GetLocal(out localIndex, exp.Name) )
            {
                emitResult = new EmitResult();
                emitResult.Push(new LocalRef(resultIndex, localIndex));
                return;
            }

            int globalIndex;
            if( context.GetGlobal(out globalIndex, exp.Name))
            {
                emitResult = new EmitResult();
                emitResult.Push(new GlobalRef(resultIndex, globalIndex));
                return;
            }

            throw new InvalidOperationException();
            
        }

        // 값들은 위치가 없다
        public void Visit(IntegerExp exp)
        {
            throw new InvalidOperationException();
        }

        public void Visit(StringExp exp)
        {
            throw new InvalidOperationException();
        }

        public void Visit(BoolExp exp)
        {
            throw new InvalidOperationException();
        }

        // binary exp를 함수콜로 변형시켜버릴까, 속도가 느려진다 -> 나중에 call.int.operator+만 새로 파면 됩니다. 
        // (a + b)
        // a.operator+(b)
        // push local(a)
        // load
        // push local(b)
        // load 
        // call int.operator+
        // 지금은 그냥 놔둡니다. 두 함수의 결과는 임시변수 일수도 있고, 아닐 수도 있죠
        public void Visit(BinaryExp exp)
        {
            // 일단 call을 작성하고 나면 여기도 
        }

        // (-a) =
        public void Visit(UnaryExp exp)
        {
            throw new NotImplementedException();
        }

        // f() = 2; // 불가능.. 
        public void Visit(CallExp exp)
        {
            throw new InvalidOperationException();
        }

        
        public void Visit(NewExp exp)
        {
            throw new InvalidOperationException();
        }

        // a.b.c = 2;
        public void Visit(FieldExp fieldExp)
        {
            var emitResult = new EmitResult();

            // new ValueType().b = 2;
            // refType이어야 의미가 있다?
            

            var refType = ValueEmitter.Emit(fieldExp.Exp) as RefType;

            ValueEmitter.Emit()

            if (refType == null)
                throw new InvalidOperationException();

            // field가 없을 때
            int fieldID;
            if (!refType.TryGetFieldID(fieldExp.ID, out fieldID))
                throw new InvalidOperationException();

            AddInst(new Push(fieldID));            

            // loc, field => loc
            AddInst(new FieldLoc());
        }
    }
}
