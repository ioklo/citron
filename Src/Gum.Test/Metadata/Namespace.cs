using Gum.Test.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
{
    // 네임스페이스를 정의합니다
    class Namespace : MetadataComponent
    {   
        public IReadOnlyList<MetadataComponent> Children { get { return children; } }
        private List<MetadataComponent> children;

        public Namespace(Namespace ns, string name)
            : base(ns, name)
        {
            children = new List<MetadataComponent>();
        }
        

        // 검색 함수를 여기에 둘까? 저쪽에 둘까?
        // 전역 검색함수가 있어야 할 것 같다.. 여기에는 컴포넌트만 있고

        internal Namespace GetNamespace(string name)
        {
            foreach (var ns in children.OfType<Namespace>())
                if (ns.Name == name) return ns;

            return null;
        }

        internal Namespace GetOrAddNamespace(string name)
        {
            var ns = GetNamespace(name);
            if (ns != null) return ns;

            ns = new Namespace(this, name);
            children.Add(ns);
            return ns;
        }

        internal TypeDef GetTypeDef(string name, int typeParamCount )
        {
            foreach (var typeDef in children.OfType<TypeDef>())
            {
                if (typeDef.Name == name && 
                    typeDef.TypeVarCount == typeParamCount) return typeDef;
            }

            return null;
        }


        internal VarDef AddVariable(IType Type, string name)
        {
            VarDef varDef = new VarDef(this, Type, name);
            children.Add(varDef);
            return varDef;
        }
        
        internal FuncDef AddFunction(string name, int typeVarCount, IType returnType, IEnumerable<IType> argTypes)
        {
            var funcDef = new FuncDef(this, name, typeVarCount, returnType, argTypes);
            children.Add(funcDef);
            return funcDef;
        }

        internal TypeDef AddType(string name, int typeVarCount)
        {
            var typeDef = new TypeDef(this, name, typeVarCount);
            children.Add(typeDef);
            return typeDef;
        }

        internal virtual string GetChildFullName(string name)
        {
            return FullName + "." + name;
        }        
    }
}
