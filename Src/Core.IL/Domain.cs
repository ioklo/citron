using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL
{
    // Type과 Value를 같은 ID 공간에 두면
    
    // 어떤 x를 다음과 같은 방식으로 찾습니다
    // 1) 현재 스코프에서 찾고 (함수 스코프)
    // 2) this에서 찾고(부모 클래스까지)
    // 3) 현재 클래스에서 찾고 (static 변수 등)
    // 4) using namespace에서 찾고 (type 등)
    // 5) global에서 찾고 (4번이랑 통합 가능성 있음)

    // 함수 안에 로컬 변수 선언시
    // AValue AValue; 라고 하면
    // 앞의 AValue는 타입이어야 하고, 이 줄 이후부터 AValue는 값입니다.

    // 프로그램 실행을 위한 정보들, 실행 중 바뀔 수도 있습니다
    public class Domain
    {
        // Extern 타입들을 .... 어떻게 해야 합니까
        // Domain 저장시에는 Extern이었다가.. Load 시에 실제 함수로 교체 해야 합니다.
        // 즉 로딩시에 Extern이 로드 되면, ExternManager가 가져 오도록        

        // 이름 -> 타입 + 함수 + 글로벌 Variable Location 위치
        Dictionary<string, IValue> idValueMap;
        public event Action<string, IValue> OnValueAdded;

        public IEnumerable<IValue> Values { get { return idValueMap.Values; } }
        public Domain ParentDomain {get; private set;}

        public Domain(Domain parentDomain)
        {
            ParentDomain = parentDomain;
            idValueMap = new Dictionary<string, IValue>();
        }

        public Domain()
        {
            ParentDomain = null;
            idValueMap = new Dictionary<string, IValue>();
        }

        public bool TryGetValue<T>(string name, out T value) where T : IValue
        {
            if (!TryGetValue(name, out value)) return false;
            return (value is T);
        }

        public bool TryGetValue(string name, out IValue value)
        {
            if (idValueMap.TryGetValue(name, out value))
                return true;

            if (ParentDomain != null)
                return ParentDomain.TryGetValue(name, out value);

            return false;
        }

        public void AddValue(string name, IValue value)
        {
            idValueMap.Add(name, value);            
            if (OnValueAdded != null) OnValueAdded(name, value);
        }
    }
}
