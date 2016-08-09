using Gum.Evaluator;
using Gum.Lang.AbstractSyntax;
using Gum.Translator.Text2AST;
using Gum.Translator.Text2AST.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gum.Sandbox
{
    // Gum 개발 중 빠르게 실험해 보고 싶을때 쓰는 클래스
    class Program
    {
        static void Main(string[] args)
        {
            string code = File.ReadAllText(@"..\..\Tests\00_Print.gum");

            // preprocessing
            code = Regex.Replace(code, "//.*$", " ", RegexOptions.Multiline);

            var lexer = new Lexer(code);
            var ast = Parser<FileUnit, FileUnitParser>.Parse(lexer);

            // ASTEvaluator.Eval(ast);


            // ASTPrinter.Print(ast);

            // 실행기는..
            // var evaluator = new Evaluator();

            // 메타 데이터라고 표현하는 것이 좋은지?
            // evaluator.BuildMetadata(fileUnit);

            // First Pass: AST -> Metadata

            // 두 패스로 실행을 합니다. 
            // 1. 모든 파일들을 훑어서 메타데이터를 만들어내는 역할을 합니다.

            // 2. 훑은 파일을 이제 실행하는 역할을 합니다.
            
            // fileUnit.Print();
        }
    }
}
