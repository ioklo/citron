using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Xunit.Abstractions;
using System.Diagnostics;
using Gum.IR0Translator;
using System.Reflection;

namespace Gum.Test.IntegrateTest
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
        public abstract Task TestAsync();
    }

    public record ParseTranslateTestData(string Code, S.Script SScript, R.Script RScript) : TestData
    {
        public override string? GetCode()
        {
            return Code;
        }

        public override Task TestAsync()
        {
            return Misc.TestParseTranslateAsync(Code, SScript, RScript);
        }
    }

    public record ParseTranslateWithErrorTestData(string Code, S.Script SScript, AnalyzeErrorCode ErrorCode, S.ISyntaxNode Node) : TestData
    {
        public override string? GetCode()
        {
            return Code;
        }

        public override Task TestAsync()
        {
            return Misc.TestParseTranslateWithErrorAsync(Code, SScript, ErrorCode, Node);
        }
    }

    public record ParseTranslateEvalTestData(string Code, S.Script SScript, R.Script RScript, string Result) : TestData
    {
        public override string? GetCode()
        {
            return Code;
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

        public override Task TestAsync()
        {
            return Misc.TestEvalAsync(Code, Result);
        }
    }

    public record EvalWithErrorTestData(string Code, AnalyzeErrorCode ErrorCode) : TestData
    {
        public override string? GetCode()
        {
            return Code;
        }

        public override Task TestAsync()
        {
            return Misc.TestEvalWithErrorAsync(Code, ErrorCode);
        }
    }
}
