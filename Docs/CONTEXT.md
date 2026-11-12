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
| 현재 탐색에서 진입하는 배틀 씬 | `Assets/Scenes/Battle02_Proto.unity` |
| 기존 배틀 씬 | `Assets/Scenes/Battle_Proto.unity` |
| 배틀 완료 후 봉인 연출 | `Assets/Explore_Proto/Scripts/BattleSealingController.cs`, 씬의 `BattleSealingRoot` |
| 탐색·통화·씬 연결·배틀 진행 | `Assets/Explore_Proto/Scripts` |
| CSV 리더 | `Assets/Scripts/DialogueReader/DialogueCsvReader.cs` |
| 하단 대화창 프리팹 | `Assets/Prefabs/BottomDialogueSystem.prefab` |
| 화상통화 카드 프리팹 | `Assets/Prefab/BloodyMaryCall` |
| 기존 민서 의뢰 프로토타입 | `Assets/Scenes/CyberExorcistPrototype.unity`, `Assets/CyberExorcist` |

- `Explore_Proto`의 진행 코드는 필수 단서 5개·선택 단서 1개와 단서별 메신저 질문을 사용한다. 닉네임 단서와 친구 상황·부작용 질문 완료로 전말 질문과 두 번째 통화를 열고, 필수 단서 5개와 두 번째 통화 완료 후 밤·`Battle02_Proto`로 진행한다. 현재 검색 자료는 18개다. 강령술 검색 9번 의뢰인 글의 첫 입장에서 작성자 독백, 본문 첫 클릭에서 관심사 독백과 `mary_comment` 수집을 처리한다. Bloody.org에서 퇴마 프로그램을 켜면 유의사항 3번 ‘메리는 '네 아기는 죽지 않았어' 라는 말에 희망을 갖는다’가 나타나며, 클릭하면 `mary_weakness`를 수집한다. `Battle02_Proto`에서 새 단어 기믹을 실행하며, 기존 `Battle_Proto`는 탐색 진행에서 호출하지 않는다.
- `Battle02_Proto`는 기존 유미나·이서연 통화와 메리의 첫 등장까지 `BattleProtoConversation`·`BattleMaryIntroController`의 연출을 유지한다. 메리와 처음 조우한 뒤에는 예전 카드 1페이즈 대신 `Battle02RitualController`를 시작한다. `Battle_Proto`는 기존 카드 전투를 보존한다.
- `Battle02_Proto`에서는 메리 영상이 5초마다 1초간 Noise 사운드와 함께 일그러진다. 칼을 든 메리는 기믹 시작·재시도 후 최초 1회에 한해 1초 뒤 등장한다. 이후 첫 회차는 8~14초 간격이며 회차마다 등장 간격이 1초씩 짧아진다. 메리 이미지는 씬에서 편집한 고정 크기로 표시한다. 칼 든 모습은 A 1.6초 → B 1.2초 → C 1.2초로 바뀌며, 합계 4초 안에 통화 종료를 누르지 않으면 현재 게임 화면 전체에 Hand_Animation.aseprite의 원본 캔버스를 맞춰 애니메이션을 한 번 재생한 뒤 클립 종료와 함께 SignalLost 화면을 표시한다. 단계별 시간은 Battle02RitualController Inspector에서 조절한다. 첫 조우부터 보이는 통화 종료 버튼은 기믹 시작 후 사용 가능하며 클릭 간격은 0.5초다. Button_Messenger로 여는 Messenger_Dark 창에서 고정된 붉은 단어를 골라 ‘네 아기는 죽지 않았어’를 다섯 번 완성한다. 4·5회는 Text_Baby·Text_Face 이미지를 사용한다. 페이크 문구는 눌러도 반응이 없고, 붉은 문구를 가리는 글리치는 회차가 진행될수록 더 자주·여러 곳에 발생한다. 문구 글리치는 Glitch_R_Sheet, 메리 영상 글리치는 Glitch_Sheet의 각 5종을 섞어 0.06초 간격으로 교대한다. 메리가 칼을 들지 않았을 때 통화 종료를 누르면 Broken_Window_Sheet의 3단계 균열이 통화창 안쪽에 누적된다. 오입력 노이즈 연출·사운드와 기믹 초기화는 사용하지 않는다. 3회째에는 통화 종료와 문구 선택이 잠기고 메신저가 닫히며, 다음 메리 공격을 막지 못해 게임오버가 된다. 회차를 넘어 횟수와 균열을 유지하고 재시도 시 초기화한다. [단어 기믹 편집 안내](../Assets/Scripts/HowToUseScripts/BATTLE02_RITUAL_USAGE.md).
- 이전 인터넷 이미지·어절 수집 코드가 남아 있으므로 현재 활성 흐름은 씬의 연결을 기준으로 확인한다. 저장·불러오기는 미구현이다.

- `Battle02_Proto`의 다섯 번째 문장 완료 후 `BloodyMaryPhaseTwoController.CompleteFromExternal()`을 통해 `BattleSealingController`가 완료 화면을 1.5초 유지한 뒤 창 전체를 0.75초 동안 페이드 아웃하고, 창을 닫은 다음 메리 반복 이미지와 봉인 버튼을 표시한다. 버튼 선택 후 단색 배경의 전체화면 봉인 UI를 눈 높이의 가는 띠로 왼쪽부터 공개한다. 1초 뒤 미나가 눈을 뜨고, 0.75초 뒤 화면을 위아래로 펼친 다음 0.5초 뒤 키 안내와 입력을 활성화한다. 타이핑은 Mina_02~04만 반복한다. A/D 입력으로 진행도 30을 채워 문양을 완성하며, 입력 가능 상태에서 0.3초마다 진행도가 1씩 감소한다(최소 0). 문양 발광·퇴장, 양쪽 로프와 작은 부적 생성 이후 픽셀 디졸브로 CalmMary를 표시한다. 이어 흰 실루엣·축소 연출로 중앙 SDMary로 전환하고 원래 색상을 복원한 뒤 양쪽 로프가 위 모서리로 퇴장한다. 전체화면 입력 연출 종료 후 바탕화면 버튼 네 개가 복원된다. SDMary와 타이핑된 mary.exe가 폴더에 들어가고 저장 팝업을 표시한다. 확인하면 현재 화면이 한 번의 픽셀 디졸브로 사라지면서 뒤의 초기 테마 두 사람 화상통화 UI가 드러나며 도입 대사나 배틀은 재시작하지 않는다. 배치와 연출 시간은 씬·Inspector에서 조정한다.

- 탐색 메신저는 ‘말을 건다.’ 선택 → 인사 3개 → ‘화상통화를 연결한다.’ 선택으로 진행한다. 통화는 좌측 아이콘까지 덮는 전체화면 `EmbeddedVideoCall`에서 진행되며 하단 작업 표시줄은 남는다. 탐색 통화는 CallChat_Card 프레임의 검정 Tint를 기본으로 사용하고 현재 화자만 원본 초록색으로 표시한다. 중앙에는 Uncall_Icon 장식이 있으며 중단 기능은 연결하지 않는다. 배틀은 기존 SpeakingBorder 강조 방식을 유지한다. 하단 대화 바는 BottomDialogueSystem을 사용한다. 통화의 Connected Controls는 선택 사항이며, Explore_Proto에서는 제거된 상태다. 해당 UI가 없어도 연결 연출·대화 시작·통화 종료가 진행된다. 첫 통화와 두 번째 통화 모두 EnterCall → WaitCall 2.5초 → EnterCall 순서로 연결하고, 두 번째 EnterCall이 시작할 때 이서연의 이미지를 표시한다. 효과음 종료 뒤 `BottomDialogueSystem`의 대화를 시작한다. `Explore_FirstCall.csv`의 `AcquireBloodyMary` 이벤트 줄에서 키워드를 자동 획득하며, 종료 후 별도 획득 버튼 없이 메신저로 복귀한다. 나레이션은 화자 이름을 비운다. 연결 연출은 `VideoCallConnectionSequence`를 사용한다.

- 탐색 인터넷 창은 9슬라이스 이미지로 테두리·제목·버튼 그림 크기를 유지하며, 네 변과 모서리 드래그로 최소 1700×1000까지 크기를 조절한다. 탐색 인터넷은 검색 홈과 사이트 본문을 구분한다. 검색창 도형·입력 영역·아이콘은 고정 크기를 유지하며 좁은 창에서는 오른쪽을 마스크로 가린다. 일반 검색의 첫 세 글은 씬에 저장된 1856×2670 고정 크기 게시판과 스크롤을 따르는 왼쪽 광고를 공유하며, 관련 게시물 두 행으로 서로 이동한다. 오른쪽에는 광고와 같은 크기의 투명 영역을 두어 본문을 가운데에 배치한다. 게시판의 스크롤 영역은 씬에서 편집한 SiteDetailPanel을 채우며, 흰 배경·맨 위 보라색 띠·맨 아래 푸터 배경도 이 표시 영역의 좌우 끝까지 늘어난다. 좁은 창에서는 여백을 줄인 뒤 내용을 마스크로 가린다. `Site Entries`의 글별 검색어와 입력값을 공백·영문 대소문자를 무시하고 전체 일치로 비교한다. `블러디 메리`는 음료·역사 8개, `블러디 메리 강령술`은 후기·질문·안내 8개와 의뢰인 증거 글 1개(2·3·6번 본문에 Bloody.org 언급), `Bloody.org`는 의식 절차 1개를 표시한다. UI는 씬에 저장된 18개 결과 버튼을 필터링하며 재생성하지 않는다. `ExploreBrowserNavigation`이 입력값·결과 목록·홈·뒤로·앞으로·스크롤을 복원하고 창을 닫아도 같은 씬의 기록을 유지한다. 메모 창은 단서 정보와 자유 메모를 전환한다. [인터넷 창 사용 안내](../Assets/Scripts/HowToUseScripts/EXPLORE_BROWSER_USAGE.md), [게시물 원고](../Assets/Scripts/Story_Parts/Chapter1_BloodyMary/SearchPosts.md).

## 필요한 작업에서만 읽을 문서

- 배경 대화·이미지 추가: [Conversation 씬 사용 안내](../Assets/Scripts/HowToUseScripts/CONVERSATION_SCENE_USAGE.md). 배경별 대사 묶음을 클릭으로 진행하고 마지막에는 탐색 씬으로 이동한다.
- 배틀 봉인 연출·시간·배치: [봉인 연출 사용 안내](../Assets/Scripts/HowToUseScripts/BATTLE_SEALING_USAGE.md).

Bloody.org의 목록형 의식 안내에서 `ExploreProtoArticleLinks`가 `<veil>` 문구를 교체한다. 프로그램 OFF는 ‘메리의 얼굴을 보게 될 것’, ON은 ‘메리가 당신을 용서하지 않을 것이다.’다. ON에서만 `mary_curse` 단서를 수집할 수 있고 같은 문장을 메모에 기록한다. 기존 `RevealedArticleLayer`는 사용하지 않는다.

- CSV 데이터 연동: [대사 CSV 사용 가이드](../Assets/Scripts/HowToUseScripts/DIALOGUE_CSV_GUIDE.md)
- 하단 대화창: [사용 안내](../Assets/Scripts/HowToUseScripts/BOTTOM_DIALOGUE_USAGE.txt)
- 화상통화 화면·연결음: [사용 안내](../Assets/Scripts/HowToUseScripts/VIDEO_CALL_USAGE.md)
- 기존 민서 프로토타입 UI: [UI 편집 안내](UI_EDITING.md)
- 스토리·대사 작성: [스토리 작업 규칙](../Assets/Scripts/AGENTS.md) → 해당 작업에 필요한 설정·파트 문서. 일반 개발에서는 스토리 문서를 일괄 열람하지 않는다.
