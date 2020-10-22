using Xunit;
using QuickSC;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using System.Linq;
using Gum.CompileTime;
using Gum;

namespace QuickSC
{
    public class TypeValueApplierTests
    {
        [Fact()]
        public void Apply_FuncTest()
        {
            TypeValue MakeTypeValue(string name)
                => TypeValue.MakeNormal(ModuleItemId.Make(name));

            // class X<T, U, V> { class Y<T, U> { V Func<T>(T t, U u); } }
            var typeInfos = new List<ITypeInfo>();
            var funcInfos = new List<FuncInfo>();

            var xId = ModuleItemId.Make("X", 3);
            var yId = xId.Append("Y", 2);
            var funcId = yId.Append("Func", 1);

            var xVTypeVar = TypeValue.MakeTypeVar(xId, "V");
            var yUTypeVar = TypeValue.MakeTypeVar(yId, "U");
            var funcTTypeVar = TypeValue.MakeTypeVar(funcId, "T");

            var xInfo = new ClassInfo(null, xId, new string[] { "T", "U", "V" }, null, new[] { yId }, Enumerable.Empty<ModuleItemId>(), Enumerable.Empty<ModuleItemId>());
            var yInfo = new ClassInfo(xId, yId, new string[] { "T", "U" }, null, Enumerable.Empty<ModuleItemId>(), new[] { funcId }, Enumerable.Empty<ModuleItemId>());
            var funcInfo = new FuncInfo(yId, funcId, false, true, new[] { "T" }, xVTypeVar, funcTTypeVar, yUTypeVar);

            typeInfos.Add(xInfo);
            typeInfos.Add(yInfo);

            funcInfos.Add(funcInfo);

            IModuleInfo moduleInfo = new ScriptModuleInfo("Script", typeInfos, funcInfos, Enumerable.Empty<VarInfo>());

            var moduleInfoService = new ModuleInfoService(new[] { moduleInfo });
            var applier = new TypeValueApplier(moduleInfoService);

            // X<A, B, C>.Y<D, E>.Func<F>
            var funcTypeArgList = TypeArgumentList.Make(
                new[] { MakeTypeValue("A"), MakeTypeValue("B"), MakeTypeValue("C") },
                new[] { MakeTypeValue("D"), MakeTypeValue("E") },
                new[] { MakeTypeValue("F") });

            var funcValue = new FuncValue(funcId, funcTypeArgList);
            var funcTypeValue = TypeValue.MakeFunc(xVTypeVar, new[] { funcTTypeVar, yUTypeVar });

            var appliedTypeValue = applier.Apply_Func(funcValue, funcTypeValue);

            var expectedTypeValue = TypeValue.MakeFunc(MakeTypeValue("C"), new[] { MakeTypeValue("F"), MakeTypeValue("E") });
            Assert.Equal(expectedTypeValue, appliedTypeValue);
        }

        [Fact()]
        public void ApplyTest()
        {
            // class X<T> { class Y<T> { T x; } }
            List<ITypeInfo> typeInfos = new List<ITypeInfo>();

            var xId = ModuleItemId.Make("X", 1);
            var yId = xId.Append("Y", 1);

            var xInfo = new ClassInfo(null, xId, new string[] { "T" }, null, new[] { yId }, Enumerable.Empty<ModuleItemId>(), Enumerable.Empty<ModuleItemId>());
            var yInfo = new ClassInfo(xId, yId, new string[] { "T" }, null, Enumerable.Empty<ModuleItemId>(), Enumerable.Empty<ModuleItemId>(), Enumerable.Empty<ModuleItemId>());

            typeInfos.Add(xInfo);
            typeInfos.Add(yInfo);

            IModuleInfo moduleInfo = new ScriptModuleInfo("Script", typeInfos, Enumerable.Empty<FuncInfo>(), Enumerable.Empty<VarInfo>());
                
            var moduleInfoService = new ModuleInfoService(new[] { moduleInfo });
            var applier = new TypeValueApplier(moduleInfoService);


            // Apply(X<int>.Y<short>, TofX) == int
            var intId = ModuleItemId.Make("int");
            var shortId = ModuleItemId.Make("short");
            var intValue = TypeValue.MakeNormal(intId, TypeArgumentList.Empty);
            var shortValue = TypeValue.MakeNormal(shortId, TypeArgumentList.Empty);

            var yTypeArgs = TypeArgumentList.Make(
                new[] { intValue },
                new[] { shortValue });

            var appliedValue = applier.Apply(TypeValue.MakeNormal(yId, yTypeArgs), TypeValue.MakeTypeVar(xId, "T"));

            Assert.Equal(intValue, appliedValue);
        }
    }
}