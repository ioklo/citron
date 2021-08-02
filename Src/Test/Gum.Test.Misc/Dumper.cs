using Gum.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test.Misc
{
    public struct Dumper
    {
        TextWriter writer;
        HashSet<object> visited;

        public static void Dump(TextWriter writer, object o)
        {
            var dumper = new Dumper() { writer = writer, visited = new HashSet<object>() };
            dumper.DumpCore(o, "");
        }

        public static string DumpToString(object o)
        {
            var stringWriter = new StringWriter();
            Dump(stringWriter, o);
            return stringWriter.ToString();
        }

        public void DumpCore(object o, string indent)
        {
            if (o == null)
            {
                writer.Write("null");
                return;
            }

            var type = o.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableArray<>))
                DumpImmutableArray(o, indent);
            else if (o is Enum e)
                writer.Write("{0}.{1}", e.GetType().Name, e);
            else if (o is ValueType)
                writer.Write(o);
            else if (o is string str)
                writer.Write($"\"{str}\"");
            else
                DumpObject(o, indent);
        }

        static IEnumerable<MemberInfo> GetMembers(Type type)
        {
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                yield return fieldInfo;

            //foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            //    yield return propertyInfo;

            //foreach (var memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            //    yield return memberInfo.Name;
        }

        void DumpImmutableArray(object o, string indent)
        {
            // ImmutableArray라면 
            var array = o.GetType().GetField("array", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(o) as IList;
            if (array == null)
            {
                writer.Write("[]");
                return;
            }

            bool isDefault = (bool)array.GetType().GetProperty("IsDefault")!.GetValue(array)!;

            // System.Collections.Immutable.ImmutableArray<>.Empty.Is
            if (isDefault || array.Count == 0)
            {
                writer.Write("[]");
                return;
            }

            bool bFirst = true;
            writer.WriteLine("[");
            var childIndent = indent + "    ";
            foreach(var item in array)
            {
                if (!bFirst) writer.WriteLine(",");
                else bFirst = false;

                writer.Write(childIndent);
                DumpCore(item, childIndent);                
            }

            writer.WriteLine();
            writer.Write($"{indent}]");
        }

        void DumpObject(object o, string indent)
        {
            if (visited.Contains(o))
            {
                writer.Write("$visited");
                return;
            }

            visited.Add(o);

            var type = o.GetType();
            writer.WriteLine("{");
            writer.Write($"{indent}    $type: \"{type.Name}\"");

            foreach (var memberInfo in GetMembers(type))
            {
                string name;
                object? memberObj;

                if (memberInfo is FieldInfo fieldInfo)
                {
                    name = fieldInfo.Name;
                    memberObj = fieldInfo.GetValue(o);
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    name = propertyInfo.Name;                    
                    try
                    {
                        memberObj = propertyInfo.GetValue(o);
                    }
                    catch
                    {   
                        continue;
                    }

                    memberObj = null;
                }
                else
                    continue;

                writer.WriteLine(",");
                writer.Write($"{indent}    {name}: ");

                if (memberObj != null)
                    DumpCore(memberObj, indent + "    ");
                else
                    writer.Write("null");
            }

            writer.WriteLine();
            writer.Write($"{indent}}}");
        }
    }
}
