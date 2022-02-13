using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class ClassRuntimeItem : AllocatableRuntimeItem
    {        
        public abstract R.Path.Nested GetActualType(TypeContext typeContext);
        public abstract int GetTotalVariableCount(); // c#은 byte단위로 컨트롤이 힘들어서 그냥 개수로 카운트 한다
        public abstract void FillVariables(ImmutableArray<Value>.Builder builder);
        public abstract int GetBaseIndex();

        class IR0ClassRuntimeItem : ClassRuntimeItem
        {
            IR0GlobalContext globalContext;
            R.ClassDecl decl;
            ImmutableArray<R.Path> memberVarTypes;

            // 모든 RuntimeItem이 만들어 지고 나서 부를수 있기 때문에 한 템포 늦게 간다
            Lazy<ClassRuntimeItem?> baseItem;

            public IR0ClassRuntimeItem(IR0GlobalContext globalContext, R.ClassDecl decl, ImmutableArray<R.Path> memberTypes)
            {    
                this.globalContext = globalContext;
                this.decl = decl;
                this.memberVarTypes = memberTypes;

                if (decl.BaseClass == null)
                    baseItem = new Lazy<ClassRuntimeItem?>((ClassRuntimeItem?)null);
                else
                    baseItem = new Lazy<ClassRuntimeItem?>(() => globalContext.GetRuntimeItem<ClassRuntimeItem>(decl.BaseClass));
            }

            public override R.Name Name => new R.Name.Normal(decl.Name);

            public override R.ParamHash ParamHash => new R.ParamHash(decl.TypeParams.Length, default);

            public override void FillVariables(ImmutableArray<Value>.Builder builder)
            {
                var baseItemValue = baseItem.Value;

                if (baseItemValue != null)
                    baseItemValue.FillVariables(builder);

                foreach (var memberVarType in memberVarTypes)
                {       
                    var memberValue = context.AllocValue(memberVarType);
                    builder.Add(memberValue);
                }
            }

            // To IR0 Module Driver
            // public override ClassInstance AllocInstance(TypeContext typeContext) { }

            //public override Value Alloc(TypeContext typeContext)
            //{   
            //    return new ClassValue();
            //}

            public override R.Path.Nested GetActualType(TypeContext typeContext)
            {
                throw new NotImplementedException();
                // return typeContext.Apply_Nested(classPath);
            }

            public override int GetTotalVariableCount() // c#은 byte단위로 컨트롤이 힘들어서 그냥 개수로 카운트 한다
            {
                return GetBaseIndex() + memberVarTypes.Length;
            }

            public override int GetBaseIndex()
            {
                var baseItemValue = baseItem.Value;

                if (baseItemValue != null)
                    return baseItemValue.GetTotalVariableCount();
                else
                    return 0;
            }
        }
}