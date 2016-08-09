using Gum.Data.AbstractSyntax;
using Gum.Test.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test
{
    //class Environment
    //{
    //    class TypeArgInfo
    //    {
    //        public string Name { get; private set; }
    //        public IType Type { get; private set; }

    //        public TypeArgInfo(string name, IType type) { Name = name; Type = type; }
    //    }

    //    GumMetadata metadata;
    //    IEnumerable<IMetadata> references;
    //    List<IReadOnlyList<string>> usings = new List<IReadOnlyList<string>>();
    //    Stack<Namespace> namespaceStack = new Stack<Namespace>();
    //    List<TypeArgInfo[]> typeArgs = new List<TypeArgInfo[]>();

    //    public Namespace CurNamespace { get { return namespaceStack.Peek(); } }

    //    public Environment(GumMetadata metadata, IEnumerable<IMetadata> references)
    //    {
    //        this.metadata = metadata;
    //        this.references = references;
    //        namespaceStack.Push(metadata.TopNamespace);
    //    }

    //    internal void AddUsing(IEnumerable<string> names)
    //    {
    //        usings.Add(names.ToList());
    //    }

    //    internal void PushNamespace(IEnumerable<string> names)
    //    {
    //        Namespace ns = CurNamespace;

    //        foreach (var name in names)
    //            ns = ns.GetNamespace(name);

    //        namespaceStack.Push(ns);
    //    }

    //    internal void PopNamespace()
    //    {
    //        namespaceStack.Pop();
    //    }

    //    internal void PushObjectTypeVars(IEnumerable<string> typeArgStrings)
    //    {
    //        int count = typeArgStrings.Count();

    //        var typeArgArray = new TypeArgInfo[count];

    //        int i = 0;
    //        foreach (var str in typeArgStrings)
    //        {
    //            typeArgArray[i] = new TypeArgInfo(str, new ObjectTypeVar(i));
    //            i++;
    //        }

    //        typeArgs.Add(typeArgArray);
    //    }

    //    internal void PushFuncTypeVars(IEnumerable<string> typeArgStrings)
    //    {
    //        int count = typeArgStrings.Count();
    //        var typeArgArray = new TypeArgInfo[count];

    //        int i = 0;
    //        foreach (var str in typeArgStrings)
    //        {
    //            typeArgArray[i] = new TypeArgInfo(str, new FuncTypeVar(i));
    //            i++;
    //        }

    //        typeArgs.Add(typeArgArray);
    //    }

    //    internal void PopTypeArgs()
    //    {
    //        typeArgs.RemoveAt(typeArgs.Count - 1);
    //    }


    //    internal IType GetType(IDWithTypeArgs idWithTypeArgs)
    //    {
    //        // Argument가 0이면 일단 TypeArgs에서 검색을 해봅니다
    //        if( idWithTypeArgs.Args.Count() == 0)
    //        {
    //            for(int t = typeArgs.Count - 1; 0 <= t; t--)
    //            {
    //                var typeArgArray = typeArgs[t];

    //                foreach(var typeArg in typeArgArray)
    //                {
    //                    if (typeArg.Name == idWithTypeArgs.Name)
    //                        return typeArg.Type;
    //                }
    //            }
    //        }

    //        var typeDefs = new List<TypeDef>();

    //        // 1. using에서 찾습니다
    //        foreach (var usingNS in usings)
    //        {
    //            foreach (var reference in references)
    //            {
    //                var ns = reference.GetNamespace(usingNS);
    //                if (ns == null) continue;

    //                var def = ns.GetTypeDef(idWithTypeArgs.Name, idWithTypeArgs.Args.Count());
    //                if (def != null)
    //                    typeDefs.Add(def);

    //            }
    //        }

    //        // 2. 현재, 부모 namespace에서 찾습니다
    //        foreach (var ns in namespaceStack.Reverse())
    //        {
    //            var def = ns.GetTypeDef(idWithTypeArgs.Name, idWithTypeArgs.Args.Count());
    //            if (def != null)
    //            {
    //                typeDefs.Add(def);
    //                break; // 자식에서 찾았으면 부모로 올라가지 않는다
    //            }
    //        }

    //        if (typeDefs.Count == 0)
    //        {
    //            throw new InvalidOperationException("타입을 찾을 수 없습니다.");
    //        }

    //        // 에러 상황
    //        if (typeDefs.Count != 1)
    //        {
    //            throw new InvalidOperationException("현재 네임스페이스 혹은 using으로 선언된 네임스페이스들에 이 이름의 선언이 있어서 어느 것을 사용할지 결정할 수 없습니다.");
    //        }

    //        return new TypeWithVar(typeDefs[0], idWithTypeArgs.Args.Select(GetType));
    //    }

    //    internal FuncDef GetOperatorFunc(IType type1, IType type2, BinaryExpKind binaryExpKind)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    internal IType GetBoolType()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    internal IType GetIntType()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    internal IType GetStringType()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    internal FuncDef GetOperatorFunc(dynamic operandType, UnaryExpKind operation)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
