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
| 배경 대화 도입 씬 | `Assets/Scenes/Conversation.unity`, `Assets/Explore_Proto/Scripts/BackgroundConversationController.cs` |
| 탐색 씬 | `Assets/Scenes/Explore_Proto.unity` |
| 배틀 씬 | `Assets/Scenes/Battle_Proto.unity` |
| 배틀 완료 후 봉인 연출 | `Assets/Explore_Proto/Scripts/BattleSealingController.cs`, 씬의 `BattleSealingRoot` |
| 탐색·통화·씬 연결·배틀 진행 | `Assets/Explore_Proto/Scripts` |
| CSV 리더 | `Assets/Scripts/DialogueReader/DialogueCsvReader.cs` |
| 하단 대화창 프리팹 | `Assets/Prefabs/BottomDialogueSystem.prefab` |
| 화상통화 카드 프리팹 | `Assets/Prefab/BloodyMaryCall` |
| 기존 민서 의뢰 프로토타입 | `Assets/Scenes/CyberExorcistPrototype.unity`, `Assets/CyberExorcist` |

- 현재 `Explore_Proto`는 메일·첫 통화 이후 필수 단서 5개와 선택 단서 1개를 조사한다. `ExploreResearchQuestions`가 수집 단서별 메신저 질문을 표시하며, 닉네임 단서와 친구 상황·부작용 질문 완료로 전말 질문과 두 번째 통화를 연다. 닉네임 단서는 독백만 표시하고 메모에서는 제외한다. 필수 단서 5개와 두 번째 통화를 마치면 ‘밤까지 기다린다.’가 나타난다. 검은 화면으로 시간을 넘긴 뒤 메신저 대화 4줄과 ‘강령술을 시작한다.’ 선택지를 거쳐 배틀로 이동한다. 선택 단서는 진행 조건에서 제외된다. `Battle_Proto` 단독 실행은 조사 완료 상태의 화상통화부터 시작한다.
- 배틀 통화는 유미나·이서연 2인이다. 배틀 씬이 활성 씬으로 시작할 때 준비 메신저 없이 전체화면 통화를 열고 EnterCall을 두 번 차례로 재생한다. 두 번째 EnterCall이 시작할 때 이서연의 이미지를 표시하고 효과음 종료 뒤 대화를 시작한다. `BattleProtoConversation`의 Element 10 이후 `BattleMaryIntroController`가 전체 화면 가로 지직거림·CRT 암전, 종소리와 주인공·캠 프레임 등장, 딸깍 소리와 메리 등장을 순서대로 진행한다. 빈 프레임 9장이 기존 인물을 덮은 뒤 손이 다가오고, 칼끝이 닿는 손 왼쪽 프레임에 칼 든 메리가 먼저 나타난다. 손이 2초간 사라진 뒤 나머지 얼굴이 차례로 채워지고 같은 배치로 1페이즈가 시작된다. 시간은 해당 컴포넌트 Inspector에서 조절한다. 전체 화면 효과는 `BattleInterferenceRendererFeature`와 카메라의 `BattleScreenInterference`를 사용한다.
- 현재 배틀 1페이즈는 화상통화 카드 퍼즐이다. 9장/칼 1개 → 12장/칼 3개 → 18장/칼 5개, 각 파트 전체 표시 후 10초 제한을 적용한다. 2·3파트의 카드는 처음부터 0.15초 고정 간격으로 등장한다. 이후 2페이즈에서는 붉은 문구에서 음절을 골라 문장을 완성한다. 1파트 문구는 입력창 위에서 생성되고 3.4초 간격으로 바뀐다. 2파트의 사람 형상은 `Assets/Sprite/ProtoType/Phase2_Part2.png` 한 장으로 표시하며 씬에 저장된 클릭 영역으로 음절을 선택한다.
- 이전 인터넷 이미지·어절 수집 코드가 남아 있으므로 현재 활성 흐름은 씬의 연결을 기준으로 확인한다. 저장·불러오기는 미구현이다.

- 배틀 2페이즈 완료 후 `BattleSealingController`가 1.5초 뒤 완료 창을 닫고 메리 반복 이미지와 봉인 버튼을 표시한다. 버튼 선택 후 단색 배경의 전체화면 봉인 UI를 눈 높이의 가는 띠로 왼쪽부터 공개한다. 1초 뒤 미나가 눈을 뜨고, 0.75초 뒤 화면을 위아래로 펼친 다음 0.5초 뒤 키 안내와 입력을 활성화한다. 타이핑은 Mina_02~04만 반복한다. A/D 입력으로 진행도 30을 채워 문양을 완성하며, 입력 가능 상태에서 0.3초마다 진행도가 1씩 감소한다(최소 0). 문양 발광·퇴장, 양쪽 로프와 작은 부적 생성 이후 픽셀 디졸브로 CalmMary를 표시한다. 이어 흰 실루엣·축소 연출로 중앙 SDMary로 전환하고 원래 색상을 복원한 뒤 양쪽 로프가 위 모서리로 퇴장한다. 전체화면 입력 연출 종료 후 바탕화면 버튼 네 개가 복원된다. SDMary와 타이핑된 mary.exe가 폴더에 들어가고 저장 팝업을 표시한다. 확인하면 픽셀 디졸브로 초기 테마의 두 사람 화상통화 UI로 돌아가며 도입 대사나 배틀은 재시작하지 않는다. 배치와 연출 시간은 씬·Inspector에서 조정한다.

- 탐색 메신저는 ‘말을 건다.’ 선택 → 인사 3개 → ‘화상통화를 연결한다.’ 선택으로 진행한다. 통화는 좌측 아이콘까지 덮는 전체화면 `EmbeddedVideoCall`에서 진행되며 하단 작업 표시줄은 남는다. 탐색·배틀 시작 통화는 어두운 창 배경, 같은 인물 배치와 하단 `BottomDialogueSystem` 대화 바를 사용하고 현재 화자 카드에 초록색 테두리를 표시한다. 두 씬의 통화 카드 아래에는 같은 위치에 기능 연결 전의 임시 조작 버튼이 있다. 첫 통화와 두 번째 통화 모두 EnterCall → WaitCall 2.5초 → EnterCall 순서로 연결하고, 두 번째 EnterCall이 시작할 때 이서연의 이미지를 표시한다. 효과음 종료 뒤 `BottomDialogueSystem`의 대화를 시작한다. `Explore_FirstCall.csv`의 `AcquireBloodyMary` 이벤트 줄에서 키워드를 자동 획득하며, 종료 후 별도 획득 버튼 없이 메신저로 복귀한다. 나레이션은 화자 이름을 비운다. 연결 연출은 `VideoCallConnectionSequence`를 사용한다.

- 탐색 인터넷은 검색 홈과 사이트 본문을 구분한다. 본문에서는 검색 UI를 숨기고 스크롤 영역을 넓게 사용한다. `ExploreBrowserNavigation`이 홈·뒤로·앞으로 기록과 페이지별 스크롤을 관리하며, 이동 불가 버튼은 회색으로 비활성화한다. 창을 닫아도 같은 씬의 기록을 유지한다. 본문 하단 목표 안내와 기존 검색 결과 복귀 버튼은 표시하지 않는다. [인터넷 창 사용 안내](../Assets/Scripts/HowToUseScripts/EXPLORE_BROWSER_USAGE.md).

## 필요한 작업에서만 읽을 문서

- 배경 대화·이미지 추가: [Conversation 씬 사용 안내](../Assets/Scripts/HowToUseScripts/CONVERSATION_SCENE_USAGE.md). 배경별 대사 묶음을 클릭으로 진행하고 마지막에는 탐색 씬으로 이동한다.
- 배틀 봉인 연출·시간·배치: [봉인 연출 사용 안내](../Assets/Scripts/HowToUseScripts/BATTLE_SEALING_USAGE.md).

퇴마 프로그램을 켜면 `ExploreProtoArticleLinks`가 주의 문단의 ‘이상한 화면과 소리가 나오니 주의하세요.’만 ‘메리가    당신을    붙잡습니다.’로 교체한다. 본문과 같은 글자 크기를 사용하며 앞뒤 문장은 유지한다. 끄면 원문이 돌아온다. 기존 `RevealedArticleLayer`는 사용하지 않는다.

- CSV 데이터 연동: [대사 CSV 사용 가이드](../Assets/Scripts/HowToUseScripts/DIALOGUE_CSV_GUIDE.md)
- 하단 대화창: [사용 안내](../Assets/Scripts/HowToUseScripts/BOTTOM_DIALOGUE_USAGE.txt)
- 화상통화 화면·연결음: [사용 안내](../Assets/Scripts/HowToUseScripts/VIDEO_CALL_USAGE.md)
- 기존 민서 프로토타입 UI: [UI 편집 안내](UI_EDITING.md)
- 스토리·대사 작성: [스토리 작업 규칙](../Assets/Scripts/AGENTS.md) → 해당 작업에 필요한 설정·파트 문서. 일반 개발에서는 스토리 문서를 일괄 열람하지 않는다.
