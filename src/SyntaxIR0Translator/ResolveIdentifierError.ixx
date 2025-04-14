export module Citron.SyntaxIR0Translator:ResolveIdentifierError;

// 에러가 너무 많아서 variant로 하기엔 무리가 있다
export struct ResolveIdentifierError 
{
    virtual ~ResolveIdentifierError() = default;
};

export struct ResolveIdentifierError_MultipleCandidates : ResolveIdentifierError {}; 