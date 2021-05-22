using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

using static Gum.IR0Translator.AnalyzeErrorCode;
using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    struct TypeSkeletonCollector
    {
        // runtime
        ImmutableDictionary<ItemPath, TypeSkeleton>.Builder typeSkeletonsByItemPathBuilder;
        ImmutableArray<TypeSkeleton>.Builder skeletonsBuilder;

        public static TypeSkeletonRepository Collect(S.Script script)
        {
            var typeSkeletonCollector = new TypeSkeletonCollector(ImmutableDictionary.CreateBuilder<ItemPath, TypeSkeleton>());
            typeSkeletonCollector.VisitScript(script);

            return new TypeSkeletonRepository(typeSkeletonCollector.typeSkeletonsByItemPathBuilder.ToImmutable());
        }

        TypeSkeletonCollector(ImmutableDictionary<ItemPath, TypeSkeleton>.Builder builder)
        {
            this.typeSkeletonsByItemPathBuilder = builder;
            this.skeletonsBuilder = ImmutableArray.CreateBuilder<TypeSkeleton>();
        }

        void ExecInNewEnumScope(S.EnumDecl enumDecl, Action action)
            => ExecInNewTypeScope(enumDecl, path => new EnumSkeleton(path, enumDecl), action);

        void ExecInNewStructScope(S.StructDecl structDecl, Action action)
            => ExecInNewTypeScope(structDecl, path => new StructSkeleton(path, structDecl), action);

        void ExecInNewTypeScope<TSkeleton>(S.TypeDecl typeDecl, Func<ItemPath, TSkeleton> constructor, Action action)
            where TSkeleton : TypeSkeleton
        {
            var prevSkeleton = curSkeleton;
            curSkeleton = AddType(typeDecl, constructor);

            try
            {
                action.Invoke();
            }
            finally
            {
                curSkeleton = prevSkeleton;
            }
        }
        
        TSkeleton AddType<TSkeleton>(S.TypeDecl decl, Func<ItemPath, TSkeleton> constructor)
            where TSkeleton : TypeSkeleton
        {
            ItemPath typePath = (curSkeleton != null)
                ? curSkeleton.Path.Append(decl.Name, decl.TypeParamCount)
                : new ItemPath(NamespacePath.Root, new ItemPathEntry(decl.Name, decl.TypeParamCount)); // TODO: NamespaceRoot가 아니라 namespace 선언 상황에 따라 달라진다

            var typeSkeleton = constructor.Invoke(typePath);
            typeSkeletonsByItemPathBuilder.Add(typePath, typeSkeleton);

            if (curSkeleton != null)
                curSkeleton.AddMember(typeSkeleton);

            return typeSkeleton;
        }

        ItemPath GetTypePath(string name, int typeParamCount)
        {
            if (curSkeleton != null)
                return curSkeleton.Path.Append(name, typeParamCount);
            else
                return new ItemPath(NamespacePath.Root, new ItemPathEntry(name, typeParamCount)); // TODO: NamespaceRoot가 아니라 namespace 선언 상황에 따라 달라진다
        }

        void AddSkeleton(TypeSkeleton skeleton)
        {
            typeSkeletonsByItemPathBuilder.Add(skeleton.Path, skeleton);
            skeletonsBuilder.Add(skeleton);
        }

        void VisitEnumDecl(S.EnumDecl enumDecl)
        {
            var builder = ImmutableArray.CreateBuilder<TypeSkeleton>();

            var newCollector = new TypeSkeletonCollector(typeSkeletonsByItemPathBuilder);
            foreach (var enumElem in enumDecl.Elems)
            {
                var elemPath = typePath.Append(enumElem.Name);

                var enumElemSkel = new EnumElemSkeleton(elemPath);
                newCollector.AddSkeleton(enumElemSkel);
            }

            var typePath = GetTypePath(enumDecl.Name, enumDecl.TypeParamCount);
            var enumSkeleton = new EnumSkeleton(typePath, enumDecl);
            AddSkeleton(enumSkeleton);

            // 
            // 



            
            new TypeSkeletonCollector(builder, AddType(enumDecl, )

            ExecInNewEnumScope(enumDecl, () =>
            {
                curSkeleton.AddMember()
                // var enumElemNames = enumDecl.Elems.Select(elem => elem.Name);
            });
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            ExecInNewStructScope(structDecl, () =>
            {
                foreach (var elem in structDecl.Elems)
                {
                    switch (elem)
                    {
                        case S.TypeStructDeclElement typeElem:
                            VisitTypeDecl(typeElem.TypeDecl);
                            break;
                    }
                }
            });
        }

        void VisitTypeDecl(S.TypeDecl typeDecl)
        {
            switch (typeDecl)
            {
                case S.EnumDecl enumDecl: VisitEnumDecl(enumDecl); break;
                case S.StructDecl structDecl: VisitStructDecl(structDecl); break;
                default: throw new UnreachableCodeException();
            }
        }

        void VisitScript(S.Script script)
        {
            foreach (var scriptElem in script.Elements)
            {
                if (scriptElem is S.TypeDeclScriptElement typeDeclElem)
                    VisitTypeDecl(typeDeclElem.TypeDecl);
            }
        }
    }
}
