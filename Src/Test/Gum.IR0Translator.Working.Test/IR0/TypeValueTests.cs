using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using M = Gum.CompileTime;

namespace Gum.IR0
{
    public class TypeValueTests
    {
        static ImmutableArray<T> Arr<T>(params T[] items) => ImmutableArray.Create(items);

        // UnitOfWork_Scenario_ExpectedBehavior

        [Fact]
        void ConstructTypeValue_GlobalStructType_ConstructsProperly()
        {
            var factory = new TypeValueFactory();

            var intType = TypeValues.Int;
            var xInfo = new M.StructInfo("X", Arr<string>("T"), Arr<M.Type>(), Arr<M.TypeInfo>(), Arr<M.FuncInfo>(), Arr<M.MemberVarInfo>());

            var xType = factory.MakeGlobalType("TestModule", M.NamespacePath.Root, xInfo, Arr(intType));

            Assert.NotNull(xType);
        }

        [Fact]
        void ConstructTypeValue_MemberStructType_ConstructsProperly()
        {
            var internalModuleInfo = new M.ModuleInfo("TestModule", Arr<M.NamespaceInfo>(), Arr<M.TypeInfo>(), Arr<M.FuncInfo>());
            var externalModuleRepo = new ModuleInfoRepository(Arr<M.ModuleInfo>());
            var factory = new GlobalItemValueFactory(internalModuleInfo, externalModuleRepo);
            
            // struct X<T> { struct Y<U> { } }
            var xInfo = new M.StructInfo("X", Arr<string>("T"), Arr<M.Type>(), Arr<M.TypeInfo>(), Arr<M.FuncInfo>(), Arr<M.MemberVarInfo>());
            var yInfo = new M.StructInfo("Y", Arr<string>("U"), Arr<M.Type>(), Arr<M.TypeInfo>(), Arr<M.FuncInfo>(), Arr<M.MemberVarInfo>());

            // X<int>
            var xType = factory.MakeStructTypeValue("TestModule", M.NamespacePath.Root, xInfo, Arr(TypeValues.Int));

            xType.GetMemberType()

            // X<int>.Y<bool>
            var yType = factory.MakeStructTypeValue(xType, yInfo, Arr(TypeValues.Bool));

            Assert.NotNull(yType);
        }
    }
}
