﻿using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    public record VarDeclElement(Path Type, string Name, Exp? InitExp);
}
