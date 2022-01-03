using Gum.Collections;
using Pretune;
using System;
using System.Diagnostics;
using M = Gum.CompileTime;
using static Gum.Analysis.Misc;

namespace Gum.Analysis
{
    public record ExternalModuleStructConstructorDecl : IModuleStructConstructorDecl
    {
        ExternalModuleStructDecl outer;
        M.StructConstructorDecl decl;

        public ExternalModuleStructConstructorDecl(ExternalModuleStructDecl outer, M.StructConstructorDecl decl)
        {
            this.outer = outer;
            this.decl = decl;
        }

        M.AccessModifier IModuleStructConstructorDecl.GetAccessModifier()
        {
            return decl.AccessModifier;
        }

        IModuleStructDecl IModuleStructConstructorDecl.GetOuter()
        {
            return outer;
        }

        ImmutableArray<M.Param> IModuleStructConstructorDecl.GetParameters()
        {
            return decl.Parameters;
        }
    }

    public record ExternalModuleStructMemberTypeDecl : IModuleStructMemberTypeDecl
    {
        ExternalModuleTypeDecl typeDecl;
        M.StructMemberTypeDecl decl;

        public ExternalModuleStructMemberTypeDecl(M.StructMemberTypeDecl memberTypeDecl)
        {
            this.typeDecl = ExternalModuleMisc.Make(memberTypeDecl.TypeDecl);
            this.decl = memberTypeDecl;
        }

        public M.AccessModifier GetAccessModifier()
        {
            return decl.AccessModifier;
        }

        public M.TypeName GetName()
        {
            return typeDecl.GetName();
        }
    }

    public record ExternalModuleStructMemberFuncDecl : IModuleStructMemberFuncDecl
    {
        M.StructMemberFuncDecl memberFuncDecl;        

        public ExternalModuleStructMemberFuncDecl(M.StructMemberFuncDecl memberFuncDecl)
        {
            this.memberFuncDecl = memberFuncDecl;
        }

        public M.FuncName GetName()
        {
            return new M.FuncName(memberFuncDecl.Name, memberFuncDecl.TypeParams.Length, MakeParamTypes(memberFuncDecl.Parameters));
        }

        M.AccessModifier IModuleStructMemberFuncDecl.GetAccessModifier()
        {
            return memberFuncDecl.AccessModifier;
        }

        ITypeValue IModuleStructMemberFuncDecl.GetReturnType()
        {
            throw new NotImplementedException();
        }

        bool IModuleStructMemberFuncDecl.IsInstanceFunc()
        {
            return memberFuncDecl.IsInstanceFunc;
        }

        bool IModuleStructMemberFuncDecl.IsInternal()
        {
            return false;
        }

        bool IModuleStructMemberFuncDecl.IsRefReturn()
        {
            return memberFuncDecl.IsRefReturn;
        }
    }

    public record ExternalModuleStructMemberVarDecl : IModuleStructMemberVarDecl
    {
        ExternalModuleStructDecl outer;
        M.StructMemberVarDecl varDecl;

        public ExternalModuleStructMemberVarDecl(ExternalModuleStructDecl outer, M.StructMemberVarDecl varDecl)
        {
            this.outer = outer;
            this.varDecl = varDecl;
        }

        public IModuleStructDecl GetOuter()
        {
            return outer;
        }

        public M.AccessModifier GetAccessModifier()
        {
            return varDecl.AccessModifier;
        }

        public ITypeValue GetDeclType()
        {
            throw new NotImplementedException();
        }

        public bool IsStatic()
        {
            return varDecl.IsStatic;
        }
    }

    [ImplementIEquatable]
    public partial class ExternalModuleStructDecl : ExternalModuleTypeDecl, IModuleStructDecl
    {
        M.StructDecl structDecl;
        IStructTypeValue? baseStruct;
        ImmutableDictionary<M.TypeName, ExternalModuleStructMemberTypeDecl> typesDict;
        ImmutableDictionary<M.FuncName, ExternalModuleStructMemberFuncDecl> funcsDict;
        ExternalModuleStructConstructorDecl? trivialConstructor;
        ImmutableArray<ExternalModuleStructConstructorDecl> constructors;
        ImmutableArray<ExternalModuleStructMemberVarDecl> memberVars;

        ModuleInfoBuildState state;

        void InitializeMemberTypes()
        {
            var builder = ImmutableDictionary.CreateBuilder<M.TypeName, ExternalModuleStructMemberTypeDecl>();

            foreach (var memberType in structDecl.MemberTypes)
            {
                var t = new ExternalModuleStructMemberTypeDecl(memberType);
                builder.Add(t.GetName(), t);
            }

            typesDict = builder.ToImmutable();
        }

        void InitializeMemberFuncs()
        {
            var builder = ImmutableDictionary.CreateBuilder<M.FuncName, ExternalModuleStructMemberFuncDecl>();

            foreach (var memberFunc in structDecl.MemberFuncs)
            {
                var f = new ExternalModuleStructMemberFuncDecl(memberFunc);
                builder.Add(f.GetName(), f);
            }

            funcsDict = builder.ToImmutable();
        }

        void InitializeConstructors()
        {
            var builder = ImmutableArray.CreateBuilder<ExternalModuleStructConstructorDecl>(structDecl.Constructors.Length);
            trivialConstructor = null;

            foreach (var constructor in structDecl.Constructors)
            {
                var c = new ExternalModuleStructConstructorDecl(this, constructor);

                if (constructor.IsTrivial)
                    trivialConstructor = c;

                builder.Add(c);
            }

            constructors = builder.MoveToImmutable();
        }

        void InitializeMemberVars()
        {
            var builder = ImmutableArray.CreateBuilder<ExternalModuleStructMemberVarDecl>(structDecl.MemberVars.Length);

            foreach (var memberVar in structDecl.MemberVars)
                builder.Add(new ExternalModuleStructMemberVarDecl(this, memberVar));

            memberVars = builder.MoveToImmutable();
        }

        public ExternalModuleStructDecl(M.StructDecl structDecl)
        {
            this.structDecl = structDecl;
            this.baseStruct = null;            

            InitializeMemberTypes();
            InitializeMemberFuncs();
            InitializeConstructors();
            InitializeMemberVars();

            this.state = ModuleInfoBuildState.BeforeSetBaseAndBuildTrivialConstructor;
        }

        public override M.TypeName GetName()
        {
            return new M.TypeName(structDecl.Name, structDecl.TypeParams.Length);
        }
        
        public IModuleStructMemberTypeDecl? GetMemberType(M.TypeName name)
        {
            return typesDict.GetValueOrDefault(name);
        }

        public IModuleStructMemberFuncDecl? GetMemberFunc(M.FuncName name)
        {
            return funcsDict.GetValueOrDefault(name);
        }

        public ImmutableArray<string> GetTypeParams()
        {
            return structDecl.TypeParams;
        }

        //ImmutableArray<IModuleStructMemberTypeDecl> GetMemberTypes()
        //{
        //    return ImmutableArray<IModuleStructMemberTypeDecl>.CastUp(memberTypes);
        //}

        //ImmutableArray<IModuleStructMemberFuncDecl> GetMemberFuncs()
        //{
        //    return ImmutableArray<IModuleStructMemberFuncDecl>.CastUp(memberFuncs);
        //}

        //public ImmutableArray<IModuleStructConstructorDecl> GetConstructors()
        //{
        //    return ImmutableArray<IModuleStructConstructorDecl>.CastUp(constructors);
        //}

        //public ImmutableArray<IModuleStructMemberVarDecl> GetMemberVars()
        //{
        //    return ImmutableArray<IModuleStructMemberVarDecl>.CastUp(memberVars);
        //}

        public IStructTypeValue? GetBaseStruct()
        {
            Debug.Assert(state == ModuleInfoBuildState.Completed);
            return baseStruct;
        }

        public IModuleStructConstructorDecl? GetTrivialConstructor()
        {
            return trivialConstructor;
        }

        //public void SetBaseAndBuildTrivialConstructor(IQueryModuleTypeInfo query, IItemValueFactory itemValueFactory)
        //{
        //    if (state == ModuleInfoBuildState.Completed) return;

        //    if (structInfo.BaseType == null)
        //    {
        //        baseStruct = null;
        //    }
        //    else
        //    {
        //        baseStruct = (StructTypeValue)itemValueFactory.MakeTypeValueByMType(structInfo.BaseType);
        //    }

        //    state = ModuleInfoBuildState.Completed;
        //    return;
        //}
    }
}
