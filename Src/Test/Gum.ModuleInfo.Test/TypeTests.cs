using System;
using System.Collections;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Xunit;

namespace Gum.CompileTime
{
    public class TypeTests
    {


        [Fact]
        public void TestMakeNormal()
        {
            // System.List<System.Int32>만들어 보기
            var intType = new GlobalType("System.Runtime", new NamespacePath("System"), new Name.Normal("Int32"), ImmutableArray<Type>.Empty);
            var intListType = new GlobalType("System.Runtime", new NamespacePath("System"), new Name.Normal("List"), ImmutableArray.Create<Type>(intType));
            
            Assert.Equal("System.Runtime", intListType.ModuleName);
            Assert.Equal("System", intListType.NamespacePath.Entries[0].Value);
            Assert.Equal(new Name.Normal("List"), intListType.Name);
            Assert.Equal(intType, intListType.TypeArgs[0]);
        }

        [Fact]
        public void TestMakeNestedNormal()
        {
            var moduleName = (ModuleName)"MyModule";

            // class X<T> { class Y<T> { class Z<U> { } } }
            // X<int>.Y<short>.Z<int> 만들어 보기
            var intType = new GlobalType("System.Runtime", new NamespacePath("System"), new Name.Normal("Int32"), ImmutableArray<Type>.Empty);
            var shortType = new GlobalType("System.Runtime", new NamespacePath("System"), new Name.Normal("Int16"), ImmutableArray<Type>.Empty);
            var zType = new MemberType(
                new MemberType(
                    new GlobalType(moduleName, NamespacePath.Root, new Name.Normal("X"), ImmutableArray.Create<Type>(intType)),
                    new Name.Normal("Y"),
                    ImmutableArray.Create<Type>(shortType)
                ),
                new Name.Normal("Z"),
                ImmutableArray.Create<Type>(intType)
            );

            Assert.Equal("MyModule", ((GlobalType)((MemberType)zType.Outer).Outer).ModuleName);
            Assert.True(((GlobalType)((MemberType)zType.Outer).Outer).NamespacePath.IsRoot);
            Assert.Equal(new Name.Normal("X"), ((GlobalType)((MemberType)zType.Outer).Outer).Name);
            Assert.Equal(intType, ((GlobalType)((MemberType)zType.Outer).Outer).TypeArgs[0]);

            Assert.Equal(new Name.Normal("Y"), ((MemberType)zType.Outer).Name);
            Assert.Equal(shortType, ((MemberType)zType.Outer).TypeArgs[0]);

            Assert.Equal(new Name.Normal("Z"), zType.Name);
            Assert.Equal(intType, zType.TypeArgs[0]);
        }
    }
}
