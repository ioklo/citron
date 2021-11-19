using Gum.Analysis;

namespace Gum.IR0Translator
{
    abstract record LambdaCapture;
    record NoneLambdaCapture : LambdaCapture { public static readonly NoneLambdaCapture Instance = new NoneLambdaCapture(); private NoneLambdaCapture() { } }
    record ThisLambdaCapture : LambdaCapture { public static readonly ThisLambdaCapture Instance = new ThisLambdaCapture(); private ThisLambdaCapture() { } }
    record LocalLambdaCapture(string Name, TypeValue Type) : LambdaCapture;
}
