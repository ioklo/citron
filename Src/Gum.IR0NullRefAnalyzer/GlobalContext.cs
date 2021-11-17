using Gum.Collections;
using Gum.Infra;
using Gum.IR0;
using Gum.Log;
using System;
using System.Collections.Generic;

namespace Gum.IR0Analyzer.NullRefAnalysis
{
    class GlobalContext
    {
        ILogger logger;
        ImmutableDictionary<string, AbstractValue> globalVariables;

        public GlobalContext(ILogger logger)
        {
            this.logger = logger;
            this.globalVariables = ImmutableDictionary<string, AbstractValue>.Empty;
        }

        GlobalContext(GlobalContext other)
        {
            this.logger = other.logger;
            this.globalVariables = other.globalVariables;
        }

        public GlobalContext Clone()
        {
            return new GlobalContext(this);
        }

        public void Merge(GlobalContext other)
        {
            throw new NotImplementedException();
        }

        public void AddGlobalVariable(string name, AbstractValue value)
        {
            globalVariables = globalVariables.Add(name, value);
        }

        public bool IsNullableType(Path.Nested memberPath)
        {
            throw new NotImplementedException();
        }

        // TODO: IR0 노드에 대해서 소스코드와 연결을 해 주어야 한다
        public void AddFatalError(ErrorLog errorLog) // throws FatalException
        {
            logger.Add(errorLog);
            throw new FatalException();
        }
    }
}