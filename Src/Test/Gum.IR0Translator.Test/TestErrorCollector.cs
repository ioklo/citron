using Gum.Collections;
using Gum.Infra;
using System.Collections.Generic;
using Xunit;

namespace Gum.IR0Translator.Test
{
    // 본체는 Mutable
    class TestErrorCollector : IErrorCollector
    {
        // IError는 Immtuable
        public ImmutableArray<IError> Errors { get; private set; }
        public bool HasError => Errors.Length != 0;

        bool raiseAssertionFail;

        public TestErrorCollector(bool raiseAssertionFail)
        {
            Errors = ImmutableArray<IError>.Empty;
            this.raiseAssertionFail = raiseAssertionFail;
        }

        public TestErrorCollector(TestErrorCollector other, CloneContext context)
        {
            this.Errors = other.Errors;
        }

        public void Add(IError error)
        {
            Errors = Errors.Add(error);
            Assert.True(!raiseAssertionFail || false);
        }

        public IErrorCollector Clone(CloneContext context)
        {
            return new TestErrorCollector(this, context);
        }

        public void Update(IErrorCollector src_errorCollector, UpdateContext context)
        {
            var src = (TestErrorCollector)src_errorCollector;
            Errors.Clear();
            Errors.AddRange(src.Errors);
        }
    }
}