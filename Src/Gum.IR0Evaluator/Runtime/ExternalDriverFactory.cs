﻿using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Runtime
{
    // TODO: IR0Evaluator에서만 쓰이지 않을거라면 모듈 위치 재배치 
    public class ExternalDriverFactory
    {
        Dictionary<ExternalDriverId, IExternalDriver> drivers;

        public ExternalDriverFactory()
        {
            drivers = new Dictionary<ExternalDriverId, IExternalDriver>();
        }

        public IExternalDriver GetDriver(ExternalDriverId id)
        {
            return drivers[id];
        }

        public void Register(ExternalDriverId id, IExternalDriver testExternalDriver)
        {
            drivers.Add(id, testExternalDriver);
        }
    }
}
