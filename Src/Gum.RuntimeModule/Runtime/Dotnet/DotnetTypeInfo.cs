using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Gum.Runtime.Dotnet
{
    public class DotnetTypeInfo : ITypeInfo
    {
        public ModuleItemId? OuterTypeId { get; }
        public ModuleItemId TypeId { get; }

        // it must be open type
        TypeInfo typeInfo;

        public DotnetTypeInfo(ModuleItemId typeId, TypeInfo typeInfo)
        {
            if (typeInfo.DeclaringType != null)
                OuterTypeId = DotnetMisc.MakeTypeId(typeInfo.DeclaringType);

            TypeId = typeId;

            this.typeInfo = typeInfo;
        }        

        public TypeValue? GetBaseTypeValue()
        {
            throw new NotImplementedException();
        }

        public bool GetMemberFuncId(Name memberFuncId, [NotNullWhen(true)] out ModuleItemId? outFuncId)
        {
            if (memberFuncId.Kind != SpecialName.Normal)
                throw new NotImplementedException();

            try
            {
                var methodInfo = typeInfo.GetMethod(memberFuncId.Text!, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                // NOTICE: type의 경우에는 typeargument에 nested가 다 나오더니, func의 경우 함수의 것만 나온다
                outFuncId = TypeId.Append(memberFuncId.Text!, methodInfo.GetGenericArguments().Length);
                return true;
            }
            catch(AmbiguousMatchException)
            {
                outFuncId = null;
                return false;
            }
        }

        public bool GetMemberTypeId(string name, [NotNullWhen(true)] out ModuleItemId? outTypeId)
        {
            var memberType = typeInfo.GetNestedType(name);
            if (memberType == null)
            {
                outTypeId = null;
                return false;
            }

            outTypeId = TypeId.Append(name, memberType.GenericTypeArguments.Length - typeInfo.GenericTypeArguments.Length);
            return true;
        }

        public bool GetMemberVarId(Name varName, [NotNullWhen(true)] out ModuleItemId? outVarId)
        {
            var candidates = new List<MemberInfo>();
            var memberInfos = typeInfo.GetMember(varName.Text!, MemberTypes.Field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            if (1 < memberInfos.Length || !(memberInfos[0] is FieldInfo fieldInfo))
            {
                outVarId = null;
                return false;
            }

            outVarId = TypeId.Append(varName.Text!, 0);
            return true;
        }

        public IReadOnlyList<string> GetTypeParams()
        {
            return typeInfo.GenericTypeParameters.Select(typeInfo => typeInfo.Name).ToList();                
        }
    }
}
