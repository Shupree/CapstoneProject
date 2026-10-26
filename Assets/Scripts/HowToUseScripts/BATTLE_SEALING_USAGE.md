# 배틀 완료 후 봉인 연출

`Assets/Scenes/Battle_Proto.unity`의 `ExploreProtoMessenger`에 있는 `BattleSealingController`가 2페이즈 완료 이벤트를 받아 진행한다. 기존 완료 문구는 유지하며, 1.5초 뒤 완료 창을 닫고 봉인 화면을 표시한다.

## 진행과 입력

- 메리는 `mary_01`부터 `mary_05`까지 0.2초 간격으로 반복한다. 기존 바탕화면 버튼 네 개는 숨기고 `봉인 프로그램 가동` 버튼을 표시한다.
- 봉인 버튼을 누르면 단색 배경·부적·미나·문양·효과를 담은 전체화면 UI가 미나의 눈 높이에서 가는 띠로 왼쪽부터 오른쪽까지 공개된다. 가로 펼침 완료 후 1초 뒤 Mina_01에서 Mina_02로 눈을 뜬다. 이어 0.75초 대기 후 위아래로 펼쳐져 부적과 전체 화면이 드러나고, 펼침 완료 0.5초 뒤 KeyNote를 표시하며 입력을 받는다. 등장 도중 입력과 진행도 감소는 적용하지 않는다.
- A 또는 D를 새로 누를 때마다 문양이 왼쪽부터 1/30씩 나타난다. 교대 입력을 강제하지 않으며, 키를 누르고 있는 동안 자동으로 횟수가 증가하지 않는다. 같은 프레임에 두 키를 새로 누르면 두 번으로 계산한다.
- 미나 등장 완료 후 입력 가능한 시점부터 0.3초마다 진행도가 1씩 감소하며 문양도 그만큼 줄어든다. 최소값은 0이다. 입력 중에도 감소하며 입력으로 감소 주기를 초기화하지 않는다. 따라서 실제 필요한 총 입력 횟수는 감소한 양만큼 30회보다 많아진다. 완성 이후에는 감소하지 않는다.
- 입력마다 공개된 문양의 오른쪽 경계에 서로 다른 효과 두 개가 나타나 0.5초 동안 픽셀 단위로 사라진다.
- 입력 중 미나는 `Mina_02`~`Mina_04`만 0.2초 간격으로 반복한다. 마지막 입력 후 0.5초가 지나면 현재 프레임에서 멈춘다. 새 입력은 프레임을 초기화하지 않고 재생을 이어가며 정지 시점을 갱신한다.
- 진행도가 30에 도달하면 `Emblem_Fin`으로 바뀌고 미나와 키 안내가 아래로 퇴장한다. 문양은 1.5초 동안 밝기가 50% 증가한 뒤 부적·단색 배경과 함께 0.5초 동안 사라져 메리 화면으로 돌아간다.
- 양쪽 로프 조각이 위에서부터 조립되고, 연결 지점의 조각이 완성되면 작은 부적이 로프 쪽 끝에서 화면 안쪽으로 자란다. 전체 생성 후 0.75초를 기다린 다음 `mary_01`이 픽셀 단위로 사라지며 `CalmMary`가 나타난다.
- CalmMary는 1초 동안 흰 실루엣으로 바뀐다. 가운데에 흰 SDMary를 표시하고 1초 대기한 뒤, CalmMary가 1.5초 동안 SDMary 크기로 줄어들며 사라진다. 다시 1초 대기 후 SDMary의 원래 색상을 1초 동안 복원한다. 이후 왼쪽·오른쪽 로프와 부착된 작은 부적이 각각 왼쪽 위·오른쪽 위로 1초 동안 이동해 사라진다.

## 시간과 이미지 조정

`ExploreProtoMessenger → Battle Sealing Controller`에서 프레임 이미지, 목표 진행도(`Required Presses`), 감소 간격(`Progress Decay Seconds`), 가로 공개 후 눈뜨기 대기(`Heroine Delay Seconds`)와 각 연출 시간을 조정한다. 가로 공개 시간은 `Horizontal Reveal Seconds`(0.35초), 눈뜬 후 대기는 `Eye Open Hold Seconds`(0.75초), 세로 공개 시간은 `Unfold Seconds`(0.35초), 입력 대기는 `Input Delay Seconds`(0.5초)다. 띠 높이와 중심은 `Strip Height`(0.1), `Strip Center`(화면 아래부터 0.45)로 조정한다. 로프 관련 기본값은 조각 조립 시간은 0.16초, 작은 부적 생성 시간은 0.35초, CalmMary 전환 시간은 1초다.

`Rope Pieces`의 각 `Start Seconds`는 로프 연출 시작 기준 시간이다. `Assemble Offset`은 각 조각이 최종 위치에 합쳐지기 전의 상대 위치다. `Small Talismans → Attachment Piece`는 해당 부적이 붙는 로프 조각의 배열 인덱스다.

## 씬 배치 조정

`ExploreProtoCanvas/BattleSealingRoot` 아래에 모든 UI가 저장되어 있다.

| 오브젝트 | 조정 내용 |
| --- | --- |
| `ActivateSealingButton` / `Label` | 봉인 버튼 위치·크기·문구·글꼴 |
| `DesktopVisuals` | 메리와 로프가 표시되는 영역, 하단 작업 표시줄 제외 범위 |
| `DesktopVisuals/Mary`, `CalmMary` | 메리 이미지 배치. Aspect Ratio Fitter가 이미지 비율을 유지함 |
| `SealingScreen` / `SolidBackground` | 전체화면 공개 마스크와 단색 배경 색상 |
| `SealingScreen/TalismanGroup` | 부적 전체 위치·크기 |
| `SealingScreen/TalismanGroup/Talisman`, `Emblem` | 종이와 문양의 상대 배치 |
| `SealingScreen/HeroineGroup/Mina`, `KeyNote` | 미나와 키 안내의 최종 표시 위치·크기 |
| `DesktopVisuals/Ropes/LeftRope`, `RightRope` | 로프 조각과 작은 부적의 위치·크기·회전 |

편집 모드에서 필요한 부모와 자식을 잠시 켜서 배치를 수정하고, `BattleSealingRoot`를 다시 끈 뒤 씬을 저장한다. Play 중 수정은 종료 후 유지되지 않는다. 실행 중 UI를 새로 생성하지 않으며, 위치는 저장된 씬 값을 기준으로 애니메이션한다. 입력 효과 슬롯의 위치는 공개된 문양 경계에 맞추어 계산되므로 크기와 이미지를 주로 조정한다.

`SealingVisual`의 `Pixel Grid`는 픽셀 디졸브의 가로·세로 분할 수다. 값을 줄이면 사라지는 조각이 커진다. `Reveal From Right`는 오른쪽 끝에서부터 공개할 때 사용하며 오른쪽 로프의 작은 부적에 적용되어 있다.

`AgentScripts/AuthorBattleSealing.cs`는 최초 UI 추가용, `AgentScripts/ReframeBattleSealing.cs`는 기존 UI의 전체화면 전환용이며 이미 구성된 씬에서 재실행하지 않는다. 이후 변경은 저장된 씬과 컴포넌트에서 한다.

SD 전환 시간은 Battle Sealing Controller의 `SD 메리 전환 (초)` 항목에서 조정한다. SDMary 위치·크기는 `BattleSealingRoot/DesktopVisuals/SDMary`에서 편집한다. 최초 추가 도구 `AgentScripts/AddSDMaryEnding.cs`는 이미 구성된 씬에서 재실행하지 않는다.
