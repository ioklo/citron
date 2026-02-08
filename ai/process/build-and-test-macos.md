# 빌드 및 테스트 (macOS 예시)

필수 도구: `cmake`, C++23을 지원하는 컴파일러. 선택 도구: `vcpkg`.

예시 명령:

```bash
cd src
mkdir -p build && cd build
cmake -DCMAKE_BUILD_TYPE=Debug ..
cmake --build . -- -j$(sysctl -n hw.ncpu)
ctest --output-on-failure
```

vcpkg 사용 시:

```bash
cmake -DCMAKE_TOOLCHAIN_FILE=/path/to/vcpkg.cmake -DCMAKE_BUILD_TYPE=Debug ..
```

의존성은 `vcpkg.json`과 `vcpkg-configuration.json`을 확인하세요.
