using Gum.Core.AbstractSyntax;
using Gum.Core.IL;
using Gum.Core.IL.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler
{
    // Value의 위치를 저장한 register를 돌려 줍니다       
    public class ValueEmitter : IExpVisitor
    {
        // outputs
        EmitResult emitResult;
        int resultIndex;

        CompilerContext context;
        
        public static EmitResult Emit(
            out int resultIndex,
            IExp exp,            
            CompilerContext context)
        {
            var emitter = new ValueEmitter();
            emitter.context = context;

            exp.Visit(emitter);
            resultIndex = emitter.resultIndex;
            
            return emitter.emitResult;
        }

        public static EmitResult Emit(IExp exp, CompilerContext context)
        {
            var emitter = new ValueEmitter();
            emitter.context = context;

            exp.Visit(emitter);
            // resultIndex 전달 부분이 없습니다

            return emitter.emitResult;
        }

        // assignment expression, value
        public void Visit(AssignExp assignExp)
        {
            int leftReg;
            var leftExp = LocationEmitter.Emit(out leftReg, assignExp.Left, context);

            int rightReg;
            var rightExp = ValueEmitter.Emit(out rightReg, assignExp.Exp, context);

            var emitResult = new EmitResult();
            emitResult.Push(leftExp);
            emitResult.Push(rightExp);
            emitResult.Push(new Store(leftReg, rightReg));

            this.resultIndex = rightReg;
            this.emitResult = emitResult;
        }
        
        // Variable의 값을 돌려준다
        public void Visit(VariableExp exp)
        {
            int localIndex;            
            if (context.GetLocalIndex(out localIndex, exp.Name))
            {
                this.resultIndex = localIndex;
                this.emitResult = new EmitResult();
                return;
            }

            int globalIndex;            
            if (context.GetGlobalIndex(out globalIndex, exp.Name))
            {
                string globalType = context.GetGlobalType(globalIndex);

                int refIndex = context.AddLocal(globalType);

                var emitResult = new EmitResult();
                
                emitResult.Push(new GlobalRef(refIndex, globalIndex));
                emitResult.Push(new Load(resultIndex, refIndex));

                this.resultIndex = refIndex;                
                this.emitResult = emitResult;
                return;
            }

            throw new InvalidOperationException();
        }

        public void Visit(IntegerExp exp)
        {
            int local = context.AddLocal("int");
            var emitResult = new EmitResult();
            emitResult.Push(new Move(local, new IntValue(exp.Value)));
            this.resultIndex = local;
            this.emitResult = emitResult;
        }
        
        public void Visit(StringExp exp)
        {
            int local = context.AddLocal("string");
            var emitResult = new EmitResult();
            emitResult.Push(new Move(local, new RefValue(new StringValue(exp.Value))));
            this.resultIndex = local;
            this.emitResult = emitResult;
        }

        public void Visit(BoolExp exp)
        {
            int local = context.AddLocal("bool");
            var emitResult = new EmitResult();
            emitResult.Push(new Move(local, new BoolValue(exp.Value)));
            this.resultIndex = local;
            this.emitResult = emitResult;
        }
        
        public void Visit(BinaryExp exp)
        {
            // 1. 예외, short-circuit처리 (lazy evaluation이기 때문에 call로 변경할 수 없습니다)

            int result1;
            var exp1 = ValueEmitter.Emit(out result1, exp.Operand1, context);

            int result2;
            var exp2 = ValueEmitter.Emit(out result2, exp.Operand2, context);

            if( exp.Operation == BinaryExpKind.And )
            {
                var result = context.AddLocal("bool");

                var emitResult = new EmitResult();
                emitResult.Push(exp1);
                emitResult.PushIfNotJump(result1, "false");
                emitResult.Push(exp2);
                emitResult.Push(new MoveReg(result, result2));
                emitResult.PushJump("exit");

                emitResult.PushLabel("false");
                emitResult.Push(new Move(result, new BoolValue(false)));

                emitResult.PushLabel("exit");

                this.resultIndex = result;
                this.emitResult = emitResult;
                return;
            }

            if (exp.Operation == BinaryExpKind.Or)
            {
                var result = context.AddLocal("bool");

                var emitResult = new EmitResult();
                emitResult.Push(exp1);
                emitResult.PushIfNotJump(result1, "false");
                emitResult.Push(new Move(result, new BoolValue(true)));
                emitResult.PushJump("exit");

                emitResult.PushLabel("false");
                emitResult.Push(exp2);
                emitResult.Push(new MoveReg(result, result2));

                emitResult.PushLabel("exit");               

                this.resultIndex = result;
                this.emitResult = emitResult;
                return;
            }
            
            {
                string type1 = context.GetLocalType(result1);
                string type2 = context.GetLocalType(result2);

                var func = context.GetBinOpFunc(exp.Operation, type1, type2);

                int resultIndex = context.AddLocal("");
                int funcReg = context.AddLocal("ref");                

                // call Operation
                var emitResult = new EmitResult();
                emitResult.Push(exp1);
                emitResult.Push(exp2);
                emitResult.Push(new Move(funcReg, new RefValue(new FuncValue(func))));
                emitResult.Push(new StaticCall(resultIndex, funcReg, new[] { result1, result2 } ));

                this.resultIndex = resultIndex;
                this.emitResult = emitResult;
                return;
            }
        }

        public void Visit(UnaryExp unaryExp)
        {
            int result;
            var exp = ValueEmitter.Emit(out result, unaryExp.Operand, context);

            string type = context.GetLocalType(result);
            var func = context.GetUnOpFunc(unaryExp.Operation, type);
            
            int resultIndex = context.AddLocal("");
            int funcReg = context.AddLocal("ref");

            // call Operation
            var emitResult = new EmitResult();
            emitResult.Push(exp);
            emitResult.Push(new Move(funcReg, new RefValue(new FuncValue(func))));
            emitResult.Push(new StaticCall(resultIndex, funcReg, new[] { result }));

            this.resultIndex = resultIndex;
            this.emitResult = emitResult;
            return;
        }

        // Call, 후에 ExternCall은 Call로 바뀐다
        public void Visit(CallExp exp)
        {
            int funcReg;
            var funcExp = ValueEmitter.Emit(out funcReg, exp.FuncExp, context);

            var argIndices = new List<int>();
            var argExps = new List<EmitResult>();
            foreach( var arg in exp.Args )
            {
                int argIndex;
                var argExp = ValueEmitter.Emit(out argIndex, arg, context);

                argIndices.Add(argIndex);
                argExps.Add(argExp);
            }

            var emitResult = new EmitResult();
            emitResult.Push(funcExp);

            foreach(var argExp in argExps)
                emitResult.Push(argExp);
          
            // TODO: Virtual 함수인지 확인하는 코드
            // TODO: 리턴값이 필요한지 보고 필요없으면 -1을 넣어주는 코드
            // 첫번째 인자는 이미 binding 되어 있다고 봅니다
            int resultIndex = context.AddLocal("");
            emitResult.Push(new StaticCall(resultIndex, funcReg, argIndices));

            this.resultIndex = resultIndex;
            this.emitResult = emitResult;
        }

        public void Visit(NewExp ne)
        {
            var typeValue = context.GetTypeValue(ne.TypeName);
            var typeIndex = context.AddLocal("type");
            var resultIndex = context.AddLocal("ref");            

            var argIndices = new List<int>();
            var argExps = new List<EmitResult>();
            foreach( var arg in exp.Args )
            {
                int argIndex;
                var argExp = ValueEmitter.Emit(out argIndex, arg, context);

                argIndices.Add(argIndex);
                argExps.Add(argExp);
            }

            var emitResult = new EmitResult();
            emitResult.Push(new Move(typeIndex, new RefValue(typeValue)));

            var typeArgIndices = new List<int>();
            foreach(var typeArgName in ne.TypeArgs)
            {
                int typeArgIndex = context.AddLocal("ref");
                var typeArgType = context.GetTypeValue(typeArgName);

                emitResult.Push(new Move(typeArgIndex, new RefValue(typeArgType)));
                typeArgIndices.Add(typeArgIndex);
            }

            // New로 만들고
            emitResult.Push(new New(resultIndex, typeIndex, typeArgIndices));
            
            // 생성자 호출
            var constructorFunc = context.GetFuncValue(ne.TypeName, "$constructor");
            foreach (var argExp in argExps)
                emitResult.Push(argExp);
            emitResult.Push(new Move(constructorFuncIndex, new RefValue(constructorFunc)));
            emitResult.Push(new StaticCall(-1, constructorFuncIndex, argIndices));

            this.resultIndex = resultIndex;
            this.emitResult = emitResult;
        }

        public void Visit(FieldExp fieldExp)
        {
            int objectIndex;
            var objectExp = ValueEmitter.Emit(out objectIndex, fieldExp.Exp, context);

            int resultIndex = context.AddLocal("ref");
            var typeValue = context.GetTypeValue(context.GetLocalType(objectIndex));

            int fieldIndex = context.GetFieldIndex(typeValue, fieldExp.ID);

            var emitResult = new EmitResult();
            emitResult.Push(objectExp);
            emitResult.Push(new FieldRef(resultIndex, objectIndex, fieldIndex));

            this.resultIndex = resultIndex;
            this.emitResult = emitResult;
        }
    }
}
