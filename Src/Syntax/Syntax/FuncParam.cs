using Citron.Collections;
using Pretune;
using System;
using System.Collections.Generic;

namespace Citron.Syntax
{
    public record struct FuncParam(bool HasOut, bool HasParams, TypeExp Type, string Name);    
}