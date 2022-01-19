using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;
using Gum.Infra;
using Gum.Analysis;

namespace Gum.Analysis
{
    // 분석 과정에서 사용하는 멤버 변수의 간접 참조, 타입 인자 정보를 포함한다
    public class MemberVarValue : ItemValue
    {   
        ItemValueFactory factory;
        NormalTypeValue outer;
        IModuleMemberVarInfo info;

        public M.Name Name { get => info.GetName(); }
        public bool IsStatic { get => info.IsStatic(); }
        
        public MemberVarValue(ItemValueFactory factory, NormalTypeValue outer, IModuleMemberVarInfo info)
        {
            this.factory = factory;
            this.outer = outer;
            this.info = info;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            outer.FillTypeEnv(builder);
        }

        public ITypeSymbol GetTypeValue()
        {
            var typeEnv = MakeTypeEnv();
            var type = info.GetDeclType();

            var typeValue = factory.MakeTypeValueByMType(type);
            return typeValue.Apply(typeEnv);
        }

        public R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(Name);

            return new R.Path.Nested(outer.GetRPath_Nested(), rname, R.ParamHash.None, default);
        }

        public sealed override R.Path GetRPath()
        {
            return GetRPath_Nested();
        }

        public override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply_NormalTypeValue(typeEnv);
            return factory.MakeMemberVarValue(appliedOuter, info);
        }

        public bool CheckAccess(NormalTypeValue? thisType)
        {
            var accessModifier = info.GetAccessModifier();

            switch(accessModifier)
            {
                case M.AccessModifier.Public: return true;
                case M.AccessModifier.Protected: throw new NotImplementedException();
                case M.AccessModifier.Private:
                    {
                        // NOTICE: ConstructorValue, FuncValue에도 같은 코드가 있다 
                        if (thisType == null) return false;

                        // s.x // 내가 S이거나, S의 Descendant Inner타입이면
                        // access 체크시에는 typeArgs는 상관하지 않는다
                        // moduleTypeInfo끼리 비교하게 할 수 있는가?

                        // S{ F(S s) { s.x; // thisType: S } }
                        // thisType의 outer를 찾아서 계속 간다 (baseType말고 outer)

                        var memberContainerPath = outer.GetRPath_Nested();

                        // path로 비교할 수 있을거 같다
                        R.Path.Nested? curPath = thisType.GetRPath_Nested();                        
                        while(curPath != null)
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

        public override int GetTotalTypeParamCount()
        {
            return 0;
        }
    }
}