// 네임스페이스 이름은 모듈과 상관없다
using System;

namespace Gum.IR0Analyzer.NullRefAnalysis
{
    class LocalContext
    {
        LocalContext? parentContext;

        public LocalContext(LocalContext? parentContext)
        {
            this.parentContext = parentContext;
        }

        public LocalContext Clone()
        {
            throw new NotImplementedException();
        }

        public void Merge(LocalContext other)
        {
            throw new NotImplementedException();
        }

        public AbstractValue GetGlobalValue(string name)
        {
            throw new NotImplementedException();
        }
    }
}
