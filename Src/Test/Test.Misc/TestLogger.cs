using Citron.Collections;
using Citron.Infra;
using Citron.Log;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Citron.Test.Misc
{

    // 본체는 Mutable
    public class TestLogger : ILogger
    {
        // IError는 Immtuable
        public List<ILog> Logs { get; }
        public bool HasError => Logs.Count != 0;

        bool raiseAssertionFail;

        public TestLogger(bool raiseAssertionFail)
        {
            Logs = new List<ILog>();
            this.raiseAssertionFail = raiseAssertionFail;
        }

        public TestLogger(TestLogger other, CloneContext context)
        {
            this.Logs = new List<ILog>(other.Logs); // errors를 일일히 복사하지 않는다
        }

        public void Add(ILog error)
        {
            Logs.Add(error);
            Debug.Assert(!raiseAssertionFail || false);
        }

        public ILogger Clone(CloneContext context)
        {
            return new TestLogger(this, context);
        }

        public void Update(ILogger src_logger, UpdateContext context)
        {
            var src = (TestLogger)src_logger;
            Logs.Clear();
            Logs.AddRange(src.Logs);
        }

        public string GetMessages()
        {
            return string.Join("\r\n", Logs.Select(error => error.Message));
        }
    }
}