using Citron.Analysis;

namespace Citron.IR0Translator
{
    abstract record class LambdaCapture;
    record class NoneLambdaCapture : LambdaCapture { public static readonly NoneLambdaCapture Instance = new NoneLambdaCapture(); private NoneLambdaCapture() { } }
    record class ThisLambdaCapture : LambdaCapture { public static readonly ThisLambdaCapture Instance = new ThisLambdaCapture(); private ThisLambdaCapture() { } }
    record class LocalLambdaCapture(string Name, ITypeSymbol Type) : LambdaCapture;
}
