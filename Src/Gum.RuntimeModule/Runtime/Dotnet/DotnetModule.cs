using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Gum.Runtime.Dotnet
{
    // 닷넷 어셈블리 하나를 모듈처럼 쓸 수 있게 해주는 부분입니다
    public class DotnetModule : IModule
    {
        Assembly assembly;

        // 모듈 이름은 어셈블리 이름 그대로 씁니다
        public string ModuleName => assembly.FullName;        
        
        // 최상위 타입 정보를 담고 있는건지 모든 타입 정보를 담고 있는건지 불분명합니다

        // A.B.C        
        public DotnetModule(Assembly assembly)
        {
            this.assembly = assembly;
        }
        
        private TypeValue MakeTypeValue(Type baseType)
        {
            throw new NotImplementedException();
        }        

        string MakeDotnetName(ModuleItemId typeId)
        {
            var sb = new StringBuilder();

            void MakeNameInner(ModuleItemId typeId)
            {
                if (typeId.Outer != null)
                    MakeNameInner(typeId.Outer);

                sb.Append('.');

                sb.Append(typeId.Name);

                if (typeId.TypeParamCount != 0)
                {
                    sb.Append('`');
                    sb.Append(typeId.TypeParamCount);
                }
            }

            MakeNameInner(typeId);
            return sb.ToString();
        }

        public bool GetTypeInfo(ModuleItemId typeId, [NotNullWhen(true)] out ITypeInfo? outType)
        {
            var dotnetType = assembly.GetType(MakeDotnetName(typeId)); 

            if (dotnetType == null)
            {
                outType = null;
                return false;
            }

            outType = new DotnetTypeInfo(typeId, dotnetType.GetTypeInfo());
            return true;
        }

        public bool GetFuncInfo(ModuleItemId funcId, [NotNullWhen(true)] out FuncInfo? outFunc)
        {
            outFunc = null;
            return false;
        }

        public bool GetVarInfo(ModuleItemId typeId, [NotNullWhen(true)] out VarInfo? outVar)
        {
            outVar = null;
            return false;
        }

        public FuncInst GetFuncInst(DomainService domainService, FuncValue fv)
        {
            throw new NotImplementedException();
        }

        public TypeInst GetTypeInst(DomainService domainService, TypeValue.Normal ntv)
        {
            var dotnetType = assembly.GetType(MakeDotnetName(ntv.TypeId));
            var dotnetTypeInfo = dotnetType.GetTypeInfo();

            //if (dotnetTypeInfo.IsGenericType)
            //{
            //    if (0 < ntv.TypeArgs.Length)
            //    {
            //        // List<int> => List<Cont(int)> => List<Value<int>>
            //        // 
            //        // List<X<int>> => List<Cont(QsDotnetTypeInfo(X<int>))> => List<X<int>>
            //        // List<Y<int>> => List<Cont(QsTypeInfo(Y<int>))> => 

            //        // dotnetType.MakeGenericType(typeof(int));
            //        // dotnetType.MakeGenericType(typeof(Value<int>));

            //        // List<X>.Add(X);
            //        dotnetType.MakeGenericType()
            //    }
            //}

            // return new QsDotnetTypeInst(ntv);

            // 차차 생각하기로 한다
            throw new NotImplementedException();
        }

        public void OnLoad(DomainService domainService)
        {

        }
    }
}
