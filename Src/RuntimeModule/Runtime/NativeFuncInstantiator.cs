﻿using Citron.Module;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Invoker = Citron.Runtime.RuntimeModuleMisc.Invoker;

namespace Citron.Runtime
{
    //public class NativeFuncInstantiator
    //{
    //    public ModuleItemId FuncId { get; }

    //    bool bThisCall;
    //    Invoker invoker;

    //    public NativeFuncInstantiator(ModuleItemId funcId, bool bThisCall, Invoker invoker)
    //    {
    //        FuncId = funcId;
    //        this.bThisCall = bThisCall;
    //        this.invoker = invoker;
    //    }

    //    public FuncInst Instantiate(DomainService domainService, FuncValue fv)
    //    {
    //        return new NativeFuncInst(bThisCall, (thisValue, argValues, result) => invoker.Invoke(domainService, fv.TypeArgList, thisValue, argValues, result));
    //    }
    //}
}