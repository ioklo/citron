# 기기 간 미커밋 작업 인계 스크립트

- 사용자 요청: Git root의 `task/`에 Windows/macOS 공용 pwsh 스크립트를 둔다.
- 수신 기기에서 `./task/pull-from.ps1 OLIVER`처럼 SSH 호스트 이름만 지정한다.
- 호스트별 저장소 경로는 수신 기기의 Git local config `handoff.OLIVER.repository`에 설정한다. 양쪽에 스크립트 설치와 Git 원격 인증이 필요하다.
- 기기별 Git remote는 각 저장소의 `handoff.remote` 설정으로 지정한다. `-Remote`가 우선하며 둘 다 없으면 설정 명령을 안내하고 중단한다. `origin` 자동 선택은 하지 않는다. `pull-from.ps1 -Remote`는 수신 측에만 적용하고 SSH 송신 측은 자신의 설정을 읽는다. 서로 다른 remote 이름도 같은 원격 저장소를 가리켜야 한다.
- 흐름: 수신 측 clean/진행 중 Git 작업 검사 → SSH의 명시적 pwsh 실행 → 송신 측 HEAD 일치 검사 → stash -u 및 Git remote 전송 → 수신 측 stash apply --index.
- 송신 작업 디렉터리는 비우고 로컬 stash는 보관한다. 수신 측 변경이 있으면 SSH 호출 전에 중단한다. 기준 커밋이 다르면 송신 stash 생성 전에 중단한다.
- staged/unstaged/untracked 상태를 전달한다. ignored 파일은 제외하며 submodule 변경은 거부한다. 실행 도중 양쪽 파일을 편집하지 않는 전제다.
- `handoff.ps1`은 push/pull/status 및 내부 check 기능을 제공한다. 단독 실행은 `refs/wip/citron`, SSH 인계는 호출별 `refs/wip/handoff/<id>`로 다른 인계와의 경합을 피한다. 원격 snapshot 및 로컬 incoming ref는 복구용으로 유지한다. 보관 ref/stash의 자동 정리는 구현하지 않았다.
- 전송 실패 후 송신 stash, 수신 실패 후 출력된 ref를 이용해 복구/재시도한다. 수신 성공 전에는 양쪽을 원자적으로 이동시키지 못하므로 실패 시 두 기기가 모두 clean인 경우도 있다.
- 검증: `task/tests/handoff.tests.ps1`의 임시 bare repository와 SSH 대역으로 왕복 인계, index 보존, untracked/ignored 처리, dirty 수신 거부, SSH payload 실행, 빈 송신 거부, 기준 커밋 불일치, push 실패 후 stash 복구를 검사한다. 실제 SSH/GitHub 및 macOS 실행 검증은 별도다.
