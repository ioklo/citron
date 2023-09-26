using ISyntaxNode = Citron.Syntax.ISyntaxNode;
using Citron.IR0;
using Citron.Symbol;
using Citron.Infra;

namespace Citron.Analysis;

struct IntermediateExpIntermediateRefExpTranslator : IIntermediateExpVisitor<IntermediateRefExp?>
{   
    public static IntermediateRefExp? Translate(IntermediateExp exp)
    {
        var translator = new IntermediateExpIntermediateRefExpTranslator();
        return exp.Accept<IntermediateExpIntermediateRefExpTranslator, IntermediateRefExp?>(ref translator);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitNamespace(IntermediateExp.Namespace exp)
    {
        return new IntermediateRefExp.Namespace(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitGlobalFuncs(IntermediateExp.GlobalFuncs exp)
    {
        return null;
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitTypeVar(IntermediateExp.TypeVar exp)
    {
        return new IntermediateRefExp.TypeVar(exp.Type);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitClass(IntermediateExp.Class exp)
    {
        return new IntermediateRefExp.Class(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs exp)
    {
        return null;
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitStruct(IntermediateExp.Struct exp)
    {
        return new IntermediateRefExp.Struct(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs exp)
    {
        return null;
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitEnum(IntermediateExp.Enum exp)
    {
        return new IntermediateRefExp.Enum(exp.Symbol);
    }

    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitEnumElem(IntermediateExp.EnumElem exp)
    {
        return null;
    }

    // &this   -> invalid
    // &this.a -> valid, box ptr
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitThis(IntermediateExp.ThisVar exp)
    {
        return new IntermediateRefExp.ThisVar(exp.Type);
    }

    // &id
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitLocalVar(IntermediateExp.LocalVar exp)
    {
        return new IntermediateRefExp.LocalRef(new LocalVarLoc(exp.Name), exp.Type);
    }

    // &x
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar exp)
    {
        // TODO: [10] box lambda이면 box로 판단해야 한다 
        return new IntermediateRefExp.LocalRef(new LambdaMemberVarLoc(exp.Symbol), exp.Symbol.GetDeclType());
    }

    // x (C.x, this.x)
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitClassMemberVar(IntermediateExp.ClassMemberVar exp)
    {
        if (exp.Symbol.IsStatic()) // &C.x
        {
            return new IntermediateRefExp.StaticRef(new ClassMemberLoc(Instance: null, exp.Symbol), exp.Symbol.GetDeclType());
        }
        else // &this.x
        {
            return new IntermediateRefExp.BoxRef.ClassMember(new ThisLoc(), exp.Symbol);
        }
    }

    // x (S.x, this->x)
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitStructMemberVar(IntermediateExp.StructMemberVar exp)
    {
        if (exp.Symbol.IsStatic())
        {
            return new IntermediateRefExp.StaticRef(new StructMemberLoc(Instance: null, exp.Symbol), exp.Symbol.GetDeclType());
        }
        else
        {
            // this의 타입이 S*이다.
            // TODO: [10] box함수이면 this를 box로 판단해야 한다
            var derefThisLoc = new LocalDerefLoc(new ThisLoc());
            return new IntermediateRefExp.LocalRef(new StructMemberLoc(derefThisLoc, exp.Symbol), exp.Symbol.GetDeclType());
        }
    }

    // &x (E.First.x)    
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar exp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw new RuntimeFatalException();
    }
    
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitListIndexer(IntermediateExp.ListIndexer exp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw new RuntimeFatalException();
    }

    // *x
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitLocalDeref(IntermediateExp.LocalDeref exp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw new RuntimeFatalException();
    }

    // *x
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitBoxDeref(IntermediateExp.BoxDeref exp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw new RuntimeFatalException();
    }

    // 이것도 불가능한
    IntermediateRefExp? IIntermediateExpVisitor<IntermediateRefExp?>.VisitIR0Exp(IntermediateExp.IR0Exp exp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw new RuntimeFatalException();
    }
}

