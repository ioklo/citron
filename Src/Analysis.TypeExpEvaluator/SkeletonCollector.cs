using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

using Citron.Infra;
using Citron.Collections;

using S = Citron.Syntax;
using M = Citron.Module;

namespace Citron.Analysis
{   
    struct SkeletonCollector
    {        
        Skeleton skeleton; // 만들어지고 있는 스켈레톤

        SkeletonCollector(Skeleton skeleton)
        {
            this.skeleton = skeleton;
        }

        public static Skeleton Collect(S.Script script)
        {
            var skel = new Skeleton(SkeletonKind.Module, M.Name.TopLevel, 0, 0, null);

            var collector = NewSkeletonCollector(skel);
            collector.VisitScript(script);

            return skel;
        }

        // 타입 파라미터를 인자로 받아서, 자식 Skeleton으로 넣는다
        static SkeletonCollector NewSkeletonCollector(Skeleton skeleton)
        {
            return new SkeletonCollector(skeleton);
        }

        void AddTypeParams(ImmutableArray<S.TypeParam> typeParams)
        {
            foreach (var typeParam in typeParams)
                skeleton.AddMember(SkeletonKind.TypeVar, new M.Name.Normal(typeParam.Name), 0);
        }

        // 이름이랑 타입인자가 같은 것이 이미 있다면 index를 하나 증가시켜서 ChildPath를 만든다        

        void VisitEnumElem(S.EnumElemDecl enumElem)
        {
            skeleton.AddMember(SkeletonKind.EnumElem, new M.Name.Normal(enumElem.Name), 0);
        }

        // namespace일 경우 어떻게 할거냐
        void VisitEnumDecl(S.EnumDecl enumDecl)
        {
            var enumDeclSkel = skeleton.AddMember(SkeletonKind.Enum, new M.Name.Normal(enumDecl.Name), enumDecl.TypeParams.Length);

            var newCollector = NewSkeletonCollector(enumDeclSkel);
            newCollector.AddTypeParams(enumDecl.TypeParams);

            foreach (var enumElem in enumDecl.Elems)
            {
                newCollector.VisitEnumElem(enumElem);
            }
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            var structDeclSkel = skeleton.AddMember(SkeletonKind.Struct, new M.Name.Normal(structDecl.Name), structDecl.TypeParams.Length);
            var newCollector = NewSkeletonCollector(structDeclSkel);

            newCollector.AddTypeParams(structDecl.TypeParams);

            foreach(var elem in structDecl.MemberDecls)
            {
                switch(elem)
                {
                    case S.StructMemberTypeDecl typeDecl:
                        newCollector.VisitTypeDecl(typeDecl.TypeDecl);
                        break;

                    case S.StructMemberFuncDecl memberFuncDecl:
                        newCollector.VisitStructMemberFuncDecl(memberFuncDecl);
                        break;

                }
            }
        }

        void VisitStructMemberFuncDecl(S.StructMemberFuncDecl decl)
        {
            var declSkel = skeleton.AddMember(SkeletonKind.StructMemberFunc, new M.Name.Normal(decl.Name), decl.TypeParams.Length, decl);

            var newCollector = NewSkeletonCollector(declSkel);
            newCollector.AddTypeParams(decl.TypeParams);
        }

        void VisitClassDecl(S.ClassDecl classDecl)
        {
            var classDeclSkel = skeleton.AddMember(SkeletonKind.Class, new M.Name.Normal(classDecl.Name), classDecl.TypeParams.Length);

            var newCollector = NewSkeletonCollector(classDeclSkel);
            newCollector.AddTypeParams(classDecl.TypeParams);

            foreach (var memberDecl in classDecl.MemberDecls)
            {
                switch (memberDecl)
                {
                    case S.ClassMemberTypeDecl typeDecl:
                        newCollector.VisitTypeDecl(typeDecl.TypeDecl);
                        break;

                    case S.ClassMemberFuncDecl classMemberFuncDecl:
                        newCollector.VisitClassMemberFuncDecl(classMemberFuncDecl);
                        break;
                }
            }
        }

        void VisitClassMemberFuncDecl(S.ClassMemberFuncDecl decl)
        {
            var declSkel = skeleton.AddMember(SkeletonKind.ClassMemberFunc, new M.Name.Normal(decl.Name), decl.TypeParams.Length, decl);

            // class C { void F<T>() { } }
            var newCollector = NewSkeletonCollector(declSkel);
            newCollector.AddTypeParams(decl.TypeParams);
        }

        void VisitTypeDecl(S.TypeDecl typeDecl)
        {
            switch (typeDecl)
            {
                case S.EnumDecl enumDecl: VisitEnumDecl(enumDecl); break;
                case S.StructDecl structDecl: VisitStructDecl(structDecl); break;
                case S.ClassDecl classDecl: VisitClassDecl(classDecl); break;
                default: throw new UnreachableCodeException();
            }
        }

        // void Func<T>() ... 라면
        void VisitGlobalFuncDecl(S.GlobalFuncDecl globalFuncDecl)
        {
            var funcSkel = skeleton.AddMember(SkeletonKind.GlobalFunc, new M.Name.Normal(globalFuncDecl.Name), globalFuncDecl.TypeParams.Length, globalFuncDecl);

            var newCollector = NewSkeletonCollector(funcSkel);
            newCollector.AddTypeParams(globalFuncDecl.TypeParams);
        }

        Skeleton GetOrAddNamespaceSkel(ImmutableArray<string> names)
        {
            var curSkel = skeleton;

            foreach (var name in names)
            {
                var namespaceName = new M.Name.Normal(name);
                var namespaceSkel = curSkel.GetMember(namespaceName, 0, 0); // index는 항상 0을 유지한다

                // 없다면, 
                if (namespaceSkel == null)
                    namespaceSkel = curSkel.AddMember(SkeletonKind.Namespace, namespaceName, 0);

                curSkel = namespaceSkel;
            }

            return curSkel;
        }

        void VisitNamespaceDecl(S.NamespaceDecl namespaceDecl)
        {
            // 중첩이름 처리 NS1.NS2.NS3
            var namespaceSkel = GetOrAddNamespaceSkel(namespaceDecl.Names);
            var newSkeletonCollector = NewSkeletonCollector(namespaceSkel);

            foreach (var elem in namespaceDecl.Elements)
            {
                switch(elem)
                {
                    case S.NamespaceDeclNamespaceElement namespaceElem:
                        newSkeletonCollector.VisitNamespaceDecl(namespaceElem.NamespaceDecl);
                        break;

                    case S.GlobalFuncDeclNamespaceElement globalFuncDeclElem:
                        newSkeletonCollector.VisitGlobalFuncDecl(globalFuncDeclElem.FuncDecl);
                        break;

                    case S.TypeDeclNamespaceElement typeDeclElem:
                        newSkeletonCollector.VisitTypeDecl(typeDeclElem.TypeDecl);
                        break;

                    default:
                        throw new UnreachableCodeException();
                }
            }
        }

        void VisitScript(S.Script script)
        {
            foreach (var scriptElem in script.Elements)
            {
                switch(scriptElem)
                {
                    case S.TypeDeclScriptElement typeDeclElem:
                        VisitTypeDecl(typeDeclElem.TypeDecl);
                        break;

                    case S.GlobalFuncDeclScriptElement globalFuncDeclElem:
                        VisitGlobalFuncDecl(globalFuncDeclElem.FuncDecl);
                        break;

                    case S.NamespaceDeclScriptElement namespaceDeclElem:
                        VisitNamespaceDecl(namespaceDeclElem.NamespaceDecl);
                        break;

                    case S.StmtScriptElement:
                        // 그냥 통과
                        break;

                    default:
                        throw new UnreachableCodeException();
                }

            }
        }
    }
}
