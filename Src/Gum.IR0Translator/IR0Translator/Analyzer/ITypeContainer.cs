using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        interface ITypeContainer
        {
            void AddType(R.TypeDecl typeDecl);
        }
    }
}