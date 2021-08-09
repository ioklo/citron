using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Infra;
using Gum.Collections;

namespace Gum.IR0Translator
{
    // X<int>.Y<short>.F_T_int_int<S>
    class FuncValue : CallableValue
    {
        ItemValueFactory itemValueFactory;

        // X<int>.Y<short>
        ItemValueOuter outer;

        // F_int_int
        IModuleFuncInfo funcInfo;

        ImmutableArray<TypeValue> typeArgs;

        public bool IsStatic { get => !funcInfo.IsInstanceFunc(); }
        public bool IsSequence { get => funcInfo.IsSequenceFunc(); }
        
        internal FuncValue(ItemValueFactory itemValueFactory, ItemValueOuter outer, IModuleFuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.itemValueFactory = itemValueFactory;            
            this.outer = outer;
            
            this.funcInfo = funcInfo;
            this.typeArgs = typeArgs;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            if (outer != null)
                outer.FillTypeEnv(builder);

            for(int i = 0; i < typeArgs.Length; i++)
                builder.Add(typeArgs[i]);
        }

        protected override TypeValue MakeTypeValueByMType(M.Type type)
        {
            return itemValueFactory.MakeTypeValueByMType(type);
        }

        public override ImmutableArray<M.Param> GetParameters()
        {
            return funcInfo.GetParameters();
        }

        public TypeValue GetRetType()
        {
            var typeEnv = MakeTypeEnv();
            var retType = funcInfo.GetReturnType();            
            var retTypeValue = itemValueFactory.MakeTypeValueByMType(retType);
            return retTypeValue.Apply_TypeValue(typeEnv);
        }
        
        // IR0 Func를 만들어 줍니다
        public sealed override R.Path GetRPath()
        {
            return GetRPath_Nested();
        }

        public override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply_TypeValue(typeEnv));
            return itemValueFactory.MakeFunc(appliedOuter, funcInfo, appliedTypeArgs);
        }

        public R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(funcInfo.GetName());
            var paramInfos = GetParamInfos();
            var rparamTypes = ImmutableArray.CreateRange(paramInfos, paramInfo => new R.ParamHashEntry(paramInfo.ParamKind, paramInfo.Type.GetRPath()));

            var paramHash = new R.ParamHash(typeArgs.Length, rparamTypes);

            var rtypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.GetRPath());

            return new R.Path.Nested(outer.GetRPath(), rname, paramHash, rtypeArgs);
        }

        public override int GetTotalTypeParamCount()
        {
            return outer.GetTotalTypeParamCount() + funcInfo.GetTypeParams().Length;
        }

        bool IsGlobalFunc()
        {
            return outer is RootItemValueOuter;
        }

        public bool CheckAccess(NormalTypeValue? thisType)
        {
            var accessModifier = funcInfo.GetAccessModifier();

            // 1. 전역 함수일때 
            if (IsGlobalFunc())
            {
                switch (accessModifier)
                {
                    case M.AccessModifier.Public: return true;
                    case M.AccessModifier.Protected: throw new UnreachableCodeException(); // global에 이런게 나올수 없다
                    case M.AccessModifier.Private:

                        // CheckAccess를 부른쪽은 Internal이므로
                        return funcInfo.IsInternal();
                    default: throw new UnreachableCodeException();
                }
            }
            else // 2. 멤버 함수일때
            {
                switch (accessModifier)
                {
                    case M.AccessModifier.Public: return true;
                    case M.AccessModifier.Protected: throw new NotImplementedException();
                    case M.AccessModifier.Private:
                        {
                            // NOTICE: ConstructorValue, MemberVarValue에도 같은 코드가 있다 
                            if (thisType == null) return false;

                            // s.x // 내가 S이거나, S의 Descendant Inner타입이면
                            // access 체크시에는 typeArgs는 상관하지 않는다
                            // moduleTypeInfo끼리 비교하게 할 수 있는가?

                            // S{ F(S s) { s.x; // thisType: S } }
                            // thisType의 outer를 찾아서 계속 간다 (baseType말고 outer)

                            var memberContainerPath = outer.GetRPath() as R.Path.Nested;
                            Debug.Assert(memberContainerPath != null);

                            // path로 비교할 수 있을거 같다
                            R.Path.Nested? curPath = thisType.GetRPath_Nested();
                            while (curPath != null)
                            {
                                // TypeArgs는 빼고 비교한다
                                if (memberContainerPath.Outer.Equals(curPath.Outer) &&
                                    memberContainerPath.Name.Equals(curPath.Name) &&
                                    memberContainerPath.ParamHash.Equals(curPath.ParamHash))
                                    return true;

                                curPath = curPath.Outer as R.Path.Nested;
                            }

                            return false;
                        }
                    default: throw new UnreachableCodeException();
                }
            }

        }
    }
}
