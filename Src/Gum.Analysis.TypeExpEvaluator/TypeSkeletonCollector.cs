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
using Gum.Analysis;

namespace Gum.IR0Translator
{
    struct TypeSkeletonCollector
    {
        // runtime
        ImmutableArray<TypeSkeleton>.Builder skeletonsBuilder;

        public static TypeSkeletonRepository Collect(S.Script script)
        {
            var typeSkeletonCollector = NewTypeSkeletonCollector();
            typeSkeletonCollector.VisitScript(script);

            return new TypeSkeletonRepository(typeSkeletonCollector.skeletonsBuilder.ToImmutable());
        }

        TypeSkeletonCollector(ImmutableArray<TypeSkeleton>.Builder builder)
        {
            this.skeletonsBuilder = builder;
        }
        
        static TypeSkeletonCollector NewTypeSkeletonCollector()
        {
            return new TypeSkeletonCollector(ImmutableArray.CreateBuilder<TypeSkeleton>());
        }

        void AddSkeleton(TypeSkeleton skeleton)
        {   
            skeletonsBuilder.Add(skeleton);
        }        

        void VisitEnumDecl(S.EnumDecl enumDecl)
        {
            var newCollector = NewTypeSkeletonCollector();
            foreach (var enumElem in enumDecl.Elems)
            {
                var pathEntry = new ItemPathEntry(new Name.Normal(enumElem.Name));
                var enumElemSkel = new TypeSkeleton(pathEntry, default, TypeSkeletonKind.EnumElem); // 
                newCollector.AddSkeleton(enumElemSkel);
            }

            var enumPathEntry = new ItemPathEntry(new Name.Normal(enumDecl.Name), enumDecl.TypeParams.Length);
            var enumSkeleton = new TypeSkeleton(enumPathEntry, newCollector.skeletonsBuilder.ToImmutable(), TypeSkeletonKind.Enum);
            AddSkeleton(enumSkeleton);
        }

        void VisitStructDecl(S.StructDecl structDecl)
        {
            var newCollector = NewTypeSkeletonCollector();
            foreach(var elem in structDecl.MemberDecls)
            {
                switch(elem)
                {
                    case S.StructMemberTypeDecl typeDecl:
                        newCollector.VisitTypeDecl(typeDecl.TypeDecl);
                        break;
                }
            }

            var pathEntry = new ItemPathEntry(new Name.Normal(structDecl.Name), structDecl.TypeParams.Length);
            var skeleton = new TypeSkeleton(pathEntry, newCollector.skeletonsBuilder.ToImmutable(), TypeSkeletonKind.Struct);
            AddSkeleton(skeleton);
        }

        void VisitClassDecl(S.ClassDecl classDecl)
        {
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

            var pathEntry = new ItemPathEntry(new Name.Normal(classDecl.Name), classDecl.TypeParams.Length);
            var skeleton = new TypeSkeleton(pathEntry, newCollector.skeletonsBuilder.ToImmutable(), TypeSkeletonKind.Class);
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
