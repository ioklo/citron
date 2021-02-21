using Gum.IR0Translator;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using M = Gum.CompileTime;

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
            var intType = TypeValues.Int;
            var xInfo = new M.StructInfo("X", Arr("T"), null, default, default, default, default);            
            var moduleInfo = new M.ModuleInfo(moduleName, default, Arr<M.TypeInfo>(xInfo), default);

            var typeInfoRepo = new TypeInfoRepository(moduleInfo, new ModuleInfoRepository(default));
            var ritemFactory = new IR0ItemFactory();
            var factory = new ItemValueFactory(typeInfoRepo, ritemFactory);

            var xmType = new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int));
            var xType = factory.MakeTypeValue(xmType);

            Assert.NotNull(xType);
        }

        [Fact]
        public void ConstructTypeValue_MemberStructType_ConstructsProperly()
        {
            var internalModuleInfo = new M.ModuleInfo(moduleName, default, default, default);
            var externalModuleRepo = new ModuleInfoRepository(default);

            var typeInfoRepo = new TypeInfoRepository(internalModuleInfo, externalModuleRepo);
            var ritemFactory = new IR0ItemFactory();
            var typeValueFactory = new ItemValueFactory(typeInfoRepo, ritemFactory);
            
            // struct X<T> { struct Y<U> { } }
            var xInfo = new M.StructInfo("X", Arr<string>("T"), null, default, default, default, default);
            var yInfo = new M.StructInfo("Y", Arr<string>("U"), null, default, default, default, default);

            // X<int>            
            var xmtype = new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int));

            // X<int>.Y<bool>
            var ymtype = new M.MemberType(xmtype, "Y", Arr(MTypes.Bool));

            var ytype = typeValueFactory.MakeTypeValue(ymtype);

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
                    new M.GlobalType(moduleName, M.NamespacePath.Root, "G", Arr<M.Type>(new M.TypeVarType(0, 0, "T"))),
                    interfaces: default,
                    memberTypes: default,
                    Arr(new M.FuncInfo("F", false, true, Arr("V"), new M.TypeVarType(0, 0, "T"), Arr<M.Type>(new M.TypeVarType(2, 0, "V")))),
                    Arr(new M.MemberVarInfo(false, new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr<M.Type>(new M.TypeVarType(1, 0, "U"))), "v"))
                )
            ), default, default);

            var testModule = new M.ModuleInfo(moduleName, default, Arr<M.TypeInfo>(xInfo, gInfo), default);
            var externalModuleRepo = new ModuleInfoRepository(default);
            var typeInfoRepo = new TypeInfoRepository(testModule, externalModuleRepo);
            var ritemFactory = new IR0ItemFactory();
            return new ItemValueFactory(typeInfoRepo, ritemFactory);
        }

        // GetBaseTypeValue(X<int, bool>.Y<string>) => G<int>
        [Fact]
        public void GettingBaseTypeValue_FromNestedStruct_ApplyingTypeVarCorrectly()
        {
            var factory = MakeFactory();

            var xmtype = new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int, MTypes.Bool));
            var xymtype = new M.MemberType(xmtype, "Y", Arr(MTypes.String));

            var xyTypeValue = factory.MakeTypeValue(xymtype);
            var xyBaseTypeValue = xyTypeValue.GetBaseType();

            var expected = factory.MakeTypeValue(new M.GlobalType(moduleName, M.NamespacePath.Root, "G", Arr(MTypes.Int)));

            Assert.Equal(expected, xyBaseTypeValue);
        }

        // [X<int>.Y<bool>].GetMember("v").GetTypeValue() == X<bool>
        [Fact]
        public void GettingMemberVar_FromNestedStruct_ApplyingTypeVarCorrectly()
        {
            var factory = MakeFactory();

            var xyTypeValue = factory.MakeTypeValue(
                new M.MemberType(
                    new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Int)), "Y", Arr(MTypes.Bool)));

            var itemResult = xyTypeValue.GetMember("v", default, null);
            var expected = factory.MakeTypeValue(new M.GlobalType(moduleName, M.NamespacePath.Root, "X", Arr(MTypes.Bool)));
            Assert.Equal(expected, ((ValueItemResult)itemResult).ItemValue);
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
            var xytype = factory.MakeTypeValue(xymtype);

            // 지금은 query밖에 없다, ID를 통한 직접 참조를 할 일 이 생기게 되면 변경한다
            var itemResult = (ValueItemResult)xytype.GetMember("F", Arr(TypeValues.Bool), null);
            var funcValue = (FuncValue)itemResult.ItemValue;            

            Assert.False(funcValue.IsStatic);
            Assert.False(funcValue.IsSequence);            

            Assert.Equal(Arr(TypeValues.Bool), funcValue.GetParamTypes());
            Assert.Equal(TypeValues.Int, funcValue.GetRetType());
        }
    }
}
