using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gum.CompileTime
{
    public class TypeValueTests
    {
        // QsTypeValue.Normal테스트
        [Fact]
        public void TestMakeNormal()
        {
            // List<int>만들어 보기 
            var listId = ModuleItemId.Make("List");
            var intId = ModuleItemId.Make("int");
            var tv = TypeValue.MakeNormal(listId, TypeArgumentList.Make(TypeValue.MakeNormal(intId)));

            Assert.Equal(tv.TypeId, listId);
            Assert.Equal(tv.TypeArgList.Args[0], TypeValue.MakeNormal(intId));
        }

        [Fact]
        public void TestMakeNestedNormal()
        {
            // class X<T> { class Y<T> { class Z<U> { } } }
            // X<int>.Y<short>.Z<int> 만들어 보기            

            var intId = ModuleItemId.Make("int");
            var shortId = ModuleItemId.Make("short");
            var zId = ModuleItemId.Make(new ModuleItemIdElem("X", 1), new ModuleItemIdElem("Y", 1), new ModuleItemIdElem("Z", 1));

            var tv = TypeValue.MakeNormal(zId, TypeArgumentList.Make(
                TypeValue.MakeNormal(intId), TypeValue.MakeNormal(intId), TypeValue.MakeNormal(shortId)));

            Assert.Equal(tv.TypeId, zId);
            Assert.Equal(tv.TypeArgList.Args[0], TypeValue.MakeNormal(intId));
            Assert.Equal(tv.TypeArgList.Args[1], TypeValue.MakeNormal(intId));
            Assert.Equal(tv.TypeArgList.Args[2], TypeValue.MakeNormal(shortId));
        }

    }
}
