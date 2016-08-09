using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Data.AbstractSyntax;

namespace Gum.Test
{
    class CollectTypeDefinitions // : IFileUnitComponentVisitor, INamespaceComponentVisitor, IMemberComponentVisitor
    {
        TypeEnvironment typeEnv;
        GumTypeReference gumTypeReference;

        public CollectTypeDefinitions(IEnumerable<ITypeReference> typeReference, GumTypeReference gumTypeReference)
        {
            this.typeEnv = new TypeEnvironment(typeReference);
            this.gumTypeReference = gumTypeReference;
        }

        public void Visit(INamespaceComponent comp)
        {
            Visit(comp as dynamic);
        }

        public void Visit(UsingDirective usingDirective)
        {
            typeEnv.AddUsing(usingDirective.NamespaceName);
        }

        public void Visit(NamespaceDecl namespaceDecl)
        {
            typeEnv.PushNamespace(namespaceDecl.Name);
            foreach (var comp in namespaceDecl.Comps)
                Visit(comp);
            typeEnv.PopNamespace();
        }

        public void Visit(VarDecl varDecl)
        {
            // do nothing
        }
         
        public void Visit(FuncDecl funcDecl)
        {
            // do nothing
        }


        public void Visit(ClassDecl classDecl)
        {
            var fullName = typeEnv.GetFullNameOfCurNamespace(classDecl.Name);

            var objectType = gumTypeReference.GetType(fullName);

            foreach(var comp in classDecl.Components)
                VisitMemberComponent(objectType, comp);
        }

        public void Visit(StructDecl structDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitMemberComponent(ObjectType objType, IMemberComponent memberComp)
        {
            VisitMemberComponent(objType, memberComp as dynamic);
        }

        public void VisitMemberComponent(ObjectType objType, MemberFuncDecl memberFuncDecl)
        {
            // 멤버를 추가 합니다. 

            foreach( var typeArg in memberFuncDecl.TypeArgs )
            {
                typeEnv.AddTypeVar(typeArg);
            }
            


            // metadata 
            // void Func<T>(T value) 의 시그니처는? 
            // <T> T -> void
            // 1, FuncArg(0) -> void

            // class SomeClass<T> { U Func<U>(T a, U b) {} }

            // Func의 시그니처 
            // 1, (ClassArg(0), FuncArg(0)), FuncArg(0)

            // Class의 시그니처 
            // typeArg개수 1, 

            // Func의 타입은 Func

            // a.MemberFunc 의 타입은 
            // 

            // objType.AddFunc
            objType.AddFunc(memberFuncDecl.Name, )
        }

        public void VisitMemberComponent(ObjectType objType, MemberVarDecl memberVarDecl)
        {
            throw new NotImplementedException();
        }
    }
}
