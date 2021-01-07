using Gum.CompileTime;
using System;
using System.Collections.Generic;
using S = Gum.Syntax;

namespace Gum.IR0
{
    // ModuleInfoBuilder
    class Phase2
    {
        SkeletonRepository skelRepo;
        ModuleInfoBuilder moduleInfoBuilder;
        SyntaxItemInfoRepoBuilder syntaxItemInfoRepoBuilder;
        Phase3Factory phase3Factory;
        
        public Phase2(SkeletonRepository skelRepo, ModuleInfoBuilder moduleInfoBuilder, SyntaxItemInfoRepoBuilder syntaxItemInfoRepoBuilder, Phase3Factory phase3Factory)
        {
            this.skelRepo = skelRepo;
            this.moduleInfoBuilder = moduleInfoBuilder;
            this.syntaxItemInfoRepoBuilder = syntaxItemInfoRepoBuilder;
            this.phase3Factory = phase3Factory;
        }

        // enum E { First(int x, int y), Second } 에서 
        // First(int x, int y) 부분
        EnumElemInfo VisitEnumDeclElement(S.EnumDeclElement elem)
        {
            // 타입 Visitor계속
            return moduleInfoBuilder.BuildEnumElement(elem);
        }

        void VisitEnumSkel(Skeleton.Enum enumSkel)
        {
            var elemInfos = new List<EnumElemInfo>();
            foreach (var elem in enumSkel.EnumDecl.Elems)
            {
                var elemInfo = VisitEnumDeclElement(elem);
                elemInfos.Add(elemInfo);
            }

            var enumInfo = moduleInfoBuilder.BuildEnum(enumSkel, elemInfos);
            syntaxItemInfoRepoBuilder.AddTypeInfo(enumSkel.EnumDecl, enumInfo);
        }

        void VisitStructSkel(Skeleton.Struct structSkel)
        {
            var structInfo = moduleInfoBuilder.BuildStruct(structSkel);
            syntaxItemInfoRepoBuilder.AddTypeInfo(structSkel.StructDecl, structInfo);
        }

        void VisitFuncSkel(Skeleton.Func funcSkel)
        {
            var funcInfo = moduleInfoBuilder.BuildFunc(funcSkel);
            syntaxItemInfoRepoBuilder.AddFuncInfo(funcSkel.FuncDecl, funcInfo);
        }

        void VisitVarSkel(Skeleton.Var varSkel)
        {
            moduleInfoBuilder.BuildVar(varSkel);
        }

        void VisitSkel(Skeleton skel)
        {
            switch(skel)
            {
                case Skeleton.Enum enumSkel:
                    VisitEnumSkel(enumSkel);
                    break;

                case Skeleton.Struct structSkel:
                    VisitStructSkel(structSkel);
                    break;

                case Skeleton.Func funcSkel:
                    VisitFuncSkel(funcSkel);
                    break;

                case Skeleton.Var varSkel:
                    VisitVarSkel(varSkel);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        public Phase3 Run()
        {
            foreach (var globalSkeleton in skelRepo.GetGlobalSkeletons())
            {
                VisitSkel(globalSkeleton);
            }

            var scriptModuleInfo = moduleInfoBuilder.Build();
            var syntaxItemInfoRepo = syntaxItemInfoRepoBuilder.Build();

            return phase3Factory.Make(scriptModuleInfo, syntaxItemInfoRepo);
        }
    }
}