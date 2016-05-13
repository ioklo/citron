﻿using Gum.Lang.AbstractSyntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Text2AST.Parsing
{
    // Lexer를 받아서 %
    public class FileUnitParser : Parser<FileUnit>
    {
        protected override FileUnit ParseInner(Lexer lexer)
        {
            var fileUnitComponents = new List<IFileUnitComponent>();

            // 끝날때까지 
            while (!lexer.End)
            {   
                var stmtComp = Parse<IStmtComponent, StmtComponentParser>(lexer);
                if( stmtComp != null)
                {
                    fileUnitComponents.Add(stmtComp);
                    continue;
                }
                
                throw new ParsingFailedException<FileUnitParser, IFileUnitComponent>();
            }

            return new FileUnit(fileUnitComponents);
        }

    }
}