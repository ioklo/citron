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

using S = Gum.Syntax;

namespace Gum.Analysis
{
    struct TypeSkeletonCollector
    {
        DeclSymbolPath? basePath; // 현재 위치

        // runtime
        ImmutableArray<TypeSkeleton>.Builder skeletonsBuilder;

        public static TypeSkeletonRepository Collect(S.Script script)
        {
            var typeSkeletonCollector = NewTypeSkeletonCollector();
            typeSkeletonCollector.VisitScript(script);

            return TypeSkeletonRepository.Build(typeSkeletonCollector.skeletonsBuilder.ToImmutable());
        }

        TypeSkeletonCollector(DeclSymbolPath? basePath, ImmutableArray<TypeSkeleton>.Builder builder)
        {
            this.basePath = basePath;
            this.skeletonsBuilder = builder;
        }
        
        static TypeSkeletonCollector NewTypeSkeletonCollector()
        {
            return new TypeSkeletonCollector(null, ImmutableArray.CreateBuilder<TypeSkeleton>());
        }

        static TypeSkeletonCollector NewTypeSkeletonCollector(DeclSymbolPath path)
        {
            return new TypeSkeletonCollector(path, ImmutableArray.CreateBuilder<TypeSkeleton>());
        }

        void AddSkeleton(TypeSkeleton skeleton)
        {   
            skeletonsBuilder.Add(skeleton);
        }        

        void VisitEnumElem(S.EnumElemDecl enumElem)
        {
            var elemPath = basePath.Child(new Name.Normal(enumElem.Name), 0);

            var enumElemSkel = new TypeSkeleton(elemPath, default, TypeSkeletonKind.EnumElem); // 
            AddSkeleton(enumElemSkel);
        }

        // namespace일 경우 어떻게 할거냐
        void VisitEnumDecl(S.EnumDecl enumDecl)
        {
            var enumPath = basePath.Child(new Name.Normal(enumDecl.Name), enumDecl.TypeParams.Length);
            var newCollector = NewTypeSkeletonCollector(enumPath);

            foreach (var enumElem in enumDecl.Elems)
            {
                newCollector.VisitEnumElem(enumElem);
            }
            
            var enumSkeleton = new TypeSkeleton(enumPath, newCollector.skeletonsBuilder.ToImmutable(), TypeSkeletonKind.Enum);
            AddSkeleton(enumSkeleton);
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            var structPath = basePath.Child(new Name.Normal(structDecl.Name), structDecl.TypeParams.Length);

            var newCollector = NewTypeSkeletonCollector(structPath);
            foreach(var elem in structDecl.MemberDecls)
            {
                switch(elem)
                {
                    case S.StructMemberTypeDecl typeDecl:
                        newCollector.VisitTypeDecl(typeDecl.TypeDecl);
                        break;
                }
            }
            
            
            var skeleton = new TypeSkeleton(structPath, newCollector.skeletonsBuilder.ToImmutable(), TypeSkeletonKind.Struct);
            AddSkeleton(skeleton);
        }

        void VisitClassDecl(S.ClassDecl classDecl)
        {
            var classPath = basePath.Child(new Name.Normal(classDecl.Name), classDecl.TypeParams.Length);

            var newCollector = NewTypeSkeletonCollector();
            foreach (var memberDecl in classDecl.MemberDecls)
            {
                switch (memberDecl)
                {
                    case S.ClassMemberTypeDecl typeDecl:
                        newCollector.VisitTypeDecl(typeDecl.TypeDecl);
                        break;
                }
            }
            
            var skeleton = new TypeSkeleton(classPath, newCollector.skeletonsBuilder.ToImmutable(), TypeSkeletonKind.Class);
            AddSkeleton(skeleton);
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
