# 탐색 인터넷 창 사용 안내

대상 씬은 `Assets/Scenes/Explore_Proto.unity`다. 인터넷 화면은 씬에 저장된 uGUI를 사용하며, 방문 기록은 같은 씬 안에서만 유지된다.

## 탐색 동작

- 왼쪽부터 홈(`≪`), 뒤로(`‹`), 앞으로(`›`) 버튼이다.
- 검색 홈에서는 검색창과 결과 목록을 표시한다. 사이트에서는 검색창과 검색 안내를 숨기고 카테고리·제목·넓은 본문 스크롤을 표시한다.
- 홈은 방문 기록 안의 검색 홈으로 이동한다. 검색어와 검색 결과 목록의 스크롤 위치를 보존하며, 앞으로 이동해 방금 보던 사이트를 다시 열 수 있다.
- 뒤로·앞으로는 해당 기록의 검색 상태·사이트·스크롤 위치를 복원한다. 단서 수집 상태는 되돌리지 않으며 본문은 현재 수집 상태와 퇴마 프로그램 상태에 따라 표시된다.
- 뒤로 이동한 뒤 다른 사이트를 열면 기존 앞으로 기록은 삭제된다. 검색어 선택·해제 또는 검색 실행도 현재 검색 홈을 갱신하고 앞으로 기록을 지운다.
- 이동할 기록이 없는 버튼은 회색 이미지로 표시하며 클릭할 수 없다. 홈 화면의 홈 버튼도 비활성화한다.
- 인터넷 창을 닫았다 열어도 방문 기록과 현재 페이지·스크롤을 유지한다. 씬을 다시 시작하면 기록은 초기화한다.
- 본문 하단의 ‘검색 결과로’ 버튼은 사용하지 않는다. `ClueHint`는 비활성화되어 임무·단서 진행 안내를 표시하지 않지만 조사 코드에서 사용하는 참조는 보존한다.

## 편집 위치

- `ExploreProtoCanvas/InternetWindow/BrowserNavigation`: 상단 버튼과 아이콘. 클릭 영역과 아이콘 크기는 별도로 조절할 수 있다.
- `InternetWindow/SearchChrome`: 검색 화면에서만 표시하는 검색창·키워드·검색 안내 묶음.
- `InternetWindow/SiteDetailPanel`: 카테고리·제목·ArticleScroll 배치. 본문 스크롤의 Viewport, Content, Mask, Layout 구성은 유지한다.
- `ExploreProtoMessenger` 오브젝트의 `ExploreBrowserNavigation`: 버튼별 활성/비활성 스프라이트와 검색 UI 참조.
- 아이콘은 `Assets/Sprite/Icon/GoHome_B/G`, `GoBack_B/G`, `GoFront_B/G`를 사용한다. 홈 아이콘은 원본의 두 스프라이트 조각을 원래 오프셋으로 겹쳐 표시한다. 교체 시 각 버튼의 Icons와 Enabled/Disabled Sprites 배열 개수를 맞춘다.

`ExploreProtoMessenger`는 검색 상태와 기존 본문 표시·단서 연동을 담당하고, `ExploreBrowserNavigation`은 방문 기록·탐색 버튼·스크롤 복원을 담당한다. 이 컴포넌트는 Explore_Proto에만 연결되어 있으며 배틀 씬의 기존 인터넷 동작에는 연결하지 않는다. `AgentScripts/AuthorExploreBrowserNavigation.cs`는 최초 편집용으로, 이미 구성된 씬에는 재실행하지 않는다.
