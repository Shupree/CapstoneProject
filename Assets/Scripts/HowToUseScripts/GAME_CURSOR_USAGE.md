# 게임 커서

각 `Assets/Scenes` 씬의 루트 `GameCursor` 오브젝트에 `GameCursorController`를 저장한다. 기본 커서는 `Assets/Sprite/UI/Icon/IconSpriteCursor.png`, 클릭 가능한 일반 버튼은 `IconSpriteFingerPoint.png`를 사용한다. 원본 20×20 픽셀 크기의 네이티브 커서이며 Canvas나 클릭 영역을 생성하지 않는다.

본문 링크, 퇴마 음절과 선택한 음절의 취소 영역, 텍스트 입력칸은 기본 커서를 유지한다. 일반 버튼 안의 자식 텍스트는 버튼의 일부이므로 손가락 커서를 사용한다. 비활성 버튼과 다른 UI에 가려진 대상은 손가락 커서를 표시하지 않는다. 실제 클릭 처리와 진행 조건은 변경하지 않는다.

이미지와 클릭 기준점은 `GameCursor > GameCursorController`에서 편집한다. `Default Hotspot`과 `Pointer Hotspot`은 이미지 왼쪽 위 기준 픽셀 좌표다. 원본 이미지를 교체할 때는 Read/Write 활성화, 압축 없음, Point 필터를 유지한다. 원본 파일을 바꾸면 모든 씬에 반영되며 컴포넌트의 참조나 기준점을 바꾸는 경우 해당 씬마다 저장한다.

특정 클릭 대상은 클릭을 처리하는 오브젝트에 `GameCursorTarget`을 붙여 설정할 수 있다. `Show Pointer`를 끄면 기본 커서, 켜면 손가락 커서를 사용한다. 비활성 버튼에는 이 설정과 관계없이 기본 커서를 사용한다. Battle02의 선택된 음절 슬롯에는 기본 커서 설정이 저장되어 있다.

새 씬을 추가하면 기존 씬의 `GameCursor` 오브젝트를 복사해 하나만 배치한다. Game 화면을 벗어나거나 포커스를 잃거나 Play를 종료하면 운영체제 기본 커서로 복구한다.
