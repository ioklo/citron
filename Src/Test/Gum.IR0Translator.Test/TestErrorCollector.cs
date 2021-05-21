﻿using Gum.Collections;
using Gum.Infra;
using System.Collections.Generic;
using Xunit;

namespace Gum.IR0Translator.Test
{
    // 본체는 Mutable
    class TestErrorCollector : IErrorCollector
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