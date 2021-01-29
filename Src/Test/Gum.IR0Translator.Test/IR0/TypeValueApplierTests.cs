using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using System.Linq;
using Gum.CompileTime;

namespace Gum.IR0
{
    public class TypeValueApplierTests
    {
        [Fact]
        public void Apply_FuncTest()
        {
            TypeValue MakeTypeValue(string name)
                => new NormalTypeValue(new ItemId(ModuleName.Internal, NamespacePath.Root, new ItemPathEntry(name)));

            // class X<T, U, V> { class Y<T, U> { V Func<T>(T t, U u); } }
            var xId = new ItemId(ModuleName.Internal, NamespacePath.Root, new ItemPathEntry("X", 3));
            var yId = xId.Append("Y", 2);
            var funcId = yId.Append("Func", 1);

            var xVTypeVar = new TypeVarTypeValue(0, 2, "V");
            var yUTypeVar = new TypeVarTypeValue(1, 1, "U");
            var funcTTypeVar = new TypeVarTypeValue(2, 0, "T");

            var funcInfo = new FuncInfo(funcId, false, true, new[] { "T" }, xVTypeVar, funcTTypeVar, yUTypeVar);
            var yInfo = new ClassInfo(yId, new string[] { "T", "U" }, null, new[] { funcInfo });
            var xInfo = new ClassInfo(xId, new string[] { "T", "U", "V" }, null, new[] { yInfo });

            IModuleInfo moduleInfo = new InternalModuleInfo(Array.Empty<NamespaceInfo>(), new ItemInfo[] { xInfo });

            var moduleInfoRepo = new ModuleInfoRepository(new[] { moduleInfo });
            var applier = new TypeValueApplier(moduleInfoRepo);

            
            var funcTypeArgList = new[] {
                new[] { MakeTypeValue("A"), MakeTypeValue("B"), MakeTypeValue("C") },
                new[] { MakeTypeValue("D"), MakeTypeValue("E") },
                new[] { MakeTypeValue("F") } 
            };
            // X<A, B, C>.Y<D, E>.Func<F>
            var funcValue = new FuncValue(funcId, funcTypeArgList);

            // (Tf, Uy) => Vx 
            var funcTypeValue = new FuncTypeValue(xVTypeVar, new[] { funcTTypeVar, yUTypeVar });

            var appliedTypeValue = applier.Apply_Func(funcValue, funcTypeValue);

            var expectedTypeValue = new FuncTypeValue(MakeTypeValue("C"), new[] { MakeTypeValue("F"), MakeTypeValue("E") });
            Assert.Equal(expectedTypeValue, appliedTypeValue, ModuleInfoEqualityComparer.Instance);
        }

        [Fact]
        public void ApplyTest()
        {
            // class X<T> { class Y<T> { T x; } }
            var xId = new ItemId(ModuleName.Internal, NamespacePath.Root, new ItemPathEntry("X", 1));
            var yId = xId.Append("Y", 1);

            var yInfo = new ClassInfo(yId, new string[] { "T" }, null, Array.Empty<ItemInfo>());
            var xInfo = new ClassInfo(xId, new string[] { "T" }, null, new[] { yInfo });

            IModuleInfo moduleInfo = new InternalModuleInfo(Array.Empty<NamespaceInfo>(), new[] { xInfo });
                
            var moduleInfoRepo = new ModuleInfoRepository(new[] { moduleInfo });
            var applier = new TypeValueApplier(moduleInfoRepo);

            var intValue = new NormalTypeValue("System.Runtime", new NamespacePath("System"), new AppliedItemPathEntry("Int32"));
            var shortValue = new NormalTypeValue("System.Runtime", new NamespacePath("System"), new AppliedItemPathEntry("Int16"));

            var yTypeArgs = new[] {
                new[] { intValue },
                new[] { shortValue } };

            // Apply(X<int>.Y<short>, TofX) == int
            var appliedValue = applier.Apply(new NormalTypeValue(yId, yTypeArgs), new TypeVarTypeValue(0, 0, "T"));

            Assert.Equal(intValue, appliedValue, ModuleInfoEqualityComparer.Instance);
        }
    }
}