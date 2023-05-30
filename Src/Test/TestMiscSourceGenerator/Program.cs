using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using IndentedTextWriter = Citron.Infra.IndentedTextWriter;

namespace TestMiscSourceGenerator
{
    class Program
    {
        Type[] types;

        Program(Type[] types)
        {
            this.types = types;
        }

        // syntax상에서 칠 이름 
        string GetNameForType(Type type)
        {
            if (type.DeclaringType != null)
            {
                var declaringTypeName = GetNameForType(type.DeclaringType);
                return $"{declaringTypeName}.{type.Name}";
            }
            else
            {
                if (type.Namespace == null)
                    return type.Name;

                return $"{type.Namespace}.{type.Name}";
            }
        }

        string GetNameForMethod(Type type)
        {
            if (type.DeclaringType != null)
            {
                var declaringTypeName = GetNameForMethod(type.DeclaringType);
                return $"{declaringTypeName}_{type.Name}";
            }
            else
            {
                return type.Name;
            }
        }

        string GetNameForVariable(Type type)
        {
            Debug.Assert(0 < type.Name.Length);

            // TODO: 첫글자만 lower, 추후 변경
            return "@" + char.ToLower(type.Name[0]) + type.Name[1..];
        }

        IEnumerable<(Type MemberType, string MemberName)> GetMemberTypeAndNames(Type type)
        {
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                switch (member)
                {
                    case FieldInfo field:
                        yield return (field.FieldType, field.Name);
                        break;

                    case PropertyInfo property:
                        yield return (property.PropertyType, property.Name);
                        break;
                }
            }
        }

        IEnumerable<Type> GetDerivedTypes(Type baseType)
        {
            foreach (var type in types)
                if (baseType.Equals(type.BaseType))
                    yield return type;
        }

        IEnumerable<string> GetStaticMembers(Type type)
        {
            var typeName = GetNameForType(type);

            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
            {
                switch (member)
                {
                    case FieldInfo field:
                        yield return $"{typeName}.{field.Name}";
                        break;

                    case PropertyInfo property:
                        yield return $"{typeName}.{property.Name}";
                        break;
                }
            }
        }

        void PrintEnum(IndentedTextWriter writer, Type type)
        {
            var nameForMethod = GetNameForMethod(type);
            var nameForType = GetNameForType(type);
            var nameForVar = GetNameForVariable(type);

            writer.WriteLine($"public void Write_{nameForMethod}({nameForType}? {nameForVar})");
            writer.WriteLine($"{{");            

            var writer1 = writer.Push();

            // null guard
            writer1.WriteLine($"if ({nameForVar} == null) {{ itw.Write(\"null\"); return; }}");
            writer1.WriteLine("");
            nameForVar = $"{nameForVar}.Value";

            writer1.WriteLine($"switch({nameForVar})");
            writer1.WriteLine($"{{");

            var writer2 = writer1.Push();
            foreach (var enumName in Enum.GetNames(type))
            {
                writer2.WriteLine($"case {nameForType}.{enumName}: itw.Write(\"{nameForType}.{enumName}\"); break;");
            }

            writer2.WriteLine($"default: throw new System.Diagnostics.UnreachableException();");

            writer1.WriteLine($"}}");
            writer.WriteLine($"}}");

            //void WriteInternalBinaryOperator(R.InternalBinaryOperator op)
            //{
            //    switch(op)
            //    {
            //        case R.InternalBinaryOperator.Multiply_Int_Int_Int: itw.Write("R.InternalBinaryOperator.Multiply_Int_Int_Int"); break;
            //        ...
            //        default: throw new System.Diagnostics.UnreachableException();
            //    }
            //}

        }

        void PrintAbstract(IndentedTextWriter writer, Type type)
        {
            // abstract case, 말단으로 토스
            // void Write_Name(Citron.Module.Name name)
            // {
            //     // static 처리
            //     if (name == Citron.Module.Names.IndexerGet)
            //     {
            //         writer.Write("Citron.Module.Names.IndexerGet");
            //         return;
            //     }
            //     
            //     switch(name)
            //     {
            //         case Citron.Module.Name.Normal normal: Write_Name_Normal(normal);
            //         case Citron.Module.Name.ConstructorParam constructorParam: Write_Name_ConstructorParam(constructorParam);
            //     }
            // }

            var nameForMethod = GetNameForMethod(type);
            var nameForType = GetNameForType(type);
            var nameForVariable = GetNameForVariable(type);

            writer.WriteLine($"public void Write_{nameForMethod}({nameForType}? {nameForVariable})");
            writer.WriteLine($"{{");

            var writer1 = writer.Push();
            var writer2 = writer1.Push();

            // null guard
            writer1.WriteLine($"if ({nameForVariable} == null) {{ itw.Write(\"null\"); return; }}");
            writer1.WriteLine("");

            foreach (var staticMember in GetStaticMembers(type))
            {
                writer1.WriteLine($"if ({nameForVariable} == {staticMember})");
                writer1.WriteLine($"{{");
                writer1.WriteLine($"    itw.Write(\"{staticMember}\");");
                writer1.WriteLine($"    return;");
                writer1.WriteLine($"}}");
                writer1.WriteLine();
            }

            // switch
            writer1.WriteLine($"switch({nameForVariable})");
            writer1.WriteLine($"{{");

            var derivedTypes = GetDerivedTypes(type);
            foreach (var derivedType in derivedTypes)
            {
                var derivedNameForType = GetNameForType(derivedType);
                var derivedNameForVariable = GetNameForVariable(derivedType);
                var derivedNameForMethod = GetNameForMethod(derivedType);

                writer2.WriteLine($"case {derivedNameForType} {derivedNameForVariable}: Write_{derivedNameForMethod}({derivedNameForVariable}); break;");
            }

            writer2.WriteLine($"default: throw new System.Diagnostics.UnreachableException();");
            writer1.WriteLine("}");
            writer.WriteLine("}");
        }

        private void PrintMember(IndentedTextWriter writer, string writerInstance, string instanceName, Type memberType, string memberName)
        {
            // 특수 처리 
            // 1. ImmutableArray<> 라면
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Citron.Collections.ImmutableArray<>))
            {
                // Write_ImmutableArray(Write_Name_Normal, "Citron.ModuleInfo.Name.Normal", x.Name);
                // Write_ImmutableArray(Write_{fieldNameForMethod}, "{fieldNameForType}", {nameForVariable}.{fieldName});

                var itemType = memberType.GetGenericArguments()[0];
                var itemTypeNameForMethod = GetNameForMethod(itemType);
                var itemTypeNameForType = GetNameForType(itemType);

                writer.WriteLine($"{writerInstance}.Write_ImmutableArray(Write_{itemTypeNameForMethod}, \"{itemTypeNameForType}\", {instanceName}.{memberName});");
            }
            // 2. 'nullable reference라면..' 을 .net 6+가 아니라면 공식적으로 얻어낼 수 없다. 그냥 함수들에게 ?를 붙인다
            // 3. ISymbolNode를 상속받았다면 
            else if (memberType.GetInterface(nameof(Citron.Symbol.ISymbolNode)) != null)
            {
                // Write_ISymbolNode(x.Symbol);
                writer.WriteLine($"{writerInstance}.Write_ISymbolNode({instanceName}.{memberName});");
            }
            else
            {
                var memberTypeNameForMethod = GetNameForMethod(memberType);
                writer.WriteLine($"{writerInstance}.Write_{memberTypeNameForMethod}({instanceName}.{memberName});");
            }
        }

        void PrintConcrete(IndentedTextWriter writer, Type type)
        {
            // void Write_Name_Normal(Citron.Module.Name.Normal? normal)
            // {
            //     if (normal == null) { itw.Write("null"); return; }
            //     itw.Write(@"new "new Citron.Module.Name.Normal(");
            //     Write_String(normal.Text);
            //     writer.Write(")");
            // }

            var nameForType = GetNameForType(type);
            var nameForMethod = GetNameForMethod(type);
            var nameForVariable = GetNameForVariable(type);

            writer.WriteLine($"public void Write_{nameForMethod}({nameForType}? {nameForVariable})");
            writer.WriteLine($"{{");

            var writer1 = writer.Push();            

            // null guard
            if (!type.IsValueType) // nullable reference
            {
                writer1.WriteLine($"if ({nameForVariable} == null) {{ itw.Write(\"null\"); return; }}");
                writer1.WriteLine();
            }
            else // nullable value
            {
                writer1.WriteLine($"if ({nameForVariable} == null) {{ itw.Write(\"null\"); return; }}");
                writer1.WriteLine();
                nameForVariable = $"{nameForVariable}.Value"; // nameForVariable 업데이트 
            }

            var memberInfos = GetMemberTypeAndNames(type).ToList();

            if (memberInfos.Count == 0)
            {
                writer1.WriteLine($"itw.Write(\"new {nameForType}();\");");
            }
            else if (memberInfos.Count == 1)
            {
                writer1.WriteLine($"itw.Write(\"new {nameForType}(\");");

                var (memberType, memberName) = memberInfos[0];

                PrintMember(writer1, "this", nameForVariable, memberType, memberName);

                writer1.WriteLine($"itw.Write(\")\");");
            }
            else
            {
                writer1.WriteLine("var itw1 = itw.Push();");
                writer1.WriteLine("var writer1 = new IR0Writer(itw1);");
                writer1.WriteLine("itw1.WriteLine();");
                writer1.WriteLine();

                writer1.WriteLine($"itw.Write(\"new {nameForType}(\");");

                var bFirst = true;
                foreach (var (memberType, memberName) in memberInfos)
                {
                    if (bFirst)
                        bFirst = false;
                    else
                        writer1.WriteLine($"itw1.WriteLine(\",\");");

                    PrintMember(writer1, "writer1", nameForVariable, memberType, memberName);
                }

                writer1.WriteLine($"itw.Write(\")\");");
            }

            writer.WriteLine("}");
        }

        void Print(IndentedTextWriter writer, Type type)
        {
            if (type.IsEnum)
                PrintEnum(writer, type);
            else if (type.IsAbstract)
                PrintAbstract(writer, type);
            else
                PrintConcrete(writer, type);
        }

        static IEnumerable<Type> GetDerivedTypesInSameAssembly(Type baseType)
        {
            foreach (var type in baseType.Assembly.GetTypes())
            {
                // if (baseType.Equals(type.BaseType))
                if (type.IsAssignableTo(baseType))
                    yield return type;
            }
        }

        void Print(IndentedTextWriter writer)
        {
            // namespace Citron.Test.Misc
            // {
            //     public partial struct IR0Writer
            //     {
            //         ...
            //     }

            writer.WriteLine("#nullable enable");
            writer.WriteLine();
            writer.WriteLine("namespace Citron.Test");
            writer.WriteLine("{");

            var writer1 = writer.Push();
            writer1.WriteLine("public partial struct IR0Writer");
            writer1.WriteLine("{");

            var writer2 = writer1.Push();
            var bFirst = true;
            foreach (var type in types)
            {
                if (bFirst) bFirst = false;
                else writer2.WriteLine();
                Print(writer2, type);
            }
            
            writer1.WriteLine("}");
            writer.WriteLine("}");            
        }

        static void AddWithDerivedInSameAssembly(HashSet<Type> types, Type type)
        {
            types.Add(type);

            foreach (var derivedType in GetDerivedTypesInSameAssembly(type))
                types.Add(derivedType);
        }

        static int Main(string[] args)
        {
            // 디렉토리가 와야 한다
            if (args.Length != 1)
                return 1;

            var types = new HashSet<Type>();

            types.Add(typeof(Citron.IR0.InternalBinaryOperator));
            types.Add(typeof(Citron.IR0.InternalUnaryOperator));
            types.Add(typeof(Citron.IR0.InternalUnaryAssignOperator));

            AddWithDerivedInSameAssembly(types, typeof(Citron.IR0.Argument));
            AddWithDerivedInSameAssembly(types, typeof(Citron.IR0.ReturnInfo));

            AddWithDerivedInSameAssembly(types, typeof(Citron.IR0.StringExpElement));
            AddWithDerivedInSameAssembly(types, typeof(Citron.Symbol.Name));
            AddWithDerivedInSameAssembly(types, typeof(Citron.IR0.Loc));
            AddWithDerivedInSameAssembly(types, typeof(Citron.IR0.Exp));
            AddWithDerivedInSameAssembly(types, typeof(Citron.IR0.Stmt));
            AddWithDerivedInSameAssembly(types, typeof(Citron.Symbol.IType));

            var program = new Program(types.ToArray());

            var filePath = Path.Combine(args[0], "IR0Writer.g.cs");
            using (var writer = new StreamWriter(filePath))
            {
                var itw = new IndentedTextWriter(writer);

                program.Print(itw);
            }

            return 0;
        }

    }
}
