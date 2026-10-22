# 사이버 퇴마사 개발 맥락

게임 개요와 작업 진입점만 관리한다. 구현·수정·검증·테스트 이력은 기록하지 않는다.

## 게임과 작업 기준

- 사이버 퇴마사가 의뢰를 받고 인터넷·메신저로 괴이를 조사한 뒤 패턴을 파악하여 퇴마하는 게임이다. 웹·메일·메신저는 게임 내부 UI다.
- 작업 규칙은 프로젝트 루트의 [AGENTS.md](../AGENTS.md)를 따른다. 관련 문서만 필요할 때 읽고 실제 코드·씬·사용자 변경을 확인한다.
- UI는 uGUI·TextMeshPro·Input System을 사용하며 씬·프리팹에 미리 저장한다. 기존 배치·대사·참조와 에셋 GUID를 보존한다.
- 게임 테스트는 사용자가 수행한다. 에이전트는 정적 점검과 컴파일 오류 확인만 수행하고 테스트 목록은 채팅으로 전달한다.

## 개발 진입점

| 작업 | 파일·폴더 |
| --- | --- |
| 탐색 씬 | `Assets/Scenes/Explore_Proto.unity` |
| 배틀 씬 | `Assets/Scenes/Battle_Proto.unity` |
| 탐색·통화·씬 연결·배틀 진행 | `Assets/Explore_Proto/Scripts` |
| CSV 리더 | `Assets/Scripts/DialogueReader/DialogueCsvReader.cs` |
| 하단 대화창 프리팹 | `Assets/Prefabs/BottomDialogueSystem.prefab` |
| 화상통화 카드 프리팹 | `Assets/Prefab/BloodyMaryCall` |
| 기존 민서 의뢰 프로토타입 | `Assets/Scenes/CyberExorcistPrototype.unity`, `Assets/CyberExorcist` |

- 탐색에서 조사 후 배틀로 이어지며, 배틀 단독 실행은 조사 완료 상태에서 시작한다.
- 배틀 통화는 유미나·이서연 2인이다. `BattleProtoConversation`의 Element 12를 넘기면 암전과 메리 등장 연출이 자동 재생된다. 1페이즈 카드 9장의 빈 틀이 하나씩 배치되고 칼을 든 손이 통화창 안에 완전히 들어와 잠시 멈춘 뒤, 같은 카드에 메리 이미지가 점점 빠르게 나타난다. 칼 든 메리 한 명을 포함한 마지막 이미지가 공개되면 그 카드들이 곧바로 조작 가능해진다.
- 현재 배틀 1페이즈는 화상통화 카드 퍼즐이다. 9장/칼 1개 → 12장/칼 3개 → 18장/칼 5개, 각 파트 전체 표시 후 10초 제한을 적용한다. 2·3파트의 카드도 하나씩 점점 빠르게 등장한다. 이후 2페이즈에서는 붉은 문구에서 음절을 골라 문장을 완성한다. 1파트 문구는 입력창 위에서 생성되고 3.4초 간격으로 바뀐다. 2파트의 사람 형상은 `Assets/Sprite/ProtoType/Phase2_Part2.png` 한 장으로 표시하며 씬에 저장된 클릭 영역으로 음절을 선택한다.
- 이전 인터넷 이미지·어절 수집 코드가 남아 있으므로 현재 활성 흐름은 씬의 연결을 기준으로 확인한다. 저장·불러오기는 미구현이다.

## 필요한 작업에서만 읽을 문서

- CSV 데이터 연동: [대사 CSV 사용 가이드](../Assets/Scripts/HowToUseScripts/DIALOGUE_CSV_GUIDE.md)
- 하단 대화창: [사용 안내](../Assets/Scripts/HowToUseScripts/BOTTOM_DIALOGUE_USAGE.txt)
- 기존 민서 프로토타입 UI: [UI 편집 안내](UI_EDITING.md)
- 스토리·대사 작성: [스토리 작업 규칙](../Assets/Scripts/AGENTS.md) → 해당 작업에 필요한 설정·파트 문서. 일반 개발에서는 스토리 문서를 일괄 열람하지 않는다.
