﻿using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Phase3
    {
        struct ExpResult
        {
            AnalyzeExpResult analyzeResult;

            public Exp Exp { get => analyzeResult.Exp; }
            public TypeValue TypeValue { get => analyzeResult.TypeValue; }
                    
            // public AnalyzeExpResult Analyze { get; } // 후에 TypeChecker, TypeResolver, ...
            public ExpResult(AnalyzeExpResult analyzeResult)
            {
                this.analyzeResult = analyzeResult;
            }
        }
        
        ExpResult VisitStringExp(S.StringExp exp)
        {
            var ir0StrExpElems = new List<StringExpElement>();
            foreach (var elem in exp.Elements)
            {
                var ir0StrExpElem = VisitStringExpElement(elem);
                ir0StrExpElems.Add(ir0StrExpElem);
            }

            return analyzer.AnalyzeStringExp(ir0StrExpElems);
        }

        ExpResult? VisitStringExpElement(S.StringExpElement elem)
        {
            throw new NotImplementedException();
        }
        
        ExpResult VisitExp(S.Exp exp, TypeValue? hintType)
        {
            throw new NotImplementedException();
        }
    }
}
