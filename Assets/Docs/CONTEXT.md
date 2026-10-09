# 사이버 퇴마사 개발 맥락

문서 갱신: 2026-10-10 (한국 시간)
현재 단계: 첫 번째 의뢰의 단일 씬 플레이 프로토타입

## 1. 사용자의 목표와 확정된 조건

플레이어는 사이버 퇴마사다. 메일로 의뢰를 받고 인터넷 검색과 사이트 탐색을 통해 괴이를 찾아낸다. 괴이에게 도달하면 패턴을 파악하고 상호작용하여 퇴마한다.

사용자가 명시한 조건:
- UI는 Play 진입 시 생성하지 않고 씬에 미리 배치하여 사용자가 직접 편집할 수 있어야 한다(2026-10-09 추가).
- 현재는 프로토타입이므로 모든 진행을 씬 하나 안에서 처리한다.
- 실제 개발에서는 구조를 더 정리할 예정이다.
- 제공한 HTML을 참고한다. 첫 사례는 카메라를 치워 뒤에 숨은 귀신을 발견하고 대화 후 성불시키는 내용이다.

초기 버전의 큰 런타임 스크립트는 흐름을 빠르게 검증하기 위한 구현 선택이었다. 2026-10-09 사용자 요청에 따라 UI를 씬에 미리 배치하고 진행 코드와 표시 참조를 분리했다. 사용자가 본 개발에서도 이 구조를 유지하라고 요구한 것은 아니다. 본 개발의 상세 아키텍처는 아직 결정하지 않았다.

2026-10-09 사용자와 정한 협업 원칙:
- `Assets/AGENTS.md`와 이 문서의 확정된 규칙·조건을 이후 작업의 기준으로 삼는다.
- 새 요청이 기존 규칙·조건과 충돌하면 해당 내용과 충돌 지점을 설명하고, 사용자에게 다시 확인한 뒤 충돌하는 작업을 진행한다. 이미 명시적으로 합의한 규칙 변경은 반복 확인하지 않는다.
- 규칙은 사용자와 함께 수정하며, 합의한 변경은 관련 문서에 반영한다. 구현 현황·과거 검증·미확정 제안은 고정된 요구사항과 구분한다.

2026-10-10 추가 협업 원칙:
- 게임 테스트는 사용자가 직접 수행한다. 에이전트는 테스트를 직접 실행하지 않고 조작 순서와 기대 결과가 포함된 목록을 제공한다.
- 테스트 목적의 Play 진입, 입력 시뮬레이션, 버튼 이벤트·진행 함수 호출, 자동 플레이, Test Runner·검증 스크립트 실행, 미리보기 화면 순회를 하지 않는다. 기존 `AgentScripts/ValidateSceneUI.cs`도 실행하지 않는다.
- 구현을 위한 씬 편집, 코드·문서의 정적 점검, 컴파일 오류 확인과 게임 테스트를 구분한다. 게임 테스트 결과는 사용자가 알려준 내용으로 기록한다.

## 2. 실행 환경과 진입점

- 프로젝트 상대 경로를 기준으로 설명한다. 다른 컴퓨터에서도 같은 구조를 사용할 수 있다.
- 2026-10-10 사용자 요청으로 규칙 본문은 `Assets/AGENTS.md`, 맥락·편집 안내는 `Assets/Docs`로 이동했다. 루트 `AGENTS.md`에는 새 규칙 위치를 안내하는 진입점만 남겨 프로젝트 전체 작업에서 같은 규칙을 읽도록 한다.
- 최초 문서 작성 당시 로컬 루트: `C:\Local\Zol_Project`. 이번 작업에서 확인한 루트: `D:\CWS\Zol_Project`.
- Unity 버전: `6000.3.23f1` (`ProjectSettings/ProjectVersion.txt`).
- 실행 씬: `Assets/Scenes/CyberExorcistPrototype.unity`.
- 씬을 열고 Play를 누른다. Game 뷰를 최대화하면 읽기 편하다.
- UI는 기준 해상도 1600×900이며 CanvasScaler의 Scale With Screen Size / Expand 설정을 사용한다.
- 씬에는 Main Camera, 기존 test, CyberExorcistPrototype 루트가 있다. 루트 아래 Canvas와 화면별 UI, EventSystem이 미리 저장되어 있다. 버튼 40개의 Click 이벤트와 창 드래그 대상도 직렬화되어 있다.
- Build Settings에는 프로토타입과 기존 SampleScene이 모두 등록되어 있다. 게임 진행에서 SampleScene으로 전환하지 않는다.

Unity MCP는 2026-10-06 작업에서 해당 프로젝트의 Editor 연결과 편집에 사용했다. 당시 셸의 `unity` 명령은 PATH에서 찾지 못했지만 MCP 호출은 동작했다. 다른 환경에서도 같은 설치·연결 상태라고 가정하지 말고 대상 프로젝트와 연결을 확인한다. 문서와 프로젝트를 복사해도 MCP 연결 설정 자체가 이전되는 것은 아니다.

## 3. 현재 플레이 흐름

1. 민서의 메일 「누군가 제 화면을 보고 있어요」를 읽고 minseo_404 연락처를 추가한다.
2. 메신저에서 사이트에 들어간 경위를 묻고 영상 제목 ‘관음증 귀신’을 알아낸다.
3. 획득한 ‘관음증’과 ‘귀신’ 키워드 두 개를 선택해 검색한다.
4. 일반 정보 게시글과 괴담 게시판 중 의뢰에 맞는 기록을 찾는다. 일반 정보 게시글에서도 뒤로 이동할 수 있다.
5. 괴담 게시글의 강조된 문장을 클릭한다. 자동 녹화와 눈동자 아이콘이 의뢰인의 증상과 일치함을 확인한다.
6. 잠금이 풀린 사이트 이동 버튼을 누르고, 외부 연결을 차단한다는 게임 내 절차를 거친다.
7. 촬영도구 다섯 개를 클릭해 치워 숨은 귀신을 발견한다.
8. 귀신의 이야기를 듣고 성불을 돕는다. 약 1.8초 동안 위로 떠오르며 사라지는 연출이 실행된다.
9. 의뢰 완료 화면과 민서의 감사 메시지를 확인한다.

부가 기능: 앱 전환, 조사 기록과 현재 목표, 알림, 합성 효과음과 음소거, 창 드래그, 최소화·복원, 확인창을 거치는 재시작.

실제 웹 요청, 메일 발송, 시스템 네트워크 차단은 수행하지 않는다. ‘외부 연결 차단’ 역시 게임 상태 변화다.

## 4. 핵심 파일

| 파일 | 역할 |
| --- | --- |
| `Assets/CyberExorcist/CyberExorcistPrototype.cs` | 진행 상태, 버튼 행동 처리, 알림, 효과음, 성불 연출 |
| `Assets/CyberExorcist/PrototypeSceneUI.cs` | 씬 UI 참조, 화면 표시 상태, Inspector에서 편집하는 상태별 문구 |
| `Assets/CyberExorcist/Editor/PrototypeSceneBuilder.cs` | 최초 1회 UI 배치 및 버튼 이벤트 연결. 기존 UI가 있으면 재생성 거부 |
| `Assets/CyberExorcist/Editor/PrototypeSceneUIEditor.cs` | 편집 모드에서 21개 화면을 선택하는 UI Toolkit Inspector |
| `Assets/Docs/UI_EDITING.md` | Hierarchy 구조, 화면 미리보기, UI 편집 위치, 검증 스크립트 사용법 |
| `AgentScripts/ValidateSceneUI.cs` | Pipeline run_script용 씬 참조·진행·미리보기 검증 |
| `Assets/CyberExorcist/PrototypeWindowDrag.cs` | 제목 표시줄 드래그로 창 위치 이동 |
| `Assets/CyberExorcist/Editor/PrototypeSetup.cs` | 폰트 에셋 구성, 컴포넌트·리소스 연결, 씬 저장과 Build Settings 등록 |
| `Assets/Scenes/CyberExorcistPrototype.unity` | 실행 루트, 모든 게임 UI 오브젝트, 이벤트와 리소스 참조를 보관하는 씬 |
| `Assets/CyberExorcist/README.md` | 플레이 및 실행 안내 |
| `Assets/Screenshots/Prototype` | 2026-10-06 검증 중 저장한 화면 자료. 현재 상태를 보장하는 실시간 캡처는 아님 |

## 5. 실행과 화면 구성 원리

```text
씬 로드: Canvas, 앱 화면, 조사 기록, 확인창, EventSystem과 버튼 이벤트 복원
  → CyberExorcistPrototype.Start()
  → 씬에서 편집한 창·귀신의 초기 위치 기록, 합성 효과음 준비
  → ShowApp("mail")
  → PrototypeSceneUI.Refresh(): 기존 오브젝트의 활성 상태·문구·버튼 잠금 갱신

버튼 클릭
  → 씬에 저장된 Button.onClick → Click(행동 ID)
  → Handle(행동 ID) → 진행 조건 확인 및 상태 변경
  → Render() → PrototypeSceneUI.Refresh()
```

- 런타임 코드에는 UI GameObject 생성·파괴 또는 화면 재생성이 없다. 합성 AudioClip 생성은 유지한다.
- 모든 UI의 위치·크기·색·폰트와 일반 문구는 씬 오브젝트가 기준이다. 메일 본문·게시글·메신저 단계별 대사 등은 TMP Text를 직접 수정한다.
- 상태별 문구는 `PrototypeSceneUI.TextStates.Values`에 저장한다. 앱 제목, 검색 키워드, 귀신 대사·선택지, 조사 기록과 목표 등이 이에 해당한다. 해당 TMP의 Text만 바꾸면 Refresh에서 Values로 갱신되므로 Values를 편집한다.
- `ExorcistDesktop/Desktop/ApplicationWindow/Content` 아래 Mail, Chat, Browser가 있다. Chat은 연락처 없음/단계별 대화/완료, Browser는 Search/Article/Information/HauntedSite로 나뉜다.
- HauntedSite는 Isolation/Cameras/GhostConversation/Ending을 전환한다. 카메라 5개도 씬에 저장되어 제거 시 비활성화되고 재시작 시 다시 표시된다.
- 메일·대화·인터넷은 공통 앱 창을 공유한다. 독립적인 창 세 개를 동시에 유지하는 구조는 아니다.
- 편집 모드에서 루트의 Prototype Scene UI Inspector로 화면을 미리 볼 수 있다. 미리보기는 플레이 진행 상태를 바꾸지 않는다. 어떤 화면을 저장했든 Play는 메일부터 시작한다.
- 버튼은 Inspector의 On Click에 `CyberExorcistPrototype.Click(string)`와 행동 ID가 저장되어 있다. 이름이나 위치가 바뀌어도 참조는 유지된다.
- `Navigate()`는 페이지 이력을 Stack에 넣는다. `Departure()`는 기존 귀신 오브젝트를 이동·페이드한 뒤 완료 상태로 바꾼다.
- `ResetCase()`는 진행 상태와 이력을 초기화하고, 씬에서 지정한 초기 창 위치와 귀신 표시를 복원한다.
- `PrototypeSceneBuilder`는 최초 배치용 에디터 코드다. 현재 UI 변경은 씬을 수정한다. 재생성으로 편집 내용을 덮어쓰지 않는다.
- TMP는 글꼴 fallback을 렌더링하면서 내부 SubMeshUI를 만들 수 있다. 이는 게임의 화면 재생성과 구분한다.

## 6. 주요 상태와 조건

| 상태 | 의미와 조건 |
| --- | --- |
| `app` | mail / chat / browser 중 현재 앱 |
| `page` | home / results / article / side / haunted 중 브라우저 페이지 |
| `ContactAdded` | 연락처 추가 여부 |
| `chatStep` / `KeywordAcquired` | 대화 단계. chatStep >= 2일 때 키워드 획득 |
| `termA`, `termB` | 검색 키워드 선택 여부. 키워드 획득 및 두 선택값이 모두 true여야 검색 진행 |
| `ClueFound` | 게시글 증상 대조 완료. 사이트 이동 허용 조건 |
| `isolated` | 사이트 조사 시작을 위한 게임 내 격리 절차 완료 |
| `removed` / `CamerasRemoved` | 제거한 촬영도구 번호의 HashSet 및 개수. 다섯 개 제거 시 귀신 대화 |
| `ghostStep` | 귀신 대화 단계. 마지막 선택에서 성불 코루틴 실행 |
| `departing` | 성불 연출 진행 중. 이 동안 Handle의 행동 처리를 잠시 차단 |
| `Completed` | 의뢰 완료. 완료 화면과 감사 메시지 분기 |
| `history` | 브라우저 뒤로 이동에 사용하는 Stack |

## 7. 리소스 출처와 연결

| 리소스 | 연결·사용 방식 |
| --- | --- |
| `Assets/CyberExorcist/Fonts/Malgun.ttf` | 작성 PC의 Windows 맑은 고딕 원본 |
| `Assets/CyberExorcist/Fonts/PrototypeKorean.asset` | 각 씬 TMP 컴포넌트가 참조하는 동적 한국어 폰트 |
| `Assets/CyberExorcist/Art/Reference1.png` | 참고 HTML에서 추출한 카메라 그림. 씬 RawImage가 직접 참조 |
| `Assets/CyberExorcist/Art/Reference0.png` | 참고 HTML에서 추출한 원본 이미지. 씬 RawImage의 Texture와 uvRect로 귀신 영역 표시 |
| 효과음 | Tone()에서 실행 시 AudioClip 생성. 외부 오디오 파일 없음 |

`PrototypeSetup.Setup()`은 최초 구성 시 AssetDatabase로 에셋을 읽고 PrototypeSceneBuilder로 UI를 배치한 뒤 씬에 저장한다. 프로토타입 씬이 아닌 경우 중단하고, 기존 UI가 있으면 편집 내용을 보존하며 종료한다. 일반 실행에는 호출하지 않는다. 루트의 기존 KoreanFont/CameraArtwork/GhostReference 필드는 최초 구성 호환용으로 유지하며 Inspector에서 숨겼다. 플레이 시에는 저장된 UI 참조를 사용하고 HTML이나 AssetDatabase를 읽지 않는다.

사용자가 제공했던 HTML 원본 위치:
`C:\Users\cws78\OneDrive - 명지대학교\명지대\3-2\캡스톤게임콘텐츠기획\index.html`

원본 HTML은 다른 컴퓨터에 없을 수 있으나 현재 프로토타입 실행에는 필요하지 않다. 원본과 세부 비교가 필요한 작업에서만 접근 가능 여부를 확인한다. 기존 README에 기록했듯 배포용 폰트는 라이선스 확인 또는 교체가 필요하다.

## 8. 검증 이력

### 2026-10-06 — Unity 실행 검증

이 대화에서 실제로 수행한 검증 기록이다.
- 컴파일 성공과 Play 모드 프레임 증가 확인.
- 실제 마우스 입력으로 연락처 추가 확인.
- 연결된 UI 버튼 이벤트로 메일부터 의뢰 완료까지 진행 확인. 모든 단계를 물리 마우스 클릭으로 검증한 것은 아니다.
- 키워드 미획득 및 단일 키워드 검색 제한 확인.
- 단서 확인 전 사이트 진입 버튼 비활성화 확인.
- 일반 정보 게시글에서 뒤로 이동, 앱 전환 후 카메라 제거 상태 유지 확인.
- 카메라 다섯 개 제거, 귀신 대화, 성불 완료, 의뢰인의 감사 메시지 확인.
- 재시작 취소 시 상태 유지, 재시작 확인 시 전체 진행 초기화, 최소화·복원 확인.
- 카메라 라벨 겹침, 귀신 이미지 하단 경계, 알림 위치 수정 후 화면 확인.
- 최종 확인에서 열린 씬 1개, EventSystem 1개, 해당 화면의 텍스트 넘침 없음.
- 게임 실행 검증 구간에서 새 경고·예외 없음. 작업 초기에 MCP 통신 타임아웃 기록은 있었으며 Editor에 포커스를 준 뒤 호출이 정상화되었다.
- 마지막에 Play를 종료하고 runInBackground 설정을 false로 복원했다.

이 기록은 현재 시점의 Unity 연결·실행 상태를 보장하지 않는다. 독립 실행 파일 빌드와 배포 환경 검증은 수행하지 않았다.

### 2026-10-09 — 인수인계 문서 작성

- 기존 README, 주요 상태 및 Handle 구현, Setup 코드, Unity 버전과 Build Settings를 읽어 문서와 대조했다.
- AGENTS.md와 이 문서를 추가했다. 게임 코드 및 씬은 변경하지 않았다.
- 이번 문서 작업에서 Unity Play 검증을 다시 수행하지 않았다.

### 2026-10-09 — 씬에 미리 배치하는 UI로 전환

- 사용자 요청: 실행 중 코드에서 UI를 생성하지 않고 씬에서 직접 편집할 수 있도록 변경.
- Canvas와 화면별 패널, 카메라, 귀신, 조사 기록, 모달, AudioSource, EventSystem을 씬에 저장했다. 기존 Main Camera와 test 및 에셋 GUID를 유지했다.
- 최초 배치 도구와 런타임 진행 코드를 분리했고, 기존 UI의 자동 재생성을 막았다. 편집 화면 선택 Inspector와 UI_EDITING.md를 추가했다.
- 컴파일 성공. 씬 저장 후 재열기에서 UI 참조, 버튼 40개와 창 드래그 참조가 유지되는지 검사했다.
- Play 프레임 증가와 실제 Input System 포인터 클릭으로 연락처 추가를 확인했다.
- 버튼의 저장된 이벤트를 호출하여 검색 키워드 0개/1개 제한, 단서 전 사이트 잠금, 뒤로 이동, 앱 전환 시 진행 유지, 촬영도구 5개, 귀신 대화, 성불·완료 답장, 최소화·복원·음소거와 재시작 취소/확인을 검사했다.
- 제목 표시줄 Raycast와 EventSystem 드래그 이벤트를 검사했다. 모든 조작을 실제 포인터 입력으로 검증한 것은 아니다.
- 화면 전환·완료·재시작 시 기존 게임 UI 오브젝트의 인스턴스가 유지되고, 변경한 일반 제목·위치가 덮어써지지 않음을 검사했다.
- 21개 편집 미리보기를 순회하며 텍스트 넘침이 없음을 확인했다. 메일과 귀신 Game 뷰를 캡처하여 한국어와 배치를 확인했다.
- 초기 편집 미리보기 검사에서 TMP의 일시적 Ellipsis 경고와 fallback SubMeshUI 생성이 관찰됐다. 검사에서 렌더러 보조 오브젝트와 게임 UI를 구분했고, 이후 미리보기 검사에서 새 경고 없이 통과했다.
- 저장된 귀신 미리보기에서 Play를 시작해도 첫 메일부터 시작하는 것을 확인했다. 마지막에 Play를 종료하고 메일 미리보기로 저장했으며 runInBackground를 false로 복원했다.
- 독립 실행 파일 빌드, 다양한 기기 해상도, 모든 버튼의 실제 포인터 입력은 검증하지 않았다.

## 9. 현재 한계와 다음 작업

2026-10-10 인수인계: 씬 UI 전환 구현과 저장은 완료되어 있다. 이후 게임 테스트는 사용자 담당으로 변경했다. 이번 지시 이후에는 추가 게임 테스트를 수행하지 않았으며, 사용자가 확인할 목록은 `Assets/Docs/UI_EDITING.md`의 '사용자 테스트 목록'에 정리했다. 위 검증 이력은 이 지시 이전에 수행한 기록이다.

현재 범위:
- 의뢰 한 개, 미리 정의한 검색 키워드·결과·대화만 제공한다.
- 진행은 메모리에만 유지한다. 저장·불러오기 기능은 없다.
- 실제 웹·메일·메신저 서비스와 연결하지 않는다.
- 진행 로직은 CyberExorcistPrototype, 표시 갱신은 PrototypeSceneUI로 분리했다. 배치와 대부분의 문구는 씬에서 편집하지만 알림과 진행 분기는 아직 코드에 들어 있다.

확정된 다음 개발 기능은 아직 없다. 다음 사용자의 요청을 기준으로 작업 범위를 정한다.

본 개발 전 검토할 수 있는 항목이며, 아직 승인된 작업 목록은 아니다:
- 의뢰·메일·대화·검색 결과를 데이터로 분리할지 결정.
- 진행 상태와 화면 표시 책임 분리.
- 현재 씬 UI를 유지하면서 반복 요소를 프리팹으로 분리할지 검토.
- 다중 의뢰, 저장·불러오기, 추가 괴이 패턴의 범위 결정.

## 10. 새 Codex에서 이어가기

1. 프로젝트 파일과 `.meta`를 포함한 에셋(Assets/AGENTS.md와 Assets/Docs 포함), Packages, ProjectSettings와 루트의 AGENTS.md 안내 파일을 함께 가져온다. Git을 사용할 경우 문서도 코드와 함께 버전 관리한다.
2. Assets/AGENTS.md, 이 문서, 실행 README를 읽는다.
3. 실제 파일과 현재 변경사항을 확인한다. 과거 대화만으로 현재 상태를 단정하지 않는다.
4. Editor 작업이 필요하면 Unity 버전, 대상 프로젝트, MCP 연결을 확인한다.
5. 사용자가 요청한 다음 기능을 구현하고 정적 점검·컴파일 확인을 수행한다. 게임 테스트는 직접 실행하지 않고 사용자에게 조작 순서와 기대 결과를 제공한다.
6. 구현 상태나 결정이 바뀌면 이 문서를 갱신한다.

새 대화 시작 예시:
> Assets/AGENTS.md와 Assets/Docs/CONTEXT.md를 읽고 현재 코드를 확인해줘. 단일 씬 프로토타입을 유지하면서 개발을 이어갈 거야. 이번 작업은 [작업 내용]이야.
