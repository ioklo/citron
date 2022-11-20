using System;
using System.Collections;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using Xunit;
using static Citron.Infra.Misc;

namespace Citron.Module
{
    public class TypeTests
    {
        static Name N(string text)
        {
            return new Name.Normal(text);
        }

        static NamespacePath NS(string name)
        {
            return new NamespacePath(null, N(name));
        }

        [Fact]
        public void TestMakeNormal()
        {
            // System.List<System.Int32>만들어 보기
            var intType = new RootTypeId(N("System.Runtime"), NS("System"), N("Int32"), default);
            var intListType = new RootTypeId(N("System.Runtime"), NS("System"), N("List"), Arr<TypeId>(intType));
            
            Assert.Equal(N("System.Runtime"), intListType.Module);
            Assert.Equal(N("System"), intListType.Namespace!.Name);
            Assert.Equal(N("List"), intListType.Name);
            Assert.Equal(intType, intListType.TypeArgs[0]);
        }

        [Fact]
        public void TestMakeNestedNormal()
        {
            var moduleName = N("MyModule");

            // class X<T> { class Y<T> { class Z<U> { } } }
            // X<int>.Y<short>.Z<int> 만들어 보기
            var intType = new RootTypeId(N("System.Runtime"), NS("System"), N("Int32"), default);
            var shortType = new RootTypeId(N("System.Runtime"), NS("System"), N("Int16"), default);
            var zType = new MemberTypeId(
                new MemberTypeId(
                    new RootTypeId(moduleName, null, N("X"), ImmutableArray.Create<TypeId>(intType)),
                    N("Y"),
                    ImmutableArray.Create<TypeId>(shortType)
                ),
                N("Z"),
                ImmutableArray.Create<TypeId>(intType)
            );

            Assert.Equal(N("MyModule"), ((RootTypeId)((MemberTypeId)zType.Outer).Outer).Module);
            Assert.Null(((RootTypeId)((MemberTypeId)zType.Outer).Outer).Namespace);
            Assert.Equal(N("X"), ((RootTypeId)((MemberTypeId)zType.Outer).Outer).Name);
            Assert.Equal(intType, ((RootTypeId)((MemberTypeId)zType.Outer).Outer).TypeArgs[0]);

            Assert.Equal(N("Y"), ((MemberTypeId)zType.Outer).Name);
            Assert.Equal(shortType, ((MemberTypeId)zType.Outer).TypeArgs[0]);

            Assert.Equal(N("Z"), zType.Name);
            Assert.Equal(intType, zType.TypeArgs[0]);
        }
    }
}
