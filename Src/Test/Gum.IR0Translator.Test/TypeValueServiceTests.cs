using Gum;
using Gum.CompileTime;
using System.Linq;
using Xunit;

namespace QuickSC
{
    public class TypeValueServiceTests
    {
        TypeValue MakeTypeValue(string name)
                => TypeValue.MakeNormal(ModuleItemId.Make(name), TypeArgumentList.Empty);

        // class X<T, U> { class Y<U> : G<T> { 
        //     Dict<T, U> v; 
        //     T F<V>(V v, List<U> u); 
        // } }
        TypeValueService MakeTypeValueService()
        {
            var xId = ModuleItemId.Make("X", 2);
            var yId = xId.Append("Y", 1);
            var vId = yId.Append("v");
            var fId = yId.Append("F", 1);            
            var dictId = ModuleItemId.Make("Dict", 2);
            var listId = ModuleItemId.Make("List", 1);

            var xtTypeVar = TypeValue.MakeTypeVar(xId, "T");
            var yuTypeVar = TypeValue.MakeTypeVar(yId, "U");
            var fvTypeVar = TypeValue.MakeTypeVar(fId, "V");

            var gId = ModuleItemId.Make("G", 1);
            var gtTypeValue = TypeValue.MakeNormal(gId, TypeArgumentList.Make(xtTypeVar));

            var xInfo = new ClassInfo(null, xId, new[] { "T", "U" }, null, new[] { yId }, Enumerable.Empty<ModuleItemId>(), Enumerable.Empty<ModuleItemId>());
            var yInfo = new ClassInfo(xId, yId, new[] { "U" }, gtTypeValue, Enumerable.Empty<ModuleItemId>(), new[] { fId }, new[] { vId });
            var vInfo = new VarInfo(yId, vId, false, TypeValue.MakeNormal(dictId, TypeArgumentList.Make(xtTypeVar, yuTypeVar)));
            var fInfo = new FuncInfo(yId, fId, false, true, new[] { "V" }, xtTypeVar, fvTypeVar, TypeValue.MakeNormal(listId, TypeArgumentList.Make(yuTypeVar)));

            var moduleInfo = new ScriptModuleInfo("Script", new[] { xInfo, yInfo }, new[] { fInfo }, new[] { vInfo });

            var moduleInfoService = new ModuleInfoService(new[] { moduleInfo });
            var applier = new TypeValueApplier(moduleInfoService);

            return new TypeValueService(moduleInfoService, applier);
        }

        [Fact]
        public void GetTypeValue_VarValueTest()
        {
            // GetTypeValue(X<int>.Y<short>, v) => Dict<int, short>
            var typeValueService = MakeTypeValueService();

            var xId = ModuleItemId.Make("X", 2);
            var yId = xId.Append("Y", 1);
            var vId = yId.Append("v");

            // X<A>.Y<B>
            var yTypeArgList = TypeArgumentList.Make(
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") });

            var vValue = new VarValue(vId, yTypeArgList); // outerTypeArgList가 들어간다
            var result = typeValueService.GetTypeValue(vValue);
            var expected = TypeValue.MakeNormal(ModuleItemId.Make("Dict", 2), TypeArgumentList.Make(MakeTypeValue("A"), MakeTypeValue("C")));

            Assert.Equal(expected, result);
        }

        // 
        [Fact]
        public void GetTypeValue_FuncValueTest()
        {
            // GetTypeValue(X<A, B>.Y<C>.F<D>) => ((D, List<C>) => A)

            var typeValueService = MakeTypeValueService();
            
            var fId = ModuleItemId.Make(new ModuleItemIdElem("X", 2), new ModuleItemIdElem("Y", 1), new ModuleItemIdElem("F", 1));
            var fValue = new FuncValue(fId, TypeArgumentList.Make(
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") },
                new[] { MakeTypeValue("D") }));

            var result = typeValueService.GetTypeValue(fValue);
            var expected = TypeValue.MakeFunc(MakeTypeValue("A"), new[] {
                MakeTypeValue("D"),
                TypeValue.MakeNormal(ModuleItemId.Make(new ModuleItemIdElem("List", 1)), TypeArgumentList.Make(MakeTypeValue("C")))});

            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void GetBaseTypeValueTest()
        {
            // GetBaseTypeValue(X<A, B>.Y<C>) => G<A>           

            var typeValueService = MakeTypeValueService();

            var yId = ModuleItemId.Make(new ModuleItemIdElem("X", 2), new ModuleItemIdElem("Y", 1));
            var yValue = TypeValue.MakeNormal(yId, TypeArgumentList.Make(
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") }));

            if (!typeValueService.GetBaseTypeValue(yValue, out var baseTypeValue))
            {
                Assert.True(false, "");
                return;
            }

            var gId = ModuleItemId.Make(new ModuleItemIdElem("G", 1));
            var expected = TypeValue.MakeNormal(gId, TypeArgumentList.Make(MakeTypeValue("A")));
            Assert.Equal(expected, baseTypeValue);
        }

        [Fact]
        public void GetMemberFuncValueTest()
        {
            var typeValueService = MakeTypeValueService();

            // GetMemberFuncValue(X<A, B>.Y<C>, "F", D) => (X<,>.Y<>.F<>, [[A, B], [C], [D]])
            var yId = ModuleItemId.Make(new ModuleItemIdElem("X", 2), new ModuleItemIdElem("Y", 1));
            var yValue = TypeValue.MakeNormal(yId, TypeArgumentList.Make(
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") }));

            if (!typeValueService.GetMemberFuncValue(yValue, Name.MakeText("F"), new[] { MakeTypeValue("D") }, out var fValue))
            {
                Assert.True(false, "");
                return;
            }

            var fId = yId.Append("F", 1);
            var expected = new FuncValue(fId, TypeArgumentList.Make(
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") },
                new[] { MakeTypeValue("D") }));

            Assert.Equal(expected, fValue);
        }

        [Fact]
        public void GetMemberVarValueTest()
        {
            var typeValueService = MakeTypeValueService();

            // GetMemberVarValue(X<A, B>.Y<C>, "v") => (X<,>.Y<>.v, [[A, B], [C]])
            var yId = ModuleItemId.Make(new ModuleItemIdElem("X", 2), new ModuleItemIdElem("Y", 1));
            var yValue = TypeValue.MakeNormal(yId, TypeArgumentList.Make(
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") }));

            if (!typeValueService.GetMemberVarValue(yValue, Name.MakeText("v"), out var vValue))
            {
                Assert.True(false, "");
                return;
            }

            var vId = yId.Append("v");
            var expected = new VarValue(vId, TypeArgumentList.Make(
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") }));

            Assert.Equal(expected, vValue);
        }
        
    }
}