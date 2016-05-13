using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 메타데이타, 
namespace Gum.Metadatas
{
    // Namespace
    //   - 자식 MetadataComponent를 모두 알고 있어야 하는가..
    // 
    // MetadataComponent
    //   - Namespace
    // 
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

        internal Namespace GetOrAddNamespace(string name)
        {
            throw new NotImplementedException();
            //var ns = GetNamespace(name);
            //if (ns != null) return ns;

            //ns = new Namespace(this, name);
            //children.Add(ns);
            //return ns;
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
