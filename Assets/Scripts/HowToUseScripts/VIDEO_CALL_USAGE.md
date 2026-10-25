# 화상통화 화면과 연결 연출

## 화면 배치

`Explore_Proto`와 `Battle_Proto`의 통화 화면은 `ExploreProtoCanvas` 바로 아래에 저장된 uGUI 오브젝트를 사용한다. `RectTransform`의 앵커는 `(0, 0)`부터 `(1, 1)`까지, 좌우·상단 여백은 `0`, 하단 여백은 `190`이다. 이 범위는 좌측 앱 아이콘까지 덮고 하단 작업 표시줄은 남긴다. 새 통화 창에도 같은 앵커와 여백을 적용한다. 실행 중에 창 UI를 생성하거나 크기를 다시 설정하지 않는다.

- 탐색 통화: `EmbeddedVideoCall`. 메신저 창의 자식이 아니라 Canvas의 자식이다. 첫 통화와 진실 확인 통화가 공유한다. 배틀 시작 통화와 같은 어두운 창 배경과 헤더를 사용한다.
- 탐색 `EmbeddedVideoCall`과 배틀 시작 `VideoCallWindow`의 `ConnectedControls`는 같은 앵커·위치에 저장되어 있다. 화면·음소거·종료·카메라·채팅 버튼은 캐릭터 카드 아래, 창 하단 테두리 안쪽에 놓인 임시 UI이며 통화 중 표시된다. 현재 클릭 기능은 연결되어 있지 않고 하단 대화 바에 가릴 수 있다.
- 탐색 씬의 기존 `VideoCallWindow`, 배틀 대화용 `VideoCallWindow`, 배틀 기믹용 `BattleCallWindow`도 같은 화면 범위를 사용한다.
- 탐색과 배틀 시작 통화는 같은 크기와 위치의 인물 카드 및 `BottomDialogueSystem` 하단 대화 바를 사용한다. 배틀 씬의 기존 `DialogueBox`는 비활성화되어 있다. `VideoCallPortraits`의 화자 테두리 참조와 화자 ID를 연결하면 현재 화자의 카드에 초록색 테두리가 표시된다. 쉼표로 구분한 공동 화자는 두 카드 모두 강조한다.

## 연결 효과음

`Assets/Explore_Proto/Scripts/VideoCallConnectionSequence.cs`는 씬에 저장된 통화 UI와 `AudioSource`를 참조한다. 새 통화에는 안정적으로 활성 상태인 오브젝트에 이 컴포넌트와 전용 `AudioSource`를 붙이고 `EnterCall`, `WaitCall`, 상대 캐릭터의 **스프라이트 오브젝트**, 필요하다면 연결 중 표시 오브젝트를 Inspector에서 지정한다. 상대 영상 칸 전체를 숨길 필요는 없다.

`TryBegin(mode, onConnected)`가 `true`이면 효과음이 끝난 후 `onConnected`를 한 번 호출한다. 이 콜백에서 대화를 시작한다. 연결 중에는 대사 표시와 다음 버튼을 비활성화한다. 창을 닫거나 연결을 중단할 때는 `Cancel()`을 호출한다.

| 모드 | 효과음 순서 | 현재 사용처 |
| --- | --- | --- |
| `Incoming` | EnterCall 한 번 | 이후 수신 통화에 사용 가능 |
| `Outgoing` | EnterCall → WaitCall 2.5초 → EnterCall | 탐색 첫 통화·진실 확인 통화 |
| `UrgentOutgoing` | EnterCall 두 번 연속 | 배틀 시작 통화 |

`Outgoing`의 대기 시간은 `waitCallSeconds`에서 조정한다. 대기음은 지정 시간 동안 재생하고 끝나면 멈춘다. 상대 스프라이트는 연결 중에 숨기고, 상대가 입장하는 EnterCall의 시작과 거의 동시에 표시한다. 대화는 마지막 EnterCall이 끝난 뒤 시작한다. 새로운 통화도 위 세 모드 중 상황에 맞는 것을 선택하거나 호출부에서 새 모드를 추가한다.

현재 탐색 호출부는 `ExploreProtoMessenger`, 배틀 호출부는 `BattleProtoConversation`이다. 씬에 새 통화 화면을 추가할 때에는 위 컴포넌트의 참조와 대화 시작 콜백을 함께 연결한다.
탐색에서 추가 통화를 시작할 때는 `StartAdditionalCall(sceneId, reader, mode)`로 연결 방식을 선택할 수 있으며, 모드를 생략하면 `Outgoing`을 사용한다.
