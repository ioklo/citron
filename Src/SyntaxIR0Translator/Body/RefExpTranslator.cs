using Citron.IR0;
using System.Diagnostics;

namespace Citron.Analysis;

// ExpResult를 RefExp로 바꿀 수 있는가
struct LocalRefExpTranslator : IExpResultVisitor<Exp>
{
    public static Exp Translate(ExpResult exp)
    {

    }

    // var& x = C;
    Exp IExpResultVisitor<Exp>.VisitClass(ExpResult.Class expResult)
    {
        throw new System.NotImplementedException();
    }

    // var& x = F;
    Exp IExpResultVisitor<Exp>.VisitClassMemberFuncs(ExpResult.ClassMemberFuncs expResult)
    {
        throw new System.NotImplementedException();
    }

    // var& x = c.x;
    // var& x = C.x;
    // var& x = x;
    Exp IExpResultVisitor<Exp>.VisitClassMemberVar(ExpResult.ClassMemberVar expResult)
    {
        if (expResult.Symbol.IsStatic())
        { 
            Debug.Assert(!expResult.HasExplicitInstance);
            return new LocalRefExp(new ClassMemberLoc(null, expResult.Symbol), expResult.Symbol.GetDeclType());
        }
        else
        {


        }
    }

    Exp IExpResultVisitor<Exp>.VisitEnum(ExpResult.Enum expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitEnumElem(ExpResult.EnumElem expResul, t )
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitEnumElemMemberVar(ExpResult.EnumElemMemberVar expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitError(ExpResult.Error expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitGlobalFuncs(ExpResult.GlobalFuncs expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitIR0Exp(ExpResult.IR0Exp expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitIR0Loc(ExpResult.IR0Loc expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitLambdaMemberVar(ExpResult.LambdaMemberVar expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitLocalVar(ExpResult.LocalVar expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitNamespace(ExpResult.Namespace expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitNotFound(ExpResult.NotFound expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitStruct(ExpResult.Struct expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitStructMemberFuncs(ExpResult.StructMemberFuncs expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitStructMemberVar(ExpResult.StructMemberVar expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitThis(ExpResult.ThisVar expResult)
    {
        throw new System.NotImplementedException();
    }

    Exp IExpResultVisitor<Exp>.VisitTypeVar(ExpResult.TypeVar expResult)
    {
        throw new System.NotImplementedException();
    }
}
