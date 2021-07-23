using Gum.Collections;
using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gum.Test.Misc
{

    // 본체는 Mutable
    public class TestErrorCollector : IErrorCollector
    {
        // IError는 Immtuable
        public List<IError> Errors { get; }
        public bool HasError => Errors.Count != 0;

        bool raiseAssertionFail;

        public TestErrorCollector(bool raiseAssertionFail)
        {
            Errors = new List<IError>();
            this.raiseAssertionFail = raiseAssertionFail;
        }

        public TestErrorCollector(TestErrorCollector other, CloneContext context)
        {
            this.Errors = new List<IError>(other.Errors); // errors를 일일히 복사하지 않는다
        }

        public void Add(IError error)
        {
            Errors.Add(error);
            Debug.Assert(!raiseAssertionFail || false);
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

        public string GetMessages()
        {
            return string.Join("\r\n", Errors.Select(error => error.Message));
        }
    }
}