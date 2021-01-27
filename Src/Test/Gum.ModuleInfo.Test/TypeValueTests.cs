using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Xunit;

namespace Gum.CompileTime
{
    public class TypeValueTests
    {


        [Fact]
        public void TestMakeNormal()
        {
            // System.List<System.Int32>만들어 보기
            var intType = new ExternalType("System.Runtime", new NamespacePath("System"), "Int32", ImmutableArray<Type>.Empty);
            var intListType = new ExternalType("System.Runtime", new NamespacePath("System"), "List", ImmutableArray.Create<Type>(intType));
            
            Assert.Equal("System.Runtime", intListType.ModuleName);
            Assert.Equal("System", intListType.NamespacePath.Entries[0].Value);
            Assert.Equal("List", intListType.Name);
            Assert.Equal(intType, intListType.TypeArgs[0]);
        }

        [Fact]
        public void TestMakeNestedNormal()
        {
            var moduleName = (ModuleName)"MyModule";

            // class X<T> { class Y<T> { class Z<U> { } } }
            // X<int>.Y<short>.Z<int> 만들어 보기
            var intType = new ExternalType("System.Runtime", new NamespacePath("System"), "Int32", ImmutableArray<Type>.Empty);
            var shortType = new ExternalType("System.Runtime", new NamespacePath("System"), "Int16", ImmutableArray<Type>.Empty);
            var zType = new MemberType(
                new MemberType(
                    new ExternalType(moduleName, NamespacePath.Root, "X", ImmutableArray.Create<Type>(intType)),
                    "Y",
                    ImmutableArray.Create<Type>(shortType)
                ),
                "Z",
                ImmutableArray.Create<Type>(intType)
            );

            Assert.Equal("MyModule", ((ExternalType)((MemberType)zType.Outer).Outer).ModuleName);
            Assert.True(((ExternalType)((MemberType)zType.Outer).Outer).NamespacePath.IsRoot);
            Assert.Equal("X", ((ExternalType)((MemberType)zType.Outer).Outer).Name);
            Assert.Equal(intType, ((ExternalType)((MemberType)zType.Outer).Outer).TypeArgs[0]);

            Assert.Equal("Y", ((MemberType)zType.Outer).Name);
            Assert.Equal(shortType, ((MemberType)zType.Outer).TypeArgs[0]);

            Assert.Equal("Z", zType.Name);
            Assert.Equal(intType, zType.TypeArgs[0]);
        }
    }
}
