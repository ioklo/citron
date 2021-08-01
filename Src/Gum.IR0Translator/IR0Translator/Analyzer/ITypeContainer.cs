using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        interface ITypeContainer
        {
            void AddStruct(R.StructDecl structDecl);
        }
    }
}