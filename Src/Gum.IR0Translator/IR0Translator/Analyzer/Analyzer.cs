using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using M = Gum.CompileTime;
using Gum.Infra;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    // Root Analyzer
    partial class Analyzer
    {
        public static R.Script Analyze(
            S.Script script,
            R.ModuleName moduleName,
            ItemValueFactory itemValueFactory,
            GlobalItemValueFactory globalItemValueFactory,
            TypeExpInfoService typeExpTypeValueService,
            IErrorCollector errorCollector)
        {
            var globalContext = new GlobalContext(itemValueFactory, globalItemValueFactory, typeExpTypeValueService, errorCollector);            
            var rootContext = new RootContext(moduleName, itemValueFactory);            
            
            var rscript = RootAnalyzer.Analyze(globalContext, rootContext, script);
            
            return rscript;
        }        
    }
}
