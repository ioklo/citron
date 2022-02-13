using Citron.IR0Translator;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using M = Citron.CompileTime;
using R = Citron.IR0;

using static Citron.Infra.Misc;

namespace Citron.IR0Translator.Test
{
    public class TypeValueTests
    {   
        M.Name moduleName;

        public TypeValueTests()
        {
            moduleName = new M.Name.Normal("TestModule");
        }

        // UnitOfWork_Scenario_ExpectedBehavior

        static IModuleTypeInfo[] emptyTypeInfo = Array.Empty<IModuleTypeInfo>();
        static IModuleFuncInfo[] emptyFuncInfo = Array.Empty<IModuleFuncInfo>();

        [Fact]
        public void ConstructTypeValue_GlobalStructType_ConstructsProperly()
        {   
            var xInfo = new M.StructDecl(new M.Name.Normal("X"), Arr("T"), null, default, default, default, default, default);
            var moduleInfo = new ExternalModuleDecl(new M.ModuleDecl(moduleName, default, Arr<M.TypeDecl>(xInfo), default));

            var typeInfoRepo = new TypeInfoRepository(moduleInfo, new ExternalModuleDeclRepository(Arr(RuntimeModuleDecl.Instance)));
            var ritemFactory = new RItemFactory();
            var factory = new ItemValueFactory(typeInfoRepo, ritemFactory);

            var xmType = new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("X"), Arr(MTypes.Int));
            var xType = factory.MakeTypeValueByMType(xmType);

            Assert.NotNull(xType);
        }

        [Fact]
        public void ConstructTypeValue_MemberStructType_ConstructsProperly()
        {
            // struct X<T> { struct Y<U> { } }            
            var yInfo = new M.StructDecl(new M.Name.Normal("Y"), Arr<string>("U"), null, default, default, default, default, default);
            var xInfo = new M.StructDecl(new M.Name.Normal("X"), Arr<string>("T"), null, default, Arr<M.TypeDecl>(yInfo), default, default, default);

            var internalModuleInfo = new ExternalModuleDecl(new M.ModuleDecl(moduleName, default, Arr<M.TypeDecl>(xInfo), default));
            var externalModuleRepo = new ReferenceModuleDeclRepository(Arr(RuntimeModuleDecl.Instance));

            var typeInfoRepo = new TypeInfoRepository(internalModuleInfo, externalModuleRepo);
            var ritemFactory = new RItemFactory();
            var typeValueFactory = new ItemValueFactory(typeInfoRepo, ritemFactory);
            
            
            // X<int>            
            var xmtype = new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("X"), Arr(MTypes.Int));

            // X<int>.Y<bool>
            var ymtype = new M.MemberType(xmtype, new M.Name.Normal("Y"), Arr(MTypes.Bool));

            var ytype = typeValueFactory.MakeTypeValueByMType(ymtype);

            Assert.NotNull(ytype);
        }

        // struct G<T> { }
        // struct X<T> { 
        //     struct Y<U> : G<T> { 
        //         X<U> v; 
        //         T F<V>(V);
        //     } 
        // }        
        // 
        ItemValueFactory MakeFactory()
        {   
            var gInfo = new M.StructDecl(new M.Name.Normal("G"), Arr("T"), null, default, default, default, default, default);

            var xInfo = new M.StructDecl(new M.Name.Normal("X"), Arr("T"), null, default, Arr<M.TypeDecl>(
                new M.StructDecl(
                    new M.Name.Normal("Y"),
                    Arr("U"),
                    new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("G"), Arr<M.TypeId>(new M.TypeVarType(0, "T"))),
                    interfaces: default,
                    memberTypes: default,
                    Arr(new M.FuncInfo(M.AccessModifier.Public, new M.Name.Normal("F"), false, false, true, Arr("V"), new M.TypeVarTypeId(0, "T"),                         
                        Arr(new M.Param(M.ParamKind.Default, new M.TypeVarTypeId(2, "V"), new M.Name.Normal("v")))
                    )),
                    Arr(new M.MemberVarInfo(M.AccessModifier.Public, false, new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("X"), Arr<M.TypeId>(new M.TypeVarTypeId(1, "U"))), new M.Name.Normal("v"))),
                    default
                )
            ), default, default, default);

            var testModule = new ExternalModuleDecl(new M.ModuleDecl(moduleName, default, Arr<M.TypeDecl>(xInfo, gInfo), default));
            var externalModuleRepo = new ReferenceModuleDeclRepository(Arr(RuntimeModuleDecl.Instance));
            var typeInfoRepo = new TypeInfoRepository(testModule, externalModuleRepo);
            var ritemFactory = new RItemFactory();
            return new ItemValueFactory(typeInfoRepo, ritemFactory);
        }

        // GetBaseTypeValue(X<int>.Y<string>) => G<int>
        [Fact]
        public void GettingBaseTypeValue_FromNestedStruct_ApplyingTypeVarCorrectly()
        {
            var factory = MakeFactory();

            var xmtype = new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("X"), Arr(MTypes.Int));
            var xymtype = new M.MemberType(xmtype, new M.Name.Normal("Y"), Arr(MTypes.String));

            var xyTypeValue = (StructSymbol)factory.MakeTypeValueByMType(xymtype);
            var xyBaseTypeValue = xyTypeValue.GetBaseType();

            var expected = factory.MakeTypeValueByMType(new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("G"), Arr(MTypes.Int)));

            Assert.Equal(expected, xyBaseTypeValue);
        }

        // [X<int>.Y<bool>].QueryMember("v").GetTypeValue() == X<bool>
        [Fact]
        public void GettingMemberVar_FromNestedStruct_ApplyingTypeVarCorrectly()
        {
            var factory = MakeFactory();

            var xyTypeValue = factory.MakeTypeValueByMType(
                new M.MemberType(
                    new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("X"), Arr(MTypes.Int)), new M.Name.Normal("Y"), Arr(MTypes.Bool)));

            var itemResult = xyTypeValue.QueryMember(new M.Name.Normal("v"), default);
            var expected = factory.MakeTypeValueByMType(new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("X"), Arr(MTypes.Bool)));

            var memberVarResult = (SymbolQueryResult.MemberVar)itemResult;
            var memberVarValue = factory.MakeMemberVarValue(memberVarResult.Outer, memberVarResult.MemberVarInfo);

            Assert.Equal(expected, memberVarValue.GetTypeValue());
        }

        // FuncValue를 얻어와서         
        // [X<int>.Y<string>].GetMemberFunc("F", <bool>);
        // T F<V>(V); => int F<bool>(bool);        
        [Fact]
        public void GettingMemberFunc_FromNestedStruct_ApplyTypeVarCorrectly()
        {
            var factory = MakeFactory();

            // X<int>.Y<string>
            var xmtype = new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("X"), Arr(MTypes.Int));
            var xymtype = new M.MemberType(xmtype, new M.Name.Normal("Y"), Arr(MTypes.String));
            var xytype = factory.MakeTypeValueByMType(xymtype);

            // 지금은 query밖에 없다, ID를 통한 직접 참조를 할 일 이 생기게 되면 변경한다
            var funcResult = (SymbolQueryResult.Funcs)xytype.QueryMember(new M.Name.Normal("F"), 1);
            var funcValue = factory.MakeFunc(funcResult.Outer, funcResult.FuncInfos[0], Arr(factory.Bool));

            Assert.False(funcValue.IsStatic);
            Assert.False(funcValue.IsSequence);            

            Assert.Equal(Arr(new ParamInfo(R.ParamKind.Default, factory.Bool)), funcValue.GetParamInfos());
            Assert.Equal(factory.Int, funcValue.GetRetType());
        }
    }
}
