using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.IR0Translator
{
    class ItemIds
    {
        public static ItemId Int { get; } = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("Int32"));
        public static ItemId Bool { get; } = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("Boolean"));
        public static ItemId String { get; } = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("String"));
        public static ItemId List { get; } = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("List", 1));
        public static ItemId Enumerable { get; } = new ItemId("System.Runtime", new NamespacePath("System"), new ItemPathEntry("Enumerable", 1));
    }
}
