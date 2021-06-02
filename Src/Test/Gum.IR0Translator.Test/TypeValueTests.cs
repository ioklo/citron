using Gum.IR0Translator;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using M = Gum.CompileTime;

using static Gum.Infra.Misc;

namespace Gum.IR0Translator.Test
{
    public class TypeValueTests
    {   
        M.ModuleName moduleName;

        public TypeValueTests()
        {
            moduleName = "TestModule";
        }

        // UnitOfWork_Scenario_ExpectedBehavior

        [Fact]
        public void ConstructTypeValue_GlobalStructType_ConstructsProperly()
        {
            var xInfo = new M.StructInfo("X", Arr("T"), null, default, default, default, default);
            var moduleInfo = new M.ModuleInfo(moduleName, default, Arr<M.TypeInfo>(xInfo), default);

            var typeInfoRepo = new TypeInfoRepository(moduleInfo, new ModuleInfoRepository(Arr(RuntimeModuleInfo.Instance)));
            var ritemFactory = new RItemFactory();
            var factory = new ItemValueFactory(typeInfoRepo, ritemFactory);

            var xmType = new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int));
            var xType = factory.MakeTypeValueByMType(xmType);

            Assert.NotNull(xType);
        }

        [Fact]
        public void ConstructTypeValue_MemberStructType_ConstructsProperly()
        {
            // struct X<T> { struct Y<U> { } }            
            var yInfo = new M.StructInfo("Y", Arr<string>("U"), null, default, default, default, default);
            var xInfo = new M.StructInfo("X", Arr<string>("T"), null, default, Arr<M.TypeInfo>(yInfo), default, default);            

            var internalModuleInfo = new M.ModuleInfo(moduleName, default, Arr<M.TypeInfo>(xInfo), default);
            var externalModuleRepo = new ModuleInfoRepository(Arr(RuntimeModuleInfo.Instance));

            var typeInfoRepo = new TypeInfoRepository(internalModuleInfo, externalModuleRepo);
            var ritemFactory = new RItemFactory();
            var typeValueFactory = new ItemValueFactory(typeInfoRepo, ritemFactory);
            
            
            // X<int>            
            var xmtype = new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int));

            // X<int>.Y<bool>
            var ymtype = new M.MemberType(xmtype, "Y", Arr(MTypes.Bool));

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
            var gInfo = new M.StructInfo("G", Arr("T"), null, default, default, default, default);

            var xInfo = new M.StructInfo("X", Arr("T"), null, default, Arr<M.TypeInfo>(
                new M.StructInfo(
                    "Y",
                    Arr("U"),
                    new M.GlobalType(moduleName, M.NamespacePath.Root, "G", Arr<M.Type>(new M.TypeVarType(0, "T"))),
                    interfaces: default,
                    memberTypes: default,
                    Arr(new M.FuncInfo("F", false, true, Arr("V"), new M.TypeVarType(0, "T"), new M.ParamInfo(
                        null,
                        Arr<(M.Type, M.Name)>((new M.TypeVarType(2, "V"), "v"))
                    ))),
                    Arr(new M.MemberVarInfo(false, new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr<M.Type>(new M.TypeVarType(1, "U"))), "v"))
                )
            ), default, default);

            var testModule = new M.ModuleInfo(moduleName, default, Arr<M.TypeInfo>(xInfo, gInfo), default);
            var externalModuleRepo = new ModuleInfoRepository(Arr(RuntimeModuleInfo.Instance));
            var typeInfoRepo = new TypeInfoRepository(testModule, externalModuleRepo);
            var ritemFactory = new RItemFactory();
            return new ItemValueFactory(typeInfoRepo, ritemFactory);
        }

        // GetBaseTypeValue(X<int>.Y<string>) => G<int>
        [Fact]
        public void GettingBaseTypeValue_FromNestedStruct_ApplyingTypeVarCorrectly()
        {
            var factory = MakeFactory();

            var xmtype = new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int));
            var xymtype = new M.MemberType(xmtype, "Y", Arr(MTypes.String));

            var xyTypeValue = (StructTypeValue)factory.MakeTypeValueByMType(xymtype);
            var xyBaseTypeValue = xyTypeValue.GetBaseType();

            var expected = factory.MakeTypeValueByMType(new M.GlobalType(moduleName, M.NamespacePath.Root, "G", Arr(MTypes.Int)));

            Assert.Equal(expected, xyBaseTypeValue);
        }

        // [X<int>.Y<bool>].GetMember("v").GetTypeValue() == X<bool>
        [Fact]
        public void GettingMemberVar_FromNestedStruct_ApplyingTypeVarCorrectly()
        {
            var factory = MakeFactory();

            var xyTypeValue = factory.MakeTypeValueByMType(
                new M.MemberType(
                    new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int)), "Y", Arr(MTypes.Bool)));

            var itemResult = xyTypeValue.GetMember("v", default);
            var expected = factory.MakeTypeValueByMType(new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Bool)));

            var memberVarResult = (ItemQueryResult.MemberVar)itemResult;
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
            var xmtype = new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int));
            var xymtype = new M.MemberType(xmtype, "Y", Arr(MTypes.String));
            var xytype = factory.MakeTypeValueByMType(xymtype);

            // 지금은 query밖에 없다, ID를 통한 직접 참조를 할 일 이 생기게 되면 변경한다
            var funcResult = (ItemQueryResult.Funcs)xytype.GetMember("F", 1);
            var funcValue = factory.MakeFunc(funcResult.Outer, funcResult.FuncInfos[0], Arr(factory.Bool));

            Assert.False(funcValue.IsStatic);
            Assert.False(funcValue.IsSequence);            

            Assert.Equal(Arr(factory.Bool), funcValue.GetParamTypes());
            Assert.Equal(factory.Int, funcValue.GetRetType());
        }
    }
}
