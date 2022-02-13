using R = Citron.IR0;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        interface ITypeContainer
        {
            void AddType(R.TypeDecl typeDecl);
        }
    }
}