namespace Citron.IR1; // TODO: namespace 정책 변경..

public struct AllocInfoId
{
    public const int RefValue = -1;
    public const int BoolValue = -2;
    public const int IntValue = -3;

    public static AllocInfoId RefId { get; } = new AllocInfoId(RefValue);
    public static AllocInfoId BoolId { get; } = new AllocInfoId(BoolValue);
    public static AllocInfoId IntId { get; } = new AllocInfoId(IntValue);

    public int Value { get; }
    public AllocInfoId(int value) { Value = value; }
}
