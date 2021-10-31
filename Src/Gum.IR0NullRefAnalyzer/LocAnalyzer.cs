using System;
using Gum.IR0Visitor;

using R = Gum.IR0;

namespace Gum.IR0Analyzer.NullRefAnalysis
{
    struct LocAnalyzer : IIR0LocVisitorWithRet<AbstractValue>
    {
        LocalContext context;

        public static AbstractValue Analyze(R.Loc loc, LocalContext context)
        {
            var analyzer = new LocAnalyzer(context);
            return analyzer.Visit<LocAnalyzer, AbstractValue>(loc);
        }

        public LocAnalyzer(LocalContext context)
        {
            this.context = context;
        }

        public AbstractValue VisitCapturedVarLoc(R.CapturedVarLoc loc)
        {
            throw new NotImplementedException();
        }
        
        // 클래스 멤버의 AbstractValue
        public AbstractValue VisitClassMemberLoc(R.ClassMemberLoc loc)
        {
            // var c = new C(); // { c -> { x -> undefined, y -> undefined } } 
            // c.x = 3;         // { c -> { x -> 3, y -> undefined } }
            
            // while()
            // {
            //    c.a = new C(); // { c -> { a -> { 
            // }

            // var x = ref s.x;
            // s.x = null;
            // x = ;            
            // ``unknown_null(s.x)

            // if (s.x is not null)
            //     s.x = 

            throw new NotImplementedException();
        }

        public AbstractValue VisitDerefExpLoc(R.DerefExpLoc loc)
        {
            // 일단 ref는 생각하지 않기로
            // 간단하게 하고 싶었는데 너무 복잡해지고 있다
            throw new NotImplementedException();
        }

        public AbstractValue VisitDerefLocLoc(R.DerefLocLoc loc)
        {
            throw new NotImplementedException();
        }

        public AbstractValue VisitEnumElemMemberLoc(R.EnumElemMemberLoc loc)
        {
            throw new NotImplementedException();
        }

        public AbstractValue VisitGlobalVarLoc(R.GlobalVarLoc loc)
        {
            return context.GetGlobalValue(loc.Name);
        }

        public AbstractValue VisitListIndexerLoc(R.ListIndexerLoc loc)
        {
            throw new NotImplementedException();
        }

        public AbstractValue VisitLocalVarLoc(R.LocalVarLoc loc)
        {
            throw new NotImplementedException();
        }

        public AbstractValue VisitStaticMemberLoc(R.StaticMemberLoc loc)
        {
            throw new NotImplementedException();
        }

        public AbstractValue VisitStructMemberLoc(R.StructMemberLoc loc)
        {
            throw new NotImplementedException();
        }

        public AbstractValue VisitTempLoc(R.TempLoc loc)
        {
            throw new NotImplementedException();
        }

        public AbstractValue VisitThisLoc(R.ThisLoc loc)
        {
            throw new NotImplementedException();
        }
    }
}