using Citron.Collections;
using Citron.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Citron;

public partial struct ScriptParser
{
    bool ParseFuncDeclParam([NotNullWhen(returnValue: true)] out FuncParam? outFuncParam)
    {
        var prevContext = context;

        if (!InternalParseFuncDeclParam(out outFuncParam))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseFuncDeclParams([NotNullWhen(returnValue: true)] out ImmutableArray<FuncParam>? outFuncParams)
    {
        var prevContext = context;

        if (!InternalParseFuncDeclParams(out outFuncParams))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseGlobalFuncDecl([NotNullWhen(returnValue: true)] out GlobalFuncDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseGlobalFuncDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseTypeParams([NotNullWhen(returnValue: true)] out ImmutableArray<TypeParam>? outTypeParams)
    {
        var prevContext = context;

        if (!InternalParseTypeParams(out outTypeParams))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseTypeDecl([NotNullWhen(returnValue: true)] out TypeDecl? outTypeDecl)
    {
        var prevContext = context;

        if (!InternalParseTypeDecl(out outTypeDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }
    
    bool ParseEnumDecl([NotNullWhen(returnValue: true)] out EnumDecl? outEnumDecl)
    {
        var prevContext = context;

        if (!InternalParseEnumDecl(out outEnumDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseAccessModifier([NotNullWhen(returnValue: true)] out AccessModifier? outAccessModifier)
    {
        var prevContext = context;

        if (!InternalParseAccessModifier(out outAccessModifier))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseStructMemberTypeDecl([NotNullWhen(returnValue: true)] out StructMemberTypeDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseStructMemberTypeDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseStructMemberVarDecl([NotNullWhen(returnValue: true)] out StructMemberVarDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseStructMemberVarDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseStructMemberFuncDecl([NotNullWhen(returnValue: true)] out StructMemberFuncDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseStructMemberFuncDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseStructConstructorDecl([NotNullWhen(returnValue: true)] out StructConstructorDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseStructConstructorDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseStructMemberDecl([NotNullWhen(returnValue: true)] out StructMemberDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseStructMemberDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseStructDecl([NotNullWhen(returnValue: true)] out StructDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseStructDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseClassMemberTypeDecl([NotNullWhen(returnValue: true)] out ClassMemberTypeDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseClassMemberTypeDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseClassMemberFuncDecl([NotNullWhen(returnValue: true)] out ClassMemberFuncDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseClassMemberFuncDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseClassConstructorDecl([NotNullWhen(returnValue: true)] out ClassConstructorDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseClassConstructorDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseClassMemberVarDecl([NotNullWhen(returnValue: true)] out ClassMemberVarDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseClassMemberVarDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseClassMemberDecl([NotNullWhen(returnValue: true)] out ClassMemberDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseClassMemberDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseClassDecl([NotNullWhen(returnValue: true)] out ClassDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseClassDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseNamespaceElement([NotNullWhen(returnValue: true)] out NamespaceElement? outElem)
    {
        var prevContext = context;

        if (!InternalParseNamespaceElement(out outElem))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseNamespaceDecl([NotNullWhen(returnValue: true)] out NamespaceDecl? outDecl)
    {
        var prevContext = context;

        if (!InternalParseNamespaceDecl(out outDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseScriptElement([NotNullWhen(returnValue: true)] out ScriptElement? outElem)
    {
        var prevContext = context;

        if (!InternalParseScriptElement(out outElem))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseScript([NotNullWhen(returnValue: true)] out Script? outScript)
    {
        var prevContext = context;

        if (!InternalParseScript(out outScript))
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