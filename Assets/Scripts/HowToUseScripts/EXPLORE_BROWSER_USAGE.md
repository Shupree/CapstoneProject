# 탐색 인터넷 창 사용 안내

대상 씬은 `Assets/Scenes/Explore_Proto.unity`다. 인터넷 화면은 씬에 저장된 uGUI를 사용하며, 방문 기록은 같은 씬 안에서만 유지된다.

## 탐색 동작

- 왼쪽부터 홈(`≪`), 뒤로(`‹`), 앞으로(`›`) 버튼이다.
- 검색 홈에서는 직접 입력하는 검색창과 결과 목록을 표시한다. 각 `Site Entries > Search Terms`와 입력값을 전체 일치로 비교하며 모든 공백과 영문 대소문자를 무시한다. `블러디 메리`는 음료·역사 글 8개, `블러디 메리 강령술`은 후기·질문·일반 안내 8개와 의뢰인 글 1개, 총 9개, `Bloody.org`는 의식 절차 1개를 표시한다. 본문에 나온 단어로 자동 검색하지 않으며 결과는 배열 순서다. 일치하지 않으면 입력한 검색어와 철자 확인 안내를 두 줄로 표시한다. 사이트에서는 검색창과 검색 안내를 숨기고 카테고리·제목·넓은 본문 스크롤을 표시한다.
- 홈은 방문 기록 안의 검색 홈으로 이동한다. 입력한 검색어와 검색 결과 목록의 스크롤 위치를 보존하며, 앞으로 이동해 방금 보던 사이트를 다시 열 수 있다.
- 뒤로·앞으로는 해당 기록의 검색 상태·사이트·스크롤 위치를 복원한다. 단서 수집 상태는 되돌리지 않으며 본문은 현재 수집 상태와 퇴마 프로그램 상태에 따라 표시된다.
- 뒤로 이동한 뒤 다른 사이트를 열면 기존 앞으로 기록은 삭제된다. 검색어를 입력·수정·지우거나 검색을 실행해도 현재 검색 홈을 갱신하고 앞으로 기록을 지운다.
- 이동할 기록이 없는 버튼은 회색 이미지로 표시하며 클릭할 수 없다. 홈 화면의 홈 버튼도 비활성화한다.
- 인터넷 창을 닫았다 열어도 방문 기록과 현재 페이지·스크롤을 유지한다. 씬을 다시 시작하면 기록은 초기화한다.
- 인터넷 창의 네 모서리 또는 네 변을 마우스 왼쪽 버튼으로 드래그하면 크기를 조절한다. 반대쪽 가장자리는 고정되며, 최소 크기는 Canvas 좌표 기준 1700×1000이다. 제목 영역을 드래그하면 창을 이동한다. 같은 씬에서 창을 닫았다 열면 조절한 크기를 유지한다.
- 본문 하단의 ‘검색 결과로’ 버튼은 사용하지 않는다. `ClueHint`는 비활성화되어 임무·단서 진행 안내를 표시하지 않지만 조사 코드에서 사용하는 참조는 보존한다.

## 편집 위치

- `SiteDetailPanel`의 크기와 가장자리 여백이 게시판 표시 영역의 기준이다. `ArticleScroll`과 `Viewport`는 이 영역을 채우고, `ForumContent`는 가로 Stretch·Width 0·Pos X 0으로 맞춘다. `ForumPage`는 고정 폭을 유지하며 이 표시 영역 안에서 중앙 정렬한다. `PageBackground`도 Full Width Backgrounds에 연결하여 흰 배경이 표시 영역을 채운다. 다른 디자인의 일반 게시물은 저장된 Default Scroll Offset을 사용한다.

- 게시판의 맨 위 `ForumPage/HeaderStripe`와 맨 아래 `ForumPage/Footer` 배경은 `ExploreForumArticlePage > Full Width Backgrounds`에 연결되어 게시물 표시 영역의 좌우 끝까지 늘어난다. 높이와 세로 위치는 씬에서 편집하며, 본문·푸터 문구는 기존 고정 크기를 유지한다. 두 배경은 문서와 함께 세로로 스크롤한다. 관련 게시물 표의 보라색 헤더는 본문 폭을 유지한다.

- `블러디 메리는 어떤 칵테일일까?`, `블러디 메리, 주스인 줄 알았는데`, `내 블러디 메리가 너무 매웠던 이유`(Site Entries의 첫 세 글)는 보라색 게시판 디자인을 공유한다. `ExploreProtoMessenger > Forum Article Page`에 연결한 `ExploreForumArticlePage`의 Site Indices는 0·1·2다. 이 글들을 표시할 때 기존 ArticleScroll의 Content를 `ForumContent`로 전환하고, 다른 글에서는 기존 본문 배치를 복원한다. 방문 기록은 같은 ScrollRect의 세로 스크롤 위치를 사용한다.
- 게시판 UI는 `InternetWindow/SiteDetailPanel/ArticleScroll/Viewport/ForumContent/ForumPage`에서 편집한다. 페이지는 1856×2670의 고정 크기이며 세로로 스크롤한다. 창이 넓으면 좌우 여백을 더하고, 좁아지면 여백을 없앤 뒤 Viewport의 Mask로 오른쪽 내용을 가린다. 본문 폭은 1196이고 양옆 공간은 각각 330이다. 오른쪽의 RightSidebarSpacer는 왼쪽 광고와 같은 239×837 크기의 투명 Image이며 Raycast Target을 꺼 입력을 막지 않는다. 텍스트·이미지는 창에 맞춰 축소하거나 재배치하지 않는다. `ForumContent`는 기존 본문과 겹치지 않도록 씬에서 비활성화되어 있으며, 해당 게시물을 열 때 활성화한다.
- `ForumPage/Logo`는 `Assets/Sprite/UI/Internet/ForumLogo.png`를 사용하는 Image다. 회색 본문 이미지·왼쪽 광고·하단 광고·추천 영역은 `ForumDummy.png` 한 장을 재사용한다. 이미지는 각 Image의 Source Image에서 교체한다. 광고와 추천은 현재 표시용 더미이며 클릭 기능은 없다.
- `ForumPage/RelatedRow0`, `RelatedRow1`은 행 전체를 클릭할 수 있는 버튼이다. 각 버튼의 On Click은 `ExploreForumArticlePage.OpenRelatedPost`에 행 번호 0·1을 전달한다. Related Posts 배열에는 버튼·제목·작성자·번호 TMP를 연결한다. 현재 글을 제외한 나머지 두 글을 자동으로 표시하고, 클릭하면 기존 `ExploreBrowserNavigation`으로 이동한다. 새 글은 맨 위부터 표시하며 뒤로·앞으로 이동하면 각 방문의 스크롤 위치를 복원한다. 검색 결과에서 두 번째·세 번째 글을 직접 열어도 같은 디자인을 사용한다.
- 왼쪽 `ForumPage/StickySidebar`는 스크롤 시 Viewport 상단에 닿으면 따라 내려오고 하단 푸터 앞에서 멈춘다. `ExploreForumArticlePage`의 Sticky Top Padding·Sidebar Start·Footer Clearance로 간격을 조정한다. 제목·작성자·본문·댓글과 관련 게시물의 제목·작성자는 기존 Site Entry 원문에서 구분하여 표시하고 원문을 변경하지 않는다. 게시판 이름·날짜·조회수·추천 수·푸터는 씬의 TMP에서 편집한다. 페이지 높이를 바꿀 때는 `ForumContent`와 `ForumPage`의 Height를 함께 조절한다.

- `InternetWindow`의 Image는 `Internet_Window_0` 스프라이트를 Sliced로 표시한다. Sprite Editor의 Border는 원본 픽셀 기준 Left 660 / Bottom 130 / Right 460 / Top 210이며, 제목·우측 버튼·모서리를 포함한 고정 영역을 보존한다. 창 크기는 Scale 대신 RectTransform의 Width/Height로 바꾼다. 테두리 두께는 Image의 Pixels Per Unit Multiplier로 조절하며, 창 크기에 맞춰 이 값을 바꾸지 않는다. 같은 스프라이트를 사용하는 다른 Image에는 Sliced 설정이 자동 적용되지 않는다.
- `InternetWindow/ResizeHandles`에 씬에 저장된 여덟 개의 투명 입력 영역이 있다. 각 `ExploreWindowResizeHandle`의 Window·Direction·Minimum Size를 Inspector에서 편집한다. 최소 크기를 바꿀 때는 여덟 영역에 같은 값을 적용한다. 모서리 입력 영역은 36×36, 변의 두께는 14다. 검색창·닫기 버튼의 높이와 가장자리 여백은 고정하고, 검색 결과·본문 영역은 창에 맞춰 확장한다. Play 진입 시 입력 영역을 새로 생성하지 않는다.

- 배너 UI는 보존되어 있지만 현재 18개 글에는 괴담레스토랑 작성자 문구가 없어 표시하지 않는다. 해당 작성자 글을 배치하면 `Site_Banner_Basic` 배너가 본문과 함께 스크롤된다. 퇴마프로그램을 켜면 눈 기준 커서 방향에 따라 Left/Right/Top/Bottom 이미지를 표시한다. 가로·세로 거리는 배너 크기로 정규화해 비교하며, 눈과 정확히 일치하면 Top을 사용한다.
- `ArticleScroll/Viewport/Content`의 `ExploreArticleBanner`에서 스프라이트와 `Eye Position`을 조정한다. 눈 위치는 이미지 좌하단 (0,0), 우상단 (1,1) 기준이다. `SiteAuthor`, `SiteBannerSlot/SiteBanner`는 씬에 저장된 UI이며 다른 사이트에서는 숨겨진다. 원본 사이트 본문 데이터의 작성자 줄은 유지하고 표시할 때만 분리한다. 배너는 원본 Basic 비율로 본문 너비에 맞춘다.

- `ExploreProtoCanvas/InternetWindow/BrowserNavigation`: 상단 버튼과 아이콘. 클릭 영역과 아이콘 크기는 별도로 조절할 수 있다.
- `InternetWindow/SearchChrome`: 검색 화면에서만 표시하는 검색창·키워드·검색 안내 묶음. 자식 UI는 좌측 상단 고정 앵커를 사용해 창을 줄여도 크기와 간격을 유지한다. 검색 배경은 약 1790×133, 입력 영역은 약 1492×94(Canvas 좌표 기준)다. 창보다 넓어지면 오른쪽을 RectMask2D로 가리며, Padding의 좌우 80은 창 테두리 안쪽 여백이다. 배경 도형·입력 영역·돋보기·지우기 버튼은 각각 RectTransform의 Width/Height와 위치로 편집한다.
- `SearchChrome/SearchInputField`: 검정색으로 깜빡이는 커서를 가진 TMP 입력창. `ExploreProtoMessenger`의 Search Input에 연결한다. 글별 검색어는 `Site Entries > Search Terms`에서 편집한다. 상위 `Searchable Terms`는 글별 검색어가 없는 기존 씬용 호환 설정이다.
- `ExploreProtoMessenger > Site Entries`: 18개 글의 분류·제목·요약·본문·검색어·결과 버튼. 검색 결과의 Category/Title/Summary 텍스트는 연결된 씬 버튼 안에서 별도로 편집한다. 버튼은 씬에 미리 저장되어 있으며 검색 시 활성 상태만 바뀐다.
- 게시물 원고는 [SearchPosts.md](../Story_Parts/Chapter1_BloodyMary/SearchPosts.md)다. 강령술 결과 2·3·6번 본문만 Bloody.org를 언급한다. 일반 검색 8개와 강령술 검색의 첫 8개에는 단서 링크가 없으며, 9번 의뢰인 글 본문에는 `mary_comment` 링크가 있다. Bloody.org에는 기존 배우자·칼·금기·숨겨진 문구 단서 링크가 있다.
- Bloody.org 본문의 `<veil>일반 문구<revealed>숨겨진 문구</revealed></veil>`가 프로그램 상태에 따라 교체된다. OFF는 `메리의 얼굴을 보게 될 것`, ON은 `메리가 당신을 용서하지 않을 것이다.`다. 숨겨진 문구의 `mary_curse` 링크는 ON일 때만 노출되며 수집 메모도 같은 문장이다.
- 의뢰인 글의 `Open Dialogue Scene Id`는 `Explore_ClientPostOpened`, Investigation의 `Evidence Dialogue Scene Id`는 `Explore_ClientPostEvidence`다. 기존 조사 독백 CSV에 연결된다. 첫 방문은 작성자 독백만 표시하며 본문 첫 클릭이 `mary_comment` 수집·클릭 독백·질문 조건 갱신을 처리한다. 각각 같은 씬에서 한 번만 표시한다. 친구 상황·부작용 질문까지 마치면 전말 질문과 두 번째 통화를 연다.
- Bloody.org의 `<vision>` 안에 유의사항 3번과 `mary_weakness` 링크를 배치한다. 프로그램 ON일 때만 ‘메리는 '네 아기는 죽지 않았어' 라는 말에 희망을 갖는다’가 보이며, 클릭하면 같은 문장을 메모에 기록하고 필수 단서를 수집한다. OFF에서는 번호·문장·링크가 모두 숨겨진다. 수집 상태는 유지한다. 필수 단서 5개와 두 번째 통화 완료가 밤·배틀 진입 조건이다.
- `InternetWindow/SiteDetailPanel`: 카테고리·제목·ArticleScroll 배치. 본문 스크롤의 Viewport, Content, Mask, Layout 구성은 유지한다.
- `ExploreProtoMessenger` 오브젝트의 `ExploreBrowserNavigation`: 버튼별 활성/비활성 스프라이트와 검색 UI 참조.
- 아이콘은 `Assets/Sprite/Icon/GoHome_B/G`, `GoBack_B/G`, `GoFront_B/G`를 사용한다. 홈 아이콘은 원본의 두 스프라이트 조각을 원래 오프셋으로 겹쳐 표시한다. 교체 시 각 버튼의 Icons와 Enabled/Disabled Sprites 배열 개수를 맞춘다.

`ExploreProtoMessenger`는 검색 상태와 기존 본문 표시·단서 연동을 담당하고, `ExploreBrowserNavigation`은 방문 기록·탐색 버튼·스크롤 복원을 담당한다. 이 컴포넌트는 Explore_Proto에만 연결되어 있으며 배틀 씬의 기존 인터넷 동작에는 연결하지 않는다. `AgentScripts/AuthorExploreBrowserNavigation.cs`는 최초 편집용으로, 이미 구성된 씬에는 재실행하지 않는다.
