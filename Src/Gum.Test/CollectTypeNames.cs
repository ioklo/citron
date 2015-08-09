using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Lang.AbstractSyntax;
using Gum.Test.Metadata;

namespace Gum.Test
{
    // 1. TypeDecl만 먼저 찾습니다 (Namespace, ID)
    //    최상위 Func, Var는 TypeDecl이 있어야 type을 알 수 있기 때문에 이번에 찾지 않습니다
    //    TypeDecl의 BaseType도 찾지 않습니다
    class CollectTypeNames : IFileUnitComponentVisitor, INamespaceComponentVisitor
    {
        GumMetadata metadata;
        Stack<Namespace> namespaceStack = new Stack<Namespace>();

        Namespace CurNamespace { get { return namespaceStack.Peek();  } }

        public CollectTypeNames()
        {
            metadata = new GumMetadata();
            namespaceStack.Push(metadata.TopNamespace);
        }

        public void Visit(UsingDirective usingDirective)
        {
            // 지금 Using이 필요하지 않습니다
        }

        public void Visit(NamespaceDecl namespaceDecl)
        {
            foreach (var name in namespaceDecl.Names)
            {
                var childNamespace = CurNamespace.GetOrAddNamespace(name);
                namespaceStack.Push(childNamespace);
            }

            foreach (var comp in namespaceDecl.Comps)
                Visit(comp as dynamic);

            for (int t = 0; t < namespaceDecl.Names.Count; t++ )
                namespaceStack.Pop();
        }

        public void Visit(VarDecl varDecl)
        {
            // Variable은 지금 보지 않고요
        }

        public void Visit(FuncDecl funcDecl)
        {
            // Function도 보지 않고요
        }

        public void Visit(ClassDecl classDecl)
        {
            CurNamespace.AddType(classDecl.Name, classDecl.TypeVars.Count);

            // TODO: nested class 처리
        }

        public void Visit(StructDecl structDecl)
        {
            CurNamespace.AddType(structDecl.Name, structDecl.TypeVars.Count);

            // TODO: nested class 처리
        }

        public void Visit(FileUnit fileUnit)
        {
            foreach(var comp in fileUnit.Comps)
                Visit(comp as dynamic);
        }

        internal static GumMetadata Collect(FileUnit comp)
        {
            var collector = new CollectTypeNames();
            collector.Visit(comp);
            return collector.metadata;
        }
    }
}
