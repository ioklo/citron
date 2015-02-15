﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserGenerator.AST
{
    class SimpleArgNode : ArgNode
    {
        public string Type { get; private set; }
        public string Name { get; private set; }

        public SimpleArgNode(string type, string name)
        {        
            Type = type;
            Name = name;
        }
    }
}
