#nullable enable

namespace Citron.Test
{
    public partial struct IR0Writer
    {
        public void Write_InternalBinaryOperator(Citron.IR0.InternalBinaryOperator? @internalBinaryOperator)
        {
            if (@internalBinaryOperator == null) { itw.Write("null"); return; }

            switch(@internalBinaryOperator.Value)
            {
                case Citron.IR0.InternalBinaryOperator.Multiply_Int_Int_Int: itw.Write("Citron.IR0.InternalBinaryOperator.Multiply_Int_Int_Int"); break;
                case Citron.IR0.InternalBinaryOperator.Divide_Int_Int_Int: itw.Write("Citron.IR0.InternalBinaryOperator.Divide_Int_Int_Int"); break;
                case Citron.IR0.InternalBinaryOperator.Modulo_Int_Int_Int: itw.Write("Citron.IR0.InternalBinaryOperator.Modulo_Int_Int_Int"); break;
                case Citron.IR0.InternalBinaryOperator.Add_Int_Int_Int: itw.Write("Citron.IR0.InternalBinaryOperator.Add_Int_Int_Int"); break;
                case Citron.IR0.InternalBinaryOperator.Add_String_String_String: itw.Write("Citron.IR0.InternalBinaryOperator.Add_String_String_String"); break;
                case Citron.IR0.InternalBinaryOperator.Subtract_Int_Int_Int: itw.Write("Citron.IR0.InternalBinaryOperator.Subtract_Int_Int_Int"); break;
                case Citron.IR0.InternalBinaryOperator.LessThan_Int_Int_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.LessThan_Int_Int_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.LessThan_String_String_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.LessThan_String_String_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.GreaterThan_Int_Int_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.GreaterThan_Int_Int_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.GreaterThan_String_String_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.GreaterThan_String_String_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.LessThanOrEqual_String_String_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.LessThanOrEqual_String_String_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.Equal_Int_Int_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.Equal_Int_Int_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.Equal_Bool_Bool_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.Equal_Bool_Bool_Bool"); break;
                case Citron.IR0.InternalBinaryOperator.Equal_String_String_Bool: itw.Write("Citron.IR0.InternalBinaryOperator.Equal_String_String_Bool"); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_InternalUnaryOperator(Citron.IR0.InternalUnaryOperator? @internalUnaryOperator)
        {
            if (@internalUnaryOperator == null) { itw.Write("null"); return; }

            switch(@internalUnaryOperator.Value)
            {
                case Citron.IR0.InternalUnaryOperator.LogicalNot_Bool_Bool: itw.Write("Citron.IR0.InternalUnaryOperator.LogicalNot_Bool_Bool"); break;
                case Citron.IR0.InternalUnaryOperator.UnaryMinus_Int_Int: itw.Write("Citron.IR0.InternalUnaryOperator.UnaryMinus_Int_Int"); break;
                case Citron.IR0.InternalUnaryOperator.ToString_Bool_String: itw.Write("Citron.IR0.InternalUnaryOperator.ToString_Bool_String"); break;
                case Citron.IR0.InternalUnaryOperator.ToString_Int_String: itw.Write("Citron.IR0.InternalUnaryOperator.ToString_Int_String"); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_InternalUnaryAssignOperator(Citron.IR0.InternalUnaryAssignOperator? @internalUnaryAssignOperator)
        {
            if (@internalUnaryAssignOperator == null) { itw.Write("null"); return; }

            switch(@internalUnaryAssignOperator.Value)
            {
                case Citron.IR0.InternalUnaryAssignOperator.PrefixInc_Int_Int: itw.Write("Citron.IR0.InternalUnaryAssignOperator.PrefixInc_Int_Int"); break;
                case Citron.IR0.InternalUnaryAssignOperator.PrefixDec_Int_Int: itw.Write("Citron.IR0.InternalUnaryAssignOperator.PrefixDec_Int_Int"); break;
                case Citron.IR0.InternalUnaryAssignOperator.PostfixInc_Int_Int: itw.Write("Citron.IR0.InternalUnaryAssignOperator.PostfixInc_Int_Int"); break;
                case Citron.IR0.InternalUnaryAssignOperator.PostfixDec_Int_Int: itw.Write("Citron.IR0.InternalUnaryAssignOperator.PostfixDec_Int_Int"); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_Argument(Citron.IR0.Argument? @argument)
        {
            if (@argument == null) { itw.Write("null"); return; }

            switch(@argument)
            {
                case Citron.IR0.Argument.Normal @normal: Write_Argument_Normal(@normal); break;
                case Citron.IR0.Argument.Params @params: Write_Argument_Params(@params); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_Argument_Normal(Citron.IR0.Argument.Normal? @normal)
        {
            if (@normal == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.Argument.Normal(");
            this.Write_Exp(@normal.Exp);
            itw.Write(")");
        }

        public void Write_Argument_Params(Citron.IR0.Argument.Params? @params)
        {
            if (@params == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.Argument.Params(");
            writer1.Write_Exp(@params.Exp);
            itw1.WriteLine(",");
            writer1.Write_Int32(@params.ElemCount);
            itw.Write(")");
        }

        public void Write_ReturnInfo(Citron.IR0.ReturnInfo? @returnInfo)
        {
            if (@returnInfo == null) { itw.Write("null"); return; }

            switch(@returnInfo)
            {
                case Citron.IR0.ReturnInfo.Expression @expression: Write_ReturnInfo_Expression(@expression); break;
                case Citron.IR0.ReturnInfo.None @none: Write_ReturnInfo_None(@none); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_ReturnInfo_Expression(Citron.IR0.ReturnInfo.Expression? @expression)
        {
            if (@expression == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.ReturnInfo.Expression(");
            this.Write_Exp(@expression.Exp);
            itw.Write(")");
        }

        public void Write_ReturnInfo_None(Citron.IR0.ReturnInfo.None? @none)
        {
            if (@none == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.ReturnInfo.None();");
        }

        public void Write_StringExpElement(Citron.IR0.StringExpElement? @stringExpElement)
        {
            if (@stringExpElement == null) { itw.Write("null"); return; }

            switch(@stringExpElement)
            {
                case Citron.IR0.TextStringExpElement @textStringExpElement: Write_TextStringExpElement(@textStringExpElement); break;
                case Citron.IR0.ExpStringExpElement @expStringExpElement: Write_ExpStringExpElement(@expStringExpElement); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_TextStringExpElement(Citron.IR0.TextStringExpElement? @textStringExpElement)
        {
            if (@textStringExpElement == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.TextStringExpElement(");
            this.Write_String(@textStringExpElement.Text);
            itw.Write(")");
        }

        public void Write_ExpStringExpElement(Citron.IR0.ExpStringExpElement? @expStringExpElement)
        {
            if (@expStringExpElement == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.ExpStringExpElement(");
            this.Write_Exp(@expStringExpElement.Exp);
            itw.Write(")");
        }

        public void Write_Name(Citron.Symbol.Name? @name)
        {
            if (@name == null) { itw.Write("null"); return; }

            switch(@name)
            {
                case Citron.Symbol.Name.Singleton @singleton: Write_Name_Singleton(@singleton); break;
                case Citron.Symbol.Name.Anonymous @anonymous: Write_Name_Anonymous(@anonymous); break;
                case Citron.Symbol.Name.ConstructorParam @constructorParam: Write_Name_ConstructorParam(@constructorParam); break;
                case Citron.Symbol.Name.Normal @normal: Write_Name_Normal(@normal); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_Name_Singleton(Citron.Symbol.Name.Singleton? @singleton)
        {
            if (@singleton == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.Name.Singleton(");
            this.Write_String(@singleton.DebugText);
            itw.Write(")");
        }

        public void Write_Name_Anonymous(Citron.Symbol.Name.Anonymous? @anonymous)
        {
            if (@anonymous == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.Name.Anonymous(");
            this.Write_Int32(@anonymous.Index);
            itw.Write(")");
        }

        public void Write_Name_ConstructorParam(Citron.Symbol.Name.ConstructorParam? @constructorParam)
        {
            if (@constructorParam == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.Symbol.Name.ConstructorParam(");
            writer1.Write_Int32(@constructorParam.Index);
            itw1.WriteLine(",");
            writer1.Write_String(@constructorParam.Text);
            itw.Write(")");
        }

        public void Write_Name_Normal(Citron.Symbol.Name.Normal? @normal)
        {
            if (@normal == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.Name.Normal(");
            this.Write_String(@normal.Text);
            itw.Write(")");
        }

        public void Write_Loc(Citron.IR0.Loc? @loc)
        {
            if (@loc == null) { itw.Write("null"); return; }

            switch(@loc)
            {
                case Citron.IR0.TempLoc @tempLoc: Write_TempLoc(@tempLoc); break;
                case Citron.IR0.LocalVarLoc @localVarLoc: Write_LocalVarLoc(@localVarLoc); break;
                case Citron.IR0.LambdaMemberVarLoc @lambdaMemberVarLoc: Write_LambdaMemberVarLoc(@lambdaMemberVarLoc); break;
                case Citron.IR0.ListIndexerLoc @listIndexerLoc: Write_ListIndexerLoc(@listIndexerLoc); break;
                case Citron.IR0.StructMemberLoc @structMemberLoc: Write_StructMemberLoc(@structMemberLoc); break;
                case Citron.IR0.ClassMemberLoc @classMemberLoc: Write_ClassMemberLoc(@classMemberLoc); break;
                case Citron.IR0.EnumElemMemberLoc @enumElemMemberLoc: Write_EnumElemMemberLoc(@enumElemMemberLoc); break;
                case Citron.IR0.ThisLoc @thisLoc: Write_ThisLoc(@thisLoc); break;
                case Citron.IR0.LocalDerefLoc @localDerefLoc: Write_LocalDerefLoc(@localDerefLoc); break;
                case Citron.IR0.NullableValueLoc @nullableValueLoc: Write_NullableValueLoc(@nullableValueLoc); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_TempLoc(Citron.IR0.TempLoc? @tempLoc)
        {
            if (@tempLoc == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.TempLoc(");
            this.Write_Exp(@tempLoc.Exp);
            itw.Write(")");
        }

        public void Write_LocalVarLoc(Citron.IR0.LocalVarLoc? @localVarLoc)
        {
            if (@localVarLoc == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.LocalVarLoc(");
            this.Write_Name(@localVarLoc.Name);
            itw.Write(")");
        }

        public void Write_LambdaMemberVarLoc(Citron.IR0.LambdaMemberVarLoc? @lambdaMemberVarLoc)
        {
            if (@lambdaMemberVarLoc == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.LambdaMemberVarLoc(");
            this.Write_ISymbolNode(@lambdaMemberVarLoc.MemberVar);
            itw.Write(")");
        }

        public void Write_ListIndexerLoc(Citron.IR0.ListIndexerLoc? @listIndexerLoc)
        {
            if (@listIndexerLoc == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.ListIndexerLoc(");
            writer1.Write_Loc(@listIndexerLoc.List);
            itw1.WriteLine(",");
            writer1.Write_Exp(@listIndexerLoc.Index);
            itw.Write(")");
        }

        public void Write_StructMemberLoc(Citron.IR0.StructMemberLoc? @structMemberLoc)
        {
            if (@structMemberLoc == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.StructMemberLoc(");
            writer1.Write_Loc(@structMemberLoc.Instance);
            itw1.WriteLine(",");
            writer1.Write_ISymbolNode(@structMemberLoc.MemberVar);
            itw.Write(")");
        }

        public void Write_ClassMemberLoc(Citron.IR0.ClassMemberLoc? @classMemberLoc)
        {
            if (@classMemberLoc == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.ClassMemberLoc(");
            writer1.Write_Loc(@classMemberLoc.Instance);
            itw1.WriteLine(",");
            writer1.Write_ISymbolNode(@classMemberLoc.MemberVar);
            itw.Write(")");
        }

        public void Write_EnumElemMemberLoc(Citron.IR0.EnumElemMemberLoc? @enumElemMemberLoc)
        {
            if (@enumElemMemberLoc == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.EnumElemMemberLoc(");
            writer1.Write_Loc(@enumElemMemberLoc.Instance);
            itw1.WriteLine(",");
            writer1.Write_ISymbolNode(@enumElemMemberLoc.MemberVar);
            itw.Write(")");
        }

        public void Write_ThisLoc(Citron.IR0.ThisLoc? @thisLoc)
        {
            if (@thisLoc == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.ThisLoc();");
        }

        public void Write_LocalDerefLoc(Citron.IR0.LocalDerefLoc? @localDerefLoc)
        {
            if (@localDerefLoc == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.LocalDerefLoc(");
            this.Write_Loc(@localDerefLoc.InnerLoc);
            itw.Write(")");
        }

        public void Write_NullableValueLoc(Citron.IR0.NullableValueLoc? @nullableValueLoc)
        {
            if (@nullableValueLoc == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.NullableValueLoc(");
            this.Write_Loc(@nullableValueLoc.Loc);
            itw.Write(")");
        }

        public void Write_Exp(Citron.IR0.Exp? @exp)
        {
            if (@exp == null) { itw.Write("null"); return; }

            switch(@exp)
            {
                case Citron.IR0.LoadExp @loadExp: Write_LoadExp(@loadExp); break;
                case Citron.IR0.AssignExp @assignExp: Write_AssignExp(@assignExp); break;
                case Citron.IR0.BoolLiteralExp @boolLiteralExp: Write_BoolLiteralExp(@boolLiteralExp); break;
                case Citron.IR0.IntLiteralExp @intLiteralExp: Write_IntLiteralExp(@intLiteralExp); break;
                case Citron.IR0.StringExp @stringExp: Write_StringExp(@stringExp); break;
                case Citron.IR0.ListExp @listExp: Write_ListExp(@listExp); break;
                case Citron.IR0.ListIteratorExp @listIteratorExp: Write_ListIteratorExp(@listIteratorExp); break;
                case Citron.IR0.CallInternalUnaryOperatorExp @callInternalUnaryOperatorExp: Write_CallInternalUnaryOperatorExp(@callInternalUnaryOperatorExp); break;
                case Citron.IR0.CallInternalUnaryAssignOperatorExp @callInternalUnaryAssignOperatorExp: Write_CallInternalUnaryAssignOperatorExp(@callInternalUnaryAssignOperatorExp); break;
                case Citron.IR0.CallInternalBinaryOperatorExp @callInternalBinaryOperatorExp: Write_CallInternalBinaryOperatorExp(@callInternalBinaryOperatorExp); break;
                case Citron.IR0.CallGlobalFuncExp @callGlobalFuncExp: Write_CallGlobalFuncExp(@callGlobalFuncExp); break;
                case Citron.IR0.NewClassExp @newClassExp: Write_NewClassExp(@newClassExp); break;
                case Citron.IR0.CallClassMemberFuncExp @callClassMemberFuncExp: Write_CallClassMemberFuncExp(@callClassMemberFuncExp); break;
                case Citron.IR0.CastClassExp @castClassExp: Write_CastClassExp(@castClassExp); break;
                case Citron.IR0.NewStructExp @newStructExp: Write_NewStructExp(@newStructExp); break;
                case Citron.IR0.CallStructMemberFuncExp @callStructMemberFuncExp: Write_CallStructMemberFuncExp(@callStructMemberFuncExp); break;
                case Citron.IR0.NewEnumElemExp @newEnumElemExp: Write_NewEnumElemExp(@newEnumElemExp); break;
                case Citron.IR0.CastEnumElemToEnumExp @castEnumElemToEnumExp: Write_CastEnumElemToEnumExp(@castEnumElemToEnumExp); break;
                case Citron.IR0.NewNullableExp @newNullableExp: Write_NewNullableExp(@newNullableExp); break;
                case Citron.IR0.LambdaExp @lambdaExp: Write_LambdaExp(@lambdaExp); break;
                case Citron.IR0.CallValueExp @callValueExp: Write_CallValueExp(@callValueExp); break;
                case Citron.IR0.CallFuncRefExp @callFuncRefExp: Write_CallFuncRefExp(@callFuncRefExp); break;
                case Citron.IR0.CastBoxLambdaToFuncRefExp @castBoxLambdaToFuncRefExp: Write_CastBoxLambdaToFuncRefExp(@castBoxLambdaToFuncRefExp); break;
                case Citron.IR0.BoxExp @boxExp: Write_BoxExp(@boxExp); break;
                case Citron.IR0.LocalRefExp @localRefExp: Write_LocalRefExp(@localRefExp); break;
                case Citron.IR0.BoxRefExp @boxRefExp: Write_BoxRefExp(@boxRefExp); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_LoadExp(Citron.IR0.LoadExp? @loadExp)
        {
            if (@loadExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.LoadExp(");
            writer1.Write_Loc(@loadExp.Loc);
            itw1.WriteLine(",");
            writer1.Write_IType(@loadExp.Type);
            itw.Write(")");
        }

        public void Write_AssignExp(Citron.IR0.AssignExp? @assignExp)
        {
            if (@assignExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.AssignExp(");
            writer1.Write_Loc(@assignExp.Dest);
            itw1.WriteLine(",");
            writer1.Write_Exp(@assignExp.Src);
            itw.Write(")");
        }

        public void Write_BoolLiteralExp(Citron.IR0.BoolLiteralExp? @boolLiteralExp)
        {
            if (@boolLiteralExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.BoolLiteralExp(");
            writer1.Write_Boolean(@boolLiteralExp.Value);
            itw1.WriteLine(",");
            writer1.Write_IType(@boolLiteralExp.BoolType);
            itw.Write(")");
        }

        public void Write_IntLiteralExp(Citron.IR0.IntLiteralExp? @intLiteralExp)
        {
            if (@intLiteralExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.IntLiteralExp(");
            writer1.Write_Int32(@intLiteralExp.Value);
            itw1.WriteLine(",");
            writer1.Write_IType(@intLiteralExp.IntType);
            itw.Write(")");
        }

        public void Write_StringExp(Citron.IR0.StringExp? @stringExp)
        {
            if (@stringExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.StringExp(");
            writer1.Write_ImmutableArray(Write_StringExpElement, "Citron.IR0.StringExpElement", @stringExp.Elements);
            itw1.WriteLine(",");
            writer1.Write_IType(@stringExp.StringType);
            itw.Write(")");
        }

        public void Write_ListExp(Citron.IR0.ListExp? @listExp)
        {
            if (@listExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.ListExp(");
            writer1.Write_ImmutableArray(Write_Exp, "Citron.IR0.Exp", @listExp.Elems);
            itw1.WriteLine(",");
            writer1.Write_IType(@listExp.ListType);
            itw.Write(")");
        }

        public void Write_ListIteratorExp(Citron.IR0.ListIteratorExp? @listIteratorExp)
        {
            if (@listIteratorExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.ListIteratorExp(");
            writer1.Write_Loc(@listIteratorExp.ListLoc);
            itw1.WriteLine(",");
            writer1.Write_IType(@listIteratorExp.IteratorType);
            itw.Write(")");
        }

        public void Write_CallInternalUnaryOperatorExp(Citron.IR0.CallInternalUnaryOperatorExp? @callInternalUnaryOperatorExp)
        {
            if (@callInternalUnaryOperatorExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallInternalUnaryOperatorExp(");
            writer1.Write_InternalUnaryOperator(@callInternalUnaryOperatorExp.Operator);
            itw1.WriteLine(",");
            writer1.Write_Exp(@callInternalUnaryOperatorExp.Operand);
            itw1.WriteLine(",");
            writer1.Write_IType(@callInternalUnaryOperatorExp.Type);
            itw.Write(")");
        }

        public void Write_CallInternalUnaryAssignOperatorExp(Citron.IR0.CallInternalUnaryAssignOperatorExp? @callInternalUnaryAssignOperatorExp)
        {
            if (@callInternalUnaryAssignOperatorExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallInternalUnaryAssignOperatorExp(");
            writer1.Write_InternalUnaryAssignOperator(@callInternalUnaryAssignOperatorExp.Operator);
            itw1.WriteLine(",");
            writer1.Write_Loc(@callInternalUnaryAssignOperatorExp.Operand);
            itw1.WriteLine(",");
            writer1.Write_IType(@callInternalUnaryAssignOperatorExp.Type);
            itw.Write(")");
        }

        public void Write_CallInternalBinaryOperatorExp(Citron.IR0.CallInternalBinaryOperatorExp? @callInternalBinaryOperatorExp)
        {
            if (@callInternalBinaryOperatorExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallInternalBinaryOperatorExp(");
            writer1.Write_InternalBinaryOperator(@callInternalBinaryOperatorExp.Operator);
            itw1.WriteLine(",");
            writer1.Write_Exp(@callInternalBinaryOperatorExp.Operand0);
            itw1.WriteLine(",");
            writer1.Write_Exp(@callInternalBinaryOperatorExp.Operand1);
            itw1.WriteLine(",");
            writer1.Write_IType(@callInternalBinaryOperatorExp.Type);
            itw.Write(")");
        }

        public void Write_CallGlobalFuncExp(Citron.IR0.CallGlobalFuncExp? @callGlobalFuncExp)
        {
            if (@callGlobalFuncExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallGlobalFuncExp(");
            writer1.Write_ISymbolNode(@callGlobalFuncExp.Func);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @callGlobalFuncExp.Args);
            itw.Write(")");
        }

        public void Write_NewClassExp(Citron.IR0.NewClassExp? @newClassExp)
        {
            if (@newClassExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.NewClassExp(");
            writer1.Write_ISymbolNode(@newClassExp.Constructor);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @newClassExp.Args);
            itw.Write(")");
        }

        public void Write_CallClassMemberFuncExp(Citron.IR0.CallClassMemberFuncExp? @callClassMemberFuncExp)
        {
            if (@callClassMemberFuncExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallClassMemberFuncExp(");
            writer1.Write_ISymbolNode(@callClassMemberFuncExp.ClassMemberFunc);
            itw1.WriteLine(",");
            writer1.Write_Loc(@callClassMemberFuncExp.Instance);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @callClassMemberFuncExp.Args);
            itw.Write(")");
        }

        public void Write_CastClassExp(Citron.IR0.CastClassExp? @castClassExp)
        {
            if (@castClassExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CastClassExp(");
            writer1.Write_Exp(@castClassExp.Src);
            itw1.WriteLine(",");
            writer1.Write_ISymbolNode(@castClassExp.Class);
            itw.Write(")");
        }

        public void Write_NewStructExp(Citron.IR0.NewStructExp? @newStructExp)
        {
            if (@newStructExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.NewStructExp(");
            writer1.Write_ISymbolNode(@newStructExp.Constructor);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @newStructExp.Args);
            itw.Write(")");
        }

        public void Write_CallStructMemberFuncExp(Citron.IR0.CallStructMemberFuncExp? @callStructMemberFuncExp)
        {
            if (@callStructMemberFuncExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallStructMemberFuncExp(");
            writer1.Write_ISymbolNode(@callStructMemberFuncExp.StructMemberFunc);
            itw1.WriteLine(",");
            writer1.Write_Loc(@callStructMemberFuncExp.Instance);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @callStructMemberFuncExp.Args);
            itw.Write(")");
        }

        public void Write_NewEnumElemExp(Citron.IR0.NewEnumElemExp? @newEnumElemExp)
        {
            if (@newEnumElemExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.NewEnumElemExp(");
            writer1.Write_ISymbolNode(@newEnumElemExp.EnumElem);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @newEnumElemExp.Args);
            itw.Write(")");
        }

        public void Write_CastEnumElemToEnumExp(Citron.IR0.CastEnumElemToEnumExp? @castEnumElemToEnumExp)
        {
            if (@castEnumElemToEnumExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CastEnumElemToEnumExp(");
            writer1.Write_Exp(@castEnumElemToEnumExp.Src);
            itw1.WriteLine(",");
            writer1.Write_ISymbolNode(@castEnumElemToEnumExp.EnumElem);
            itw.Write(")");
        }

        public void Write_NewNullableExp(Citron.IR0.NewNullableExp? @newNullableExp)
        {
            if (@newNullableExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.NewNullableExp(");
            writer1.Write_Exp(@newNullableExp.ValueExp);
            itw1.WriteLine(",");
            writer1.Write_IType(@newNullableExp.Type);
            itw.Write(")");
        }

        public void Write_LambdaExp(Citron.IR0.LambdaExp? @lambdaExp)
        {
            if (@lambdaExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.LambdaExp(");
            writer1.Write_ISymbolNode(@lambdaExp.Lambda);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @lambdaExp.Args);
            itw.Write(")");
        }

        public void Write_CallValueExp(Citron.IR0.CallValueExp? @callValueExp)
        {
            if (@callValueExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallValueExp(");
            writer1.Write_ISymbolNode(@callValueExp.Lambda);
            itw1.WriteLine(",");
            writer1.Write_Loc(@callValueExp.Callable);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @callValueExp.Args);
            itw.Write(")");
        }

        public void Write_CallFuncRefExp(Citron.IR0.CallFuncRefExp? @callFuncRefExp)
        {
            if (@callFuncRefExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallFuncRefExp(");
            writer1.Write_Loc(@callFuncRefExp.Callable);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @callFuncRefExp.Args);
            itw1.WriteLine(",");
            writer1.Write_FuncType(@callFuncRefExp.FuncType);
            itw.Write(")");
        }

        public void Write_CastBoxLambdaToFuncRefExp(Citron.IR0.CastBoxLambdaToFuncRefExp? @castBoxLambdaToFuncRefExp)
        {
            if (@castBoxLambdaToFuncRefExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CastBoxLambdaToFuncRefExp(");
            writer1.Write_Exp(@castBoxLambdaToFuncRefExp.BoxLambdaSrc);
            itw1.WriteLine(",");
            writer1.Write_FuncType(@castBoxLambdaToFuncRefExp.FuncType);
            itw.Write(")");
        }

        public void Write_BoxExp(Citron.IR0.BoxExp? @boxExp)
        {
            if (@boxExp == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.BoxExp(");
            this.Write_Exp(@boxExp.Exp);
            itw.Write(")");
        }

        public void Write_LocalRefExp(Citron.IR0.LocalRefExp? @localRefExp)
        {
            if (@localRefExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.LocalRefExp(");
            writer1.Write_Loc(@localRefExp.InnerLoc);
            itw1.WriteLine(",");
            writer1.Write_IType(@localRefExp.InnerLocType);
            itw.Write(")");
        }

        public void Write_BoxRefExp(Citron.IR0.BoxRefExp? @boxRefExp)
        {
            if (@boxRefExp == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.BoxRefExp(");
            writer1.Write_Loc(@boxRefExp.Loc);
            itw1.WriteLine(",");
            writer1.Write_IType(@boxRefExp.LocType);
            itw.Write(")");
        }

        public void Write_Stmt(Citron.IR0.Stmt? @stmt)
        {
            if (@stmt == null) { itw.Write("null"); return; }

            switch(@stmt)
            {
                case Citron.IR0.CommandStmt @commandStmt: Write_CommandStmt(@commandStmt); break;
                case Citron.IR0.LocalVarDeclStmt @localVarDeclStmt: Write_LocalVarDeclStmt(@localVarDeclStmt); break;
                case Citron.IR0.IfStmt @ifStmt: Write_IfStmt(@ifStmt); break;
                case Citron.IR0.IfTestClassStmt @ifTestClassStmt: Write_IfTestClassStmt(@ifTestClassStmt); break;
                case Citron.IR0.IfTestEnumElemStmt @ifTestEnumElemStmt: Write_IfTestEnumElemStmt(@ifTestEnumElemStmt); break;
                case Citron.IR0.ForStmt @forStmt: Write_ForStmt(@forStmt); break;
                case Citron.IR0.ContinueStmt @continueStmt: Write_ContinueStmt(@continueStmt); break;
                case Citron.IR0.BreakStmt @breakStmt: Write_BreakStmt(@breakStmt); break;
                case Citron.IR0.ReturnStmt @returnStmt: Write_ReturnStmt(@returnStmt); break;
                case Citron.IR0.BlockStmt @blockStmt: Write_BlockStmt(@blockStmt); break;
                case Citron.IR0.BlankStmt @blankStmt: Write_BlankStmt(@blankStmt); break;
                case Citron.IR0.ExpStmt @expStmt: Write_ExpStmt(@expStmt); break;
                case Citron.IR0.TaskStmt @taskStmt: Write_TaskStmt(@taskStmt); break;
                case Citron.IR0.AwaitStmt @awaitStmt: Write_AwaitStmt(@awaitStmt); break;
                case Citron.IR0.AsyncStmt @asyncStmt: Write_AsyncStmt(@asyncStmt); break;
                case Citron.IR0.ForeachStmt @foreachStmt: Write_ForeachStmt(@foreachStmt); break;
                case Citron.IR0.YieldStmt @yieldStmt: Write_YieldStmt(@yieldStmt); break;
                case Citron.IR0.CallClassConstructorStmt @callClassConstructorStmt: Write_CallClassConstructorStmt(@callClassConstructorStmt); break;
                case Citron.IR0.CallStructConstructorStmt @callStructConstructorStmt: Write_CallStructConstructorStmt(@callStructConstructorStmt); break;
                case Citron.IR0.DirectiveStmt @directiveStmt: Write_DirectiveStmt(@directiveStmt); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_CommandStmt(Citron.IR0.CommandStmt? @commandStmt)
        {
            if (@commandStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.CommandStmt(");
            this.Write_ImmutableArray(Write_StringExp, "Citron.IR0.StringExp", @commandStmt.Commands);
            itw.Write(")");
        }

        public void Write_LocalVarDeclStmt(Citron.IR0.LocalVarDeclStmt? @localVarDeclStmt)
        {
            if (@localVarDeclStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.LocalVarDeclStmt(");
            writer1.Write_IType(@localVarDeclStmt.Type);
            itw1.WriteLine(",");
            writer1.Write_String(@localVarDeclStmt.Name);
            itw1.WriteLine(",");
            writer1.Write_Exp(@localVarDeclStmt.InitExp);
            itw.Write(")");
        }

        public void Write_IfStmt(Citron.IR0.IfStmt? @ifStmt)
        {
            if (@ifStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.IfStmt(");
            writer1.Write_Exp(@ifStmt.Cond);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @ifStmt.Body);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @ifStmt.ElseBody);
            itw.Write(")");
        }

        public void Write_IfTestClassStmt(Citron.IR0.IfTestClassStmt? @ifTestClassStmt)
        {
            if (@ifTestClassStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.IfTestClassStmt(");
            writer1.Write_Loc(@ifTestClassStmt.Target);
            itw1.WriteLine(",");
            writer1.Write_ISymbolNode(@ifTestClassStmt.Class);
            itw1.WriteLine(",");
            writer1.Write_Name(@ifTestClassStmt.VarName);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @ifTestClassStmt.Body);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @ifTestClassStmt.ElseBody);
            itw.Write(")");
        }

        public void Write_IfTestEnumElemStmt(Citron.IR0.IfTestEnumElemStmt? @ifTestEnumElemStmt)
        {
            if (@ifTestEnumElemStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.IfTestEnumElemStmt(");
            writer1.Write_Loc(@ifTestEnumElemStmt.Target);
            itw1.WriteLine(",");
            writer1.Write_ISymbolNode(@ifTestEnumElemStmt.EnumElem);
            itw1.WriteLine(",");
            writer1.Write_String(@ifTestEnumElemStmt.VarName);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @ifTestEnumElemStmt.Body);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @ifTestEnumElemStmt.ElseBody);
            itw.Write(")");
        }

        public void Write_ForStmt(Citron.IR0.ForStmt? @forStmt)
        {
            if (@forStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.ForStmt(");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @forStmt.InitStmts);
            itw1.WriteLine(",");
            writer1.Write_Exp(@forStmt.CondExp);
            itw1.WriteLine(",");
            writer1.Write_Exp(@forStmt.ContinueExp);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @forStmt.Body);
            itw.Write(")");
        }

        public void Write_ContinueStmt(Citron.IR0.ContinueStmt? @continueStmt)
        {
            if (@continueStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.ContinueStmt();");
        }

        public void Write_BreakStmt(Citron.IR0.BreakStmt? @breakStmt)
        {
            if (@breakStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.BreakStmt();");
        }

        public void Write_ReturnStmt(Citron.IR0.ReturnStmt? @returnStmt)
        {
            if (@returnStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.ReturnStmt(");
            this.Write_ReturnInfo(@returnStmt.Info);
            itw.Write(")");
        }

        public void Write_BlockStmt(Citron.IR0.BlockStmt? @blockStmt)
        {
            if (@blockStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.BlockStmt(");
            this.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @blockStmt.Stmts);
            itw.Write(")");
        }

        public void Write_BlankStmt(Citron.IR0.BlankStmt? @blankStmt)
        {
            if (@blankStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.BlankStmt();");
        }

        public void Write_ExpStmt(Citron.IR0.ExpStmt? @expStmt)
        {
            if (@expStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.ExpStmt(");
            this.Write_Exp(@expStmt.Exp);
            itw.Write(")");
        }

        public void Write_TaskStmt(Citron.IR0.TaskStmt? @taskStmt)
        {
            if (@taskStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.TaskStmt(");
            writer1.Write_ISymbolNode(@taskStmt.Lambda);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @taskStmt.CaptureArgs);
            itw.Write(")");
        }

        public void Write_AwaitStmt(Citron.IR0.AwaitStmt? @awaitStmt)
        {
            if (@awaitStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.AwaitStmt(");
            this.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @awaitStmt.Body);
            itw.Write(")");
        }

        public void Write_AsyncStmt(Citron.IR0.AsyncStmt? @asyncStmt)
        {
            if (@asyncStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.AsyncStmt(");
            writer1.Write_ISymbolNode(@asyncStmt.Lambda);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @asyncStmt.CaptureArgs);
            itw.Write(")");
        }

        public void Write_ForeachStmt(Citron.IR0.ForeachStmt? @foreachStmt)
        {
            if (@foreachStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.ForeachStmt(");
            writer1.Write_IType(@foreachStmt.ItemType);
            itw1.WriteLine(",");
            writer1.Write_String(@foreachStmt.ElemName);
            itw1.WriteLine(",");
            writer1.Write_Loc(@foreachStmt.Iterator);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Stmt, "Citron.IR0.Stmt", @foreachStmt.Body);
            itw.Write(")");
        }

        public void Write_YieldStmt(Citron.IR0.YieldStmt? @yieldStmt)
        {
            if (@yieldStmt == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.YieldStmt(");
            this.Write_Exp(@yieldStmt.Value);
            itw.Write(")");
        }

        public void Write_CallClassConstructorStmt(Citron.IR0.CallClassConstructorStmt? @callClassConstructorStmt)
        {
            if (@callClassConstructorStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallClassConstructorStmt(");
            writer1.Write_ISymbolNode(@callClassConstructorStmt.Constructor);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @callClassConstructorStmt.Args);
            itw.Write(")");
        }

        public void Write_CallStructConstructorStmt(Citron.IR0.CallStructConstructorStmt? @callStructConstructorStmt)
        {
            if (@callStructConstructorStmt == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.IR0.CallStructConstructorStmt(");
            writer1.Write_ISymbolNode(@callStructConstructorStmt.Constructor);
            itw1.WriteLine(",");
            writer1.Write_ImmutableArray(Write_Argument, "Citron.IR0.Argument", @callStructConstructorStmt.Args);
            itw.Write(")");
        }

        public void Write_DirectiveStmt(Citron.IR0.DirectiveStmt? @directiveStmt)
        {
            if (@directiveStmt == null) { itw.Write("null"); return; }

            switch(@directiveStmt)
            {
                case Citron.IR0.DirectiveStmt.Null @null: Write_DirectiveStmt_Null(@null); break;
                case Citron.IR0.DirectiveStmt.NotNull @notNull: Write_DirectiveStmt_NotNull(@notNull); break;
                case Citron.IR0.DirectiveStmt.StaticNull @staticNull: Write_DirectiveStmt_StaticNull(@staticNull); break;
                case Citron.IR0.DirectiveStmt.StaticNotNull @staticNotNull: Write_DirectiveStmt_StaticNotNull(@staticNotNull); break;
                case Citron.IR0.DirectiveStmt.StaticUnknownNull @staticUnknownNull: Write_DirectiveStmt_StaticUnknownNull(@staticUnknownNull); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_DirectiveStmt_Null(Citron.IR0.DirectiveStmt.Null? @null)
        {
            if (@null == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.DirectiveStmt.Null(");
            this.Write_Loc(@null.Loc);
            itw.Write(")");
        }

        public void Write_DirectiveStmt_NotNull(Citron.IR0.DirectiveStmt.NotNull? @notNull)
        {
            if (@notNull == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.DirectiveStmt.NotNull(");
            this.Write_Loc(@notNull.Loc);
            itw.Write(")");
        }

        public void Write_DirectiveStmt_StaticNull(Citron.IR0.DirectiveStmt.StaticNull? @staticNull)
        {
            if (@staticNull == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.DirectiveStmt.StaticNull(");
            this.Write_Loc(@staticNull.Loc);
            itw.Write(")");
        }

        public void Write_DirectiveStmt_StaticNotNull(Citron.IR0.DirectiveStmt.StaticNotNull? @staticNotNull)
        {
            if (@staticNotNull == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.DirectiveStmt.StaticNotNull(");
            this.Write_Loc(@staticNotNull.Loc);
            itw.Write(")");
        }

        public void Write_DirectiveStmt_StaticUnknownNull(Citron.IR0.DirectiveStmt.StaticUnknownNull? @staticUnknownNull)
        {
            if (@staticUnknownNull == null) { itw.Write("null"); return; }

            itw.Write("new Citron.IR0.DirectiveStmt.StaticUnknownNull(");
            this.Write_Loc(@staticUnknownNull.Loc);
            itw.Write(")");
        }

        public void Write_IType(Citron.Symbol.IType? @iType)
        {
            if (@iType == null) { itw.Write("null"); return; }

            switch(@iType)
            {
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_TypeImpl(Citron.Symbol.TypeImpl? @typeImpl)
        {
            if (@typeImpl == null) { itw.Write("null"); return; }

            switch(@typeImpl)
            {
                case Citron.Symbol.SymbolType @symbolType: Write_SymbolType(@symbolType); break;
                case Citron.Symbol.NullableType @nullableType: Write_NullableType(@nullableType); break;
                case Citron.Symbol.TypeVarType @typeVarType: Write_TypeVarType(@typeVarType); break;
                case Citron.Symbol.VoidType @voidType: Write_VoidType(@voidType); break;
                case Citron.Symbol.TupleType @tupleType: Write_TupleType(@tupleType); break;
                case Citron.Symbol.FuncType @funcType: Write_FuncType(@funcType); break;
                case Citron.Symbol.VarType @varType: Write_VarType(@varType); break;
                case Citron.Symbol.BoxRefType @boxRefType: Write_BoxRefType(@boxRefType); break;
                case Citron.Symbol.LocalRefType @localRefType: Write_LocalRefType(@localRefType); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_SymbolType(Citron.Symbol.SymbolType? @symbolType)
        {
            if (@symbolType == null) { itw.Write("null"); return; }

            switch(@symbolType)
            {
                case Citron.Symbol.EnumElemType @enumElemType: Write_EnumElemType(@enumElemType); break;
                case Citron.Symbol.EnumType @enumType: Write_EnumType(@enumType); break;
                case Citron.Symbol.ClassType @classType: Write_ClassType(@classType); break;
                case Citron.Symbol.InterfaceType @interfaceType: Write_InterfaceType(@interfaceType); break;
                case Citron.Symbol.StructType @structType: Write_StructType(@structType); break;
                case Citron.Symbol.LambdaType @lambdaType: Write_LambdaType(@lambdaType); break;
                default: throw new Citron.Infra.UnreachableCodeException();
            }
        }

        public void Write_EnumElemType(Citron.Symbol.EnumElemType? @enumElemType)
        {
            if (@enumElemType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.EnumElemType(");
            this.Write_ISymbolNode(@enumElemType.Symbol);
            itw.Write(")");
        }

        public void Write_EnumType(Citron.Symbol.EnumType? @enumType)
        {
            if (@enumType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.EnumType(");
            this.Write_ISymbolNode(@enumType.Symbol);
            itw.Write(")");
        }

        public void Write_ClassType(Citron.Symbol.ClassType? @classType)
        {
            if (@classType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.ClassType(");
            this.Write_ISymbolNode(@classType.Symbol);
            itw.Write(")");
        }

        public void Write_InterfaceType(Citron.Symbol.InterfaceType? @interfaceType)
        {
            if (@interfaceType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.InterfaceType(");
            this.Write_ISymbolNode(@interfaceType.Symbol);
            itw.Write(")");
        }

        public void Write_StructType(Citron.Symbol.StructType? @structType)
        {
            if (@structType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.StructType(");
            this.Write_ISymbolNode(@structType.Symbol);
            itw.Write(")");
        }

        public void Write_LambdaType(Citron.Symbol.LambdaType? @lambdaType)
        {
            if (@lambdaType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.LambdaType(");
            this.Write_ISymbolNode(@lambdaType.Symbol);
            itw.Write(")");
        }

        public void Write_NullableType(Citron.Symbol.NullableType? @nullableType)
        {
            if (@nullableType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.NullableType(");
            this.Write_IType(@nullableType.InnerType);
            itw.Write(")");
        }

        public void Write_TypeVarType(Citron.Symbol.TypeVarType? @typeVarType)
        {
            if (@typeVarType == null) { itw.Write("null"); return; }

            var itw1 = itw.Push();
            var writer1 = new IR0Writer(itw1);
            itw1.WriteLine();

            itw.Write("new Citron.Symbol.TypeVarType(");
            writer1.Write_Int32(@typeVarType.Index);
            itw1.WriteLine(",");
            writer1.Write_Name(@typeVarType.Name);
            itw.Write(")");
        }

        public void Write_VoidType(Citron.Symbol.VoidType? @voidType)
        {
            if (@voidType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.VoidType();");
        }

        public void Write_TupleType(Citron.Symbol.TupleType? @tupleType)
        {
            if (@tupleType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.TupleType();");
        }

        public void Write_FuncType(Citron.Symbol.FuncType? @funcType)
        {
            if (@funcType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.FuncType();");
        }

        public void Write_VarType(Citron.Symbol.VarType? @varType)
        {
            if (@varType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.VarType();");
        }

        public void Write_BoxRefType(Citron.Symbol.BoxRefType? @boxRefType)
        {
            if (@boxRefType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.BoxRefType();");
        }

        public void Write_LocalRefType(Citron.Symbol.LocalRefType? @localRefType)
        {
            if (@localRefType == null) { itw.Write("null"); return; }

            itw.Write("new Citron.Symbol.LocalRefType();");
        }
    }
}
