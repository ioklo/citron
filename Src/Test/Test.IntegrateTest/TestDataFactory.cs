﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Citron.Syntax;
using M = Citron.CompileTime;
using R = Citron.IR0;
using Xunit.Abstractions;
using System.Diagnostics;
using Citron.IR0Translator;
using System.Reflection;
using Citron.Log;

namespace Citron.Test.IntegrateTest
{
    public class TestDataInfo : IXunitSerializable
    {
        Type? type;
        int index;
        public TestDataInfo() 
        { 
            type = null; 
            index = -1; 
        }

        public TestDataInfo(Type type, int index)
        {
            this.type = type;
            this.index = index;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            var typeName = info.GetValue<string>("AssemblyQualifiedTypeName");

            this.type = Type.GetType(typeName);
            this.index = info.GetValue<int>("Index");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            Debug.Assert(type != null);

            info.AddValue("AssemblyQualifiedTypeName", type.AssemblyQualifiedName);
            info.AddValue("Index", index);
        }

        public Task InvokeAsync()
        {
            if (type == null || index == -1) return Task.CompletedTask;

            return IntegrateTestData.GetInfo(type, index).MakeTestData().TestAsync();
        }

        public override string ToString()
        {
            if (type == null || index == -1) return string.Empty;

            return IntegrateTestData.GetInfo(type, index).Desc;
        }
    }

    public abstract record TestData
    {
        public abstract string? GetCode();
        public abstract R.Script? GetRScript();
        public abstract Task TestAsync();
    }

    public record ParseTranslateTestData(string Code, S.Script SScript, R.Script RScript) : TestData
    {
        public override string? GetCode()
        {
            return Code;
        }

        public override R.Script? GetRScript()
        {
            return RScript;
        }

        public override Task TestAsync()
        {
            return Misc.TestParseTranslateAsync(Code, SScript, RScript);
        }
    }

    public record ParseTranslateWithErrorTestData : TestData
    {
        string code;
        S.Script sscript;
        ILog log;

        public ParseTranslateWithErrorTestData(string code, S.Script sscript, SyntaxAnalysisErrorCode errorCode, S.ISyntaxNode node)
        {
            this.code = code;
            this.sscript = sscript;
            this.log = new SyntaxAnalysisErrorLog(errorCode, node, "");
        }

        public ParseTranslateWithErrorTestData(string code, S.Script sscript, ILog log)
        {
            this.code = code;
            this.sscript = sscript;
            this.log = log;
        }

        public override string? GetCode()
        {
            return code;
        }

        public override R.Script? GetRScript()
        {
            return null;
        }

        public override Task TestAsync()
        {
            return Misc.TestParseTranslateWithErrorAsync(code, sscript, log);
        }
    }

    public record ParseTranslateEvalTestData(string Code, S.Script SScript, R.Script RScript, string Result) : TestData
    {
        public override string? GetCode()
        {
            return Code;
        }

        public override R.Script? GetRScript()
        {
            return RScript;
        }

        public override Task TestAsync()
        {
            return Misc.TestParseTranslateEvalAsync(Code, SScript, RScript, Result);
        }
    }

    public record EvalTestData(string Code, string Result) : TestData
    {
        public override string? GetCode()
        {
            return Code;
        }

        public override R.Script? GetRScript()
        {
            return null;
        }

        public override Task TestAsync()
        {
            return Misc.TestEvalAsync(Code, Result);
        }
    }

    public record EvalWithErrorTestData(string Code, SyntaxAnalysisErrorCode ErrorCode) : TestData
    {
        public override string? GetCode()
        {
            return Code;
        }

        public override R.Script? GetRScript()
        {
            return null;
        }

        public override Task TestAsync()
        {
            return Misc.TestEvalWithErrorAsync(Code, ErrorCode);
        }
    }
}