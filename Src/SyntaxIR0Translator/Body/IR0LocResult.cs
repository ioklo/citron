using Citron.Symbol;
using R = Citron.IR0;

namespace Citron.Analysis;

record struct IR0LocResult(R.Loc Loc, IType LocType);

