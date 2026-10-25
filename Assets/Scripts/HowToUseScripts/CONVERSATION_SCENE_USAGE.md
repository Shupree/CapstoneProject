# 배경 대화 씬 편집

`Assets/Scenes/Conversation.unity`를 열고 `ConversationCanvas`의 `BackgroundConversationController`를 편집한다.

- **Segments**: 배경별 묶음. 목록 순서대로 진행한다.
- **Label**: 편집용 이름.
- **Background**: `BackgroundIMG` 등의 배경 텍스처.
- **Lines**: 해당 배경에서 표시할 대사 목록. Speaker는 화자, Text는 대사다. 화자를 비우면 이름을 숨긴다.
- **Lines > Show Portrait**: 해당 줄에서 오른쪽 `sprite_01`을 표시한다. 다음 줄에도 유지하려면 그 줄에도 체크한다. Speaker가 `나레이션`이면 이름은 표시하지 않는다.
- **Use Dialogue Box**: 해당 배경 묶음에서 아침 장면용 검은 반투명 대사창을 사용한다.
- **Fade Through Black After**: 해당 묶음 종료 시 암전 후 다음 배경이 나타난다. `ch_1_intro_05`에 적용되어 있다. 나머지 중간 전환은 크로스페이드다.
- **Next Scene Name**: 마지막 대사 이후 이동할 씬. 기본값은 `Explore_Proto`다.
- **Cross Fade Seconds**: 현재 배경 위로 다음 배경이 겹쳐지는 시간. 기본 0.45초이며 중간에 암전하지 않는다.
- **Fade Out Seconds**: 마지막 페이지에서 다음 씬으로 이동하기 전 암전 시간. 기본 0.45초다. 같은 배경 안의 대사는 즉시 넘어가며 페이드 중에는 추가 클릭을 받지 않는다.
- **Fade Out Seconds**는 Fade Through Black After의 암전에도 사용한다. 이후 밝아지는 시간은 Cross Fade Seconds를 사용한다.

같은 배경에 대사를 삽입하려면 해당 묶음의 Lines에 항목을 추가하고 순서를 옮긴다. 새 배경을 삽입하려면 Segments에 항목을 추가하고 이미지와 대사를 한 줄 이상 지정한다. 중간에 빈 묶음을 남기면 진행을 시작하지 않고 연결 오류를 알린다.

화면에는 첫 줄이 자동으로 표시된다. 화면 클릭마다 다음 줄로 이동하고, 묶음의 마지막 줄 다음 클릭에서 다음 배경으로 바뀐다. 전체 마지막 줄 다음 클릭에서는 다음 씬으로 이동한다.

구성은 기존 도입 배경 5장 → 아침 배경 `ch_1_intro2_01`, `ch_1_intro2_02` → 탐색 씬 순서다. 아침 대사창은 `ConversationCanvas/MorningVisuals/MorningDialogueBox`에서 위치·크기·색상과 자식 텍스트를 수정한다. 인물은 같은 부모의 `PortraitSprite01`에서 위치·크기를 수정한다. 두 오브젝트는 초기 비활성이며, 편집 시 잠시 켜서 수정한 뒤 꺼 두면 된다. 인물은 대사창 뒤에 표시되고 화면 아래로 넘어가는 부분은 잘린다.

배경 비율은 유지하며 화면 비율이 다르면 검은 여백이 생긴다. 화면 배치는 저장된 `Background`, `DialogueShade`, `Speaker`, `Dialogue` 오브젝트에서 수정한다. Play 종료 후에도 유지하려면 편집 모드에서 수정하고 씬을 저장한다. 실행 중 UI 오브젝트를 생성하지 않는다.
