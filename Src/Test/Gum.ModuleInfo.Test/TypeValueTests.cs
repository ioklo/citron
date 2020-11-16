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
        // QsTypeValue.Normal테스트
        [Fact]
        public void TestMakeNormal()
        {
            // System.List<System.Int32>만들어 보기
            var appliedInt = new TypeValue.Normal("System.Runtime", new AppliedItemPath(new NamespacePath("System"), new AppliedItemPathEntry("Int32")));

            var appliedList = new TypeValue.Normal("System.Runtime", 
                new AppliedItemPath(
                    new NamespacePath("System"),
                    new AppliedItemPathEntry("List", string.Empty, new[] { appliedInt })));
            
            Assert.Equal(appliedList.ModuleName, (ModuleName)"System.Runtime");
            Assert.True(appliedList.NamespacePath.Entries.Length == 1 && appliedList.NamespacePath.Entries[0].Value == "System");
            Assert.True(appliedList.OuterEntries.Length == 0);            
            Assert.Equal(appliedList.Entry.Name, (Name)"List");
            Assert.Equal(appliedList.Entry.ParamHash, string.Empty);
            Assert.True(appliedList.Entry.TypeArgs.Length == 1 && 
                ModuleInfoEqualityComparer.EqualsTypeValue(appliedList.Entry.TypeArgs[0], appliedInt));
        }

        [Fact]
        public void TestMakeNestedNormal()
        {
            // class X<T> { class Y<T> { class Z<U> { } } }
            // X<int>.Y<short>.Z<int> 만들어 보기            

            var intId = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("Int32"));
            var shortId = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("Int16"));
            var zId = new ItemId(ModuleName.Internal, NamespacePath.Root, new ItemPathEntry("X", 1), new ItemPathEntry("Y", 1), new ItemPathEntry("Z", 1));

            var intTV = new TypeValue.Normal("System.Runtime", new AppliedItemPath(new NamespacePath("System"), new AppliedItemPathEntry("Int32")));
            var shortTV = new TypeValue.Normal("System.Runtime", new AppliedItemPath(new NamespacePath("System"), new AppliedItemPathEntry("Int16")));
            var zTV = new TypeValue.Normal(ModuleName.Internal, NamespacePath.Root,
                new[] {
                    new AppliedItemPathEntry("X", string.Empty, new[]{ intTV }),
                    new AppliedItemPathEntry("Y", string.Empty, new[]{ intTV }),                    
                },
                new AppliedItemPathEntry("Z", string.Empty, new[] { shortTV })
            );

            Assert.True(ModuleInfoEqualityComparer.EqualsModuleName(zTV.ModuleName, ModuleName.Internal));
            Assert.True(ModuleInfoEqualityComparer.EqualsNamespacePath(zTV.NamespacePath, NamespacePath.Root));
            Assert.True(ModuleInfoEqualityComparer.EqualsAppliedItemPathEntry(zTV.OuterEntries[0], new AppliedItemPathEntry("X", string.Empty, new[] { intTV })));
            Assert.True(ModuleInfoEqualityComparer.EqualsAppliedItemPathEntry(zTV.OuterEntries[1], new AppliedItemPathEntry("Y", string.Empty, new[] { intTV })));
            Assert.True(ModuleInfoEqualityComparer.EqualsAppliedItemPathEntry(zTV.Entry, new AppliedItemPathEntry("Z", string.Empty, new[] { shortTV })));
        }
    }
}
