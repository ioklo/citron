using Gum.CompileTime;
using System;
using System.Linq;
using Xunit;

namespace Gum.IR0
{
    public class TypeValueServiceTests
    {
        TypeValue MakeTypeValue(string name)
            => new TypeValue.Normal(ModuleName.Internal, NamespacePath.Root, new AppliedItemPathEntry(name));

        public ItemId DictId { get; } = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("Dict", 2));
        public ItemId ListId { get; } = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("List", 1));

        ItemId MakeItemId(ItemPathEntry entry0, params ItemPathEntry[] entries)
        {
            return new ItemId(ModuleName.Internal, NamespacePath.Root, entry0, entries);
        }

        // class X<T, U> { class Y<U> : G<T> { 
        //     Dict<T, U> v; 
        //     T F<V>(V v, List<U> u); 
        // } }
        TypeValueService MakeTypeValueService()
        {
            var xId = MakeItemId(new ItemPathEntry("X", 2));
            var yId = xId.Append("Y", 1);
            var vId = yId.Append("v");
            var fId = yId.Append("F", 1);                        

            var xtTypeVar = new TypeValue.TypeVar(0, 0, "T");
            var yuTypeVar = new TypeValue.TypeVar(1, 0, "U");
            var fvTypeVar = new TypeValue.TypeVar(2, 0, "V");

            var gId = MakeItemId(new ItemPathEntry("G", 1));
            var gtTypeValue = new TypeValue.Normal(gId, new[] { xtTypeVar });

            var vInfo = new VarInfo(vId, false, new TypeValue.Normal(DictId, new[] { xtTypeVar }, new [] { yuTypeVar }));
            var fInfo = new FuncInfo(fId, false, true, new[] { "V" }, xtTypeVar, fvTypeVar, new TypeValue.Normal(ListId, new[] { yuTypeVar }));

            var yInfo = new ClassInfo(yId, new[] { "U" }, gtTypeValue, new ItemInfo[] { fInfo, vInfo });
            var xInfo = new ClassInfo(xId, new[] { "T", "U" }, null, new[] { yInfo });

            var moduleInfo = new InternalModuleInfo(Array.Empty<NamespaceInfo>(), new ItemInfo[] { xInfo });

            var emptyModuleInfoRepo = new ModuleInfoRepository(Enumerable.Empty<IModuleInfo>());
            var itemInfoRepo = new TypeInfoRepository(moduleInfo, emptyModuleInfoRepo);
            var applier = new TypeValueApplier(emptyModuleInfoRepo);

            return new TypeValueService(itemInfoRepo, applier);
        }

        [Fact]
        public void GetTypeValue_VarValueTest()
        {
            // GetTypeValue(X<int>.Y<short>, v) => Dict<int, short>
            var typeValueService = MakeTypeValueService();

            var xId = MakeItemId(new ItemPathEntry("X", 2));
            var yId = xId.Append("Y", 1);
            var vId = yId.Append("v");

            // X<A>.Y<B>
            var yTypeArgList = new[] {
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") }
            };
            
            
            

            var vValue = new MemberVarValue(vId, yTypeArgList); // outerTypeArgList가 들어간다
            var result = typeValueService.GetTypeValue(vValue);
            var expected = new TypeValue.Normal(DictId, new[] { new[] { MakeTypeValue("A"), MakeTypeValue("C") } });

            Assert.Equal(expected, result);
        }

        // 
        [Fact]
        public void GetTypeValue_FuncValueTest()
        {
            // GetTypeValue(X<A, B>.Y<C>.F<D>) => ((D, List<C>) => A)

            var typeValueService = MakeTypeValueService();
            
            var fId = MakeItemId(new ItemPathEntry("X", 2), new ItemPathEntry("Y", 1), new ItemPathEntry("F", 1));
            var fValue = new FuncValue(fId, 
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") },
                new[] { MakeTypeValue("D") } 
            );

            var result = typeValueService.GetTypeValue(fValue);

            var expected = new TypeValue.Func(MakeTypeValue("A"), new[] {
                MakeTypeValue("D"),
                new TypeValue.Normal(ListId, new [] { MakeTypeValue("C") })});

            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void GetBaseTypeValueTest()
        {
            // GetBaseTypeValue(X<A, B>.Y<C>) => G<A>           

            var typeValueService = MakeTypeValueService();

            var yId = MakeItemId(new ItemPathEntry("X", 2), new ItemPathEntry("Y", 1));
            var yValue = new TypeValue.Normal(yId, 
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") }
            );

            if (!typeValueService.GetBaseTypeValue(yValue, out var baseTypeValue))
            {
                Assert.True(false, "");
                return;
            }

            var gId = MakeItemId(new ItemPathEntry("G", 1));
            var expected = new TypeValue.Normal(gId, new[] { MakeTypeValue("A") });
            Assert.Equal(expected, baseTypeValue);
        }

        [Fact]
        public void GetMemberFuncValueTest()
        {
            var typeValueService = MakeTypeValueService();

            // GetMemberFuncValue(X<A, B>.Y<C>, "F", D) => (X<,>.Y<>.F<>, [[A, B], [C], [D]])
            var yId = MakeItemId(new ItemPathEntry("X", 2), new ItemPathEntry("Y", 1));
            var yValue = new TypeValue.Normal(yId, 
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") } );

            if (!typeValueService.GetMemberFuncValue(yValue, "F", new[] { MakeTypeValue("D") }, out var fValue))
            {
                Assert.True(false, "");
                return;
            }

            var fId = yId.Append("F", 1);
            var expected = new FuncValue(fId, 
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") },
                new[] { MakeTypeValue("D") } );

            Assert.Equal(expected, fValue);
        }

        [Fact]
        public void GetMemberVarValueTest()
        {
            var typeValueService = MakeTypeValueService();

            // GetMemberVarValue(X<A, B>.Y<C>, "v") => (X<,>.Y<>.v, [[A, B], [C]])
            var yId = MakeItemId(new ItemPathEntry("X", 2), new ItemPathEntry("Y", 1));
            var yValue = new TypeValue.Normal(yId, 
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") });

            if (!typeValueService.GetMemberVarValue(yValue, "v", out var vValue))
            {
                Assert.True(false, "");
                return;
            }

            var vId = yId.Append("v");
            var expected = new MemberVarValue(vId, 
                new[] { MakeTypeValue("A"), MakeTypeValue("B") },
                new[] { MakeTypeValue("C") });

            Assert.Equal(expected, vValue);
        }
        
    }
}