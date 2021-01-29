using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Gum.Misc;

using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Gum.IR0
{
    partial class Misc
    {
        public static ImmutableArray<M.Type>? ToMTypeArgs(ImmutableArray<TypeExpInfo> typeArgs)
        {
            var builder = ImmutableArray.CreateBuilder<M.Type>();
            foreach (var typeArg in typeArgs)
            {
                if (typeArg is MTypeTypeExpInfo mtypeArg)
                    builder.Add(mtypeArg.Type);
                else
                    return null; // 실패
            }

            return builder.ToImmutable();
        }

        //public static M.Type? ToModuleType(TypeValue typeValue)
        //{
        //    switch (typeValue)
        //    {
        //        case VarTypeValue _:
        //            return null;

        //        case TypeVarTypeValue typeVar:
        //            return new TypeVarType(typeVar.Depth, typeVar.Index, typeVar.Name);

        //        case NormalTypeValue normalTypeValue:
        //            throw new NotImplementedException();

        //        case VoidTypeValue:
        //            return VoidType.Instance;

        //        case FuncTypeValue:
        //            return null;

        //        case EnumElemTypeValue:
        //            return null;

        //        default:
        //            throw new UnreachableCodeException();
        //    }
        //}

        static void FillTypeString(M.Name name, ImmutableArray<M.Type> typeArgs, StringBuilder sb)
        {
            // X
            sb.Append(name);

            // <int>
            if (typeArgs.Length != 0)
            {
                sb.Append('<');

                bool bFirst = true;
                foreach (var typeArg in typeArgs)
                {
                    if (bFirst) bFirst = false;
                    else sb.Append(',');

                    FillTypeString(typeArg, sb);
                }

                sb.Append('>');
            }
        }

        static void FillNamespacePath(M.NamespacePath namespacePath, StringBuilder sb)
        {
            // Namespace1.Namespace2.
            if (namespacePath.Entries.Length != 0)
            {
                sb.AppendJoin('.', namespacePath.Entries.Select(entry => entry.Value));
                sb.Append('.');
            }
        }

        // [ModuleName].Namespace.Namespace.Type...
        // TypeString을 만드는데 InternalType이면 어떻게 하나..
        // void Func([AModule]MyType m);  // ParamHash
        // void Func([BModule]MyType m);  // ParamHash.. 둘이 다른 타입인데
        // void Func([Internal]MyType m); // Internal이지만, 이름은 있어야 한다.. (Internal은 Func가 나온 모듈)
        static void FillTypeString(M.Type type, StringBuilder sb)
        {
            switch (type)
            {
                case M.TypeVarType typeVar:
                    sb.Append($"`({typeVar.Depth}, {typeVar.Index})");
                    break;
                
                case M.ExternalType externalType:
                    // [ModuleName]Namespace1.Namespace2.X<int>

                    // [ModuleName]
                    sb.Append($"[{externalType.ModuleName}]");

                    // Namespace1.Namespace2.
                    FillNamespacePath(externalType.NamespacePath, sb);

                    // X<int>
                    FillTypeString(externalType.Name, externalType.TypeArgs, sb);                    
                    break;

                case M.MemberType memberType: // [ModuleName]N1.N2.X<int, short>.Y<string>
                    // [ModuleName]N1.N2.X<int, short>.
                    FillTypeString(memberType.Outer, sb);
                    sb.Append('.');

                    // Y<string>
                    FillTypeString(memberType.Name, memberType.TypeArgs, sb);
                    break;

                case M.VoidType _:
                    sb.Append("void");
                    break;
            }
        }

        public static void VisitScript(S.Script script, ISyntaxScriptVisitor visitor)
        {
            foreach(var elem in script.Elements)
            {
                switch(elem)
                {
                    case S.Script.GlobalFuncDeclElement globalFuncDeclElem:
                        visitor.VisitGlobalFuncDecl(globalFuncDeclElem.FuncDecl);
                        break;

                    case S.Script.StmtElement stmtElem:
                        visitor.VisitTopLevelStmt(stmtElem.Stmt);
                        break;

                    case S.Script.TypeDeclElement typeDeclElem:
                        visitor.VisitTypeDecl(typeDeclElem.TypeDecl);
                        break;
                }                
            }
        }

        public static string MakeParamHash(ImmutableArray<M.Type> types)
        {
            var sb = new StringBuilder();

            bool bFirst = true;
            foreach (var type in types)
            {
                if (bFirst) bFirst = false;
                else sb.Append(" * ");

                FillTypeString(type, sb);
            }

            return sb.ToString();
        }
    }
}
