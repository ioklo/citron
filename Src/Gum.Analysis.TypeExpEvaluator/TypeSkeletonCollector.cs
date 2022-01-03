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
        abstract record BasePath
        {
            public abstract TypeDeclPath MakeChildPath(TypeName name);

            public record Root : BasePath
            {
                public override TypeDeclPath MakeChildPath(TypeName name)
                {
                    return new RootTypeDeclPath(null, name);
                }
            }

            public record Namespace(NamespacePath Path) : BasePath
            {
                public override TypeDeclPath MakeChildPath(TypeName name)
                {
                    return new RootTypeDeclPath(Path, name);
                }
            }

            public record TypeDecl(TypeDeclPath Path) : BasePath
            {
                public override TypeDeclPath MakeChildPath(TypeName name)
                {
                    return new MemberTypeDeclPath(Path, name);
                }
            }
        }

        BasePath basePath;

        // runtime
        ImmutableArray<TypeSkeleton>.Builder skeletonsBuilder;

        public static TypeSkeletonRepository Collect(S.Script script)
        {
            var typeSkeletonCollector = NewTypeSkeletonCollector();
            typeSkeletonCollector.VisitScript(script);

            return new TypeSkeletonRepository(typeSkeletonCollector.skeletonsBuilder.ToImmutable());
        }

        TypeSkeletonCollector(BasePath basePath, ImmutableArray<TypeSkeleton>.Builder builder)
        {
            this.basePath = basePath;
            this.skeletonsBuilder = builder;
        }
        
        static TypeSkeletonCollector NewTypeSkeletonCollector()
        {
            return new TypeSkeletonCollector(new BasePath.Root(), ImmutableArray.CreateBuilder<TypeSkeleton>());
        }

        static TypeSkeletonCollector NewTypeSkeletonCollector(TypeDeclPath path)
        {
            return new TypeSkeletonCollector(new BasePath.TypeDecl(path), ImmutableArray.CreateBuilder<TypeSkeleton>());
        }

        void AddSkeleton(TypeSkeleton skeleton)
        {   
            skeletonsBuilder.Add(skeleton);
        }        

        void VisitEnumElem(S.EnumDeclElement enumElem)
        {
            var elemName = new TypeName(new Name.Normal(enumElem.Name), 0);
            var elemPath = basePath.MakeChildPath(elemName);

            var enumElemSkel = new TypeSkeleton(elemPath, default, TypeSkeletonKind.EnumElem); // 
            AddSkeleton(enumElemSkel);
        }

        // namespace일 경우 어떻게 할거냐
        void VisitEnumDecl(S.EnumDecl enumDecl)
        {
            var enumName = new TypeName(new Name.Normal(enumDecl.Name), enumDecl.TypeParams.Length);

            var enumPath = basePath.MakeChildPath(enumName);
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
            var structName = new TypeName(new Name.Normal(structDecl.Name), structDecl.TypeParams.Length);
            var structPath = basePath.MakeChildPath(structName);

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
            var className = new TypeName(new Name.Normal(classDecl.Name), classDecl.TypeParams.Length);
            var classPath = basePath.MakeChildPath(className);

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
