using System;
using Citron.IR0Visitor;

using R = Citron.IR0;

namespace Citron.IR0Analyzer.NullRefAnalysis
{
    struct LocAnalyzer : IIR0LocVisitorWithRet<AbstractValue>
    {
        GlobalContext globalContext;
        LocalContext context;

        public static AbstractValue Analyze(R.Loc loc, GlobalContext globalContext, LocalContext context)
        {
            var analyzer = new LocAnalyzer(globalContext, context);
            return analyzer.Visit(loc);
        }

        public LocAnalyzer(GlobalContext globalContext, LocalContext context)
        {
            this.globalContext = globalContext;
            this.context = context;
        }

        AbstractValue Visit(R.Loc loc)
        {
            return this.Visit<LocAnalyzer, AbstractValue>(loc);
        }

        public AbstractValue VisitCapturedVarLoc(R.LambdaMemberVar loc)
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

        // Value를 리턴한다는건 일회용이 아닐수 있다는 뜻이므로, 없으면 할당해서 줘야 한다
        public AbstractValue VisitStructMemberLoc(R.StructMemberLoc loc)
        {
            // TODO: member의 타입을 봐야 한다
            // nullable이 아니면 그냥 값
            // s.x는
            var structLoc = this.Visit(loc.Instance);
            return structLoc.GetMemberValue(loc.MemberPath);
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