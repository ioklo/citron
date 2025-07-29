#pragma once

// 에러가 너무 많아서 variant로 하기엔 무리가 있다
struct ResolveIdentifierError 
{
    virtual ~ResolveIdentifierError() = default;
};

struct ResolveIdentifierError_MultipleCandidates : ResolveIdentifierError {}; 