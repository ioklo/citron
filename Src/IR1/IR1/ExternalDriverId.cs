using System;

namespace Citron.IR1; // TODO: namespace 정책 변경..
                      // 범위: 전역

public struct ExternalDriverId
{
    public string Value { get; }
    public ExternalDriverId(string value) { Value = value; }

    public override bool Equals(object? obj)
    {
        return obj is ExternalDriverId id &&
               Value == id.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }
}
