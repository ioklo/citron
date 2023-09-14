using Citron.Collections;
using Citron.LexicalAnalysis;
using Citron.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Citron;

partial struct ExpParser
{	
	bool ParseLeftAssocBinaryOpExp(
		ParseBaseExpDelegate parseBaseExp,
		(Token Token, BinaryOpKind OpKind)[] infos,
		[NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseLeftAssocBinaryOpExp(parseBaseExp, infos, out outExp))
		{	
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	#region Single
	bool ParseSingleExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseSingleExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
	#endregion

	#region Primary, Postfix Inc/Dec       

	bool ParseArgument([NotNullWhen(returnValue: true)] out Argument? outArg)
	{
		var prevContext = context;

		if (!InternalParseArgument(out outArg))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	bool ParseCallArgs([NotNullWhen(returnValue: true)] out ImmutableArray<Argument>? outArgs)
	{
		var prevContext = context;

		if (!InternalParseCallArgs(out outArgs))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	bool ParsePrimaryExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParsePrimaryExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
	#endregion

	#region Unary, Prefix Inc/Dec	
	bool ParseUnaryExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseUnaryExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
	#endregion

	#region Multiplicative, LeftAssoc
	bool ParseMultiplicativeExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseMultiplicativeExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
	#endregion


	#region Additive, LeftAssoc
	bool ParseAdditiveExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseAdditiveExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
    #endregion

    #region Test, LeftAssoc
    bool ParseTestAndTypeTestExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseTestAndTypeTestExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
	#endregion

	#region Equality, Left Assoc
	bool ParseEqualityExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseEqualityExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
	#endregion

	#region Assignment, Right Assoc
	bool ParseAssignExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseAssignExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	#endregion

	#region LambdaExpression, Right Assoc
	bool ParseLambdaExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseLambdaExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
    #endregion

    bool ParseBoxExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        var prevContext = context;

        if (!InternalParseBoxExp(out outExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // redirection
    bool ParseExp([NotNullWhen(returnValue: true)] out Exp? outExp) => ParseAssignExp(out outExp);
	
	bool ParseNewExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseNewExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	bool ParseParenExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseParenExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	bool ParseNullLiteralExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseNullLiteralExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	bool ParseBoolLiteralExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseBoolLiteralExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	bool ParseIntLiteralExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseIntLiteralExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	// 스트링 파싱
	bool ParseStringExp([NotNullWhen(returnValue: true)] out StringExp? outStringExp)
	{
		var prevContext = context;

		if (!InternalParseStringExp(out outStringExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	bool ParseListExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseListExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}

	bool ParseIdentifierExp([NotNullWhen(returnValue: true)] out Exp? outExp)
	{
		var prevContext = context;

		if (!InternalParseIdentifierExp(out outExp))
		{
			context = prevContext;
			return false;
		}
		else
		{
			return true;
		}
	}
}