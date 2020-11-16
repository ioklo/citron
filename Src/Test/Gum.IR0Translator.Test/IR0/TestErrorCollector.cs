using Gum.Infra;
using System.Collections.Generic;
using Xunit;

namespace Gum.IR0
{
    class TestErrorCollector : IErrorCollector
    {
        public List<IError> Errors { get; }
        public bool HasError => Errors.Count != 0;

        bool raiseAssertionFail;

        public TestErrorCollector(bool raiseAssertionFail)
        {
            Errors = new List<IError>();
            this.raiseAssertionFail = raiseAssertionFail;
        }

        public void Add(IError error)
        {
            Errors.Add(error);
            Assert.True(!raiseAssertionFail || false);
        }
    }
}