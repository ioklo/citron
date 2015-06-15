﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public interface IFileUnitDeclVisitor
    {
        void Visit(FuncDecl funcDecl);
        void Visit(VarDecl varDecl);
    }
}
