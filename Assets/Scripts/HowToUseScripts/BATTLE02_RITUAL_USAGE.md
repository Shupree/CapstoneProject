# Battle02 단어 기믹 편집

`Battle02_Proto`의 `ExploreProtoMessenger`에 있는 `Battle02RitualController`가 메리와 첫 조우한 뒤 새 기믹을 진행한다. 기존 `Battle_Proto`는 이 컴포넌트를 연결하지 않아 이전 전투를 유지한다. 이 씬의 UI는 `ExploreProtoCanvas`에 저장되어 있으며 실행 중 새로 만들지 않는다.

## 씬 구성

- `BattleCallWindow/Battle02HangupButton`: 첫 조우부터 스프라이트 아래에 표시되며, 기믹 시작 전에는 누를 수 없다. 칼을 든 메리가 보일 때 누르면 모습과 공격 제한시간을 초기화한다. 칼을 들지 않았을 때 누르면 통화창 균열이 1→2→3단계로 진행한다. 오입력은 회차를 넘어 누적되며 회차·선택 단어는 초기화하지 않는다. 3회째에는 통화 종료 버튼만 잠긴다. 메신저를 강제로 닫지 않으며 열기·닫기, 음절 선택·취소와 문장 완성을 계속할 수 있다. 다음 회차에도 통화 종료 잠금은 유지한다. 재시도하면 횟수와 균열이 초기화된다. 오입력 노이즈 화면과 효과음은 사용하지 않고, 균열이 늘어날 때 Break_Glass를 재생한다. 클릭 후 0.5초 동안 다시 누를 수 없다. 메신저를 열고 닫아도 씬에 저장된 기본 위치를 유지한다.
- `Battle02DarkMessenger`: 하단 `Button_Messenger`나 창의 X로 열고 닫는다. 닫았다 다시 열어도 선택한 단어는 남는다.
- `RoundWordsGroup/Round_1`~`Round_3`: 기존 1·3·5회차의 문구 위치와 배경을 사용한다. 세 번째 회차는 TextFaceImage를 유지하며 6개 문구를 위한 두 배치를 추가했다. 기존 FakePhrase 오브젝트는 비활성으로 남겨두며 현재 기믹에는 표시하지 않는다.
- `SelectedWords`: 선택한 음절 10개를 순서대로 보여준다. 잘못 고른 음절은 붉은색이며, 이동이 끝난 뒤 누르면 제거되어 원래 위치에 복구된다. 단어 경계는 슬롯 간격으로 표시한다.

각 `Battle02WordSource.word`는 실제 선택되는 한 음절이다. 보이는 문구를 바꿀 때 이 값과 위치를 함께 확인한다. 현재 정답은 코드의 `Answer` 배열에 있는 `네 / 아 / 이 / 가 / 기 / 다 / 리 / 고 / 있 / 어`이며 세 회차가 같은 문장을 사용한다. 각 회차에는 정답 음절 10종이 적어도 하나씩 있어야 한다. 회차가 끝나면 창이 닫히고, 다음 회차는 플레이어가 메신저 아이콘으로 직접 연다.

선택한 음절은 원문에서 사라지고 다음 문장 칸으로 곡선을 그리며 이동한다. `Syllable Move Seconds`(기본 0.2초)와 `Syllable Move Arc`(기본 40)로 시간과 곡선 높이를 조절한다. 연속 선택은 가능하며 이동 중에는 선택 취소를 잠시 막는다. 마지막 음절 도착 후 정답을 판정하며 이동 중에도 메리 공격 제한시간은 진행된다. 메신저를 닫거나 게임오버가 되면 이동 중인 음절만 원문에 복구하고, 도착한 음절은 유지한다. 재시도 시 모든 선택을 초기화한다. 음절 버튼과 목적지 10칸은 씬에 저장하며 실행 중 생성하지 않는다.

글리치는 현재 회차의 `glitchTargets`에 연결된 문구 위치와 크기를 따라가며, 1회부터 3회까지 동시에 영향을 받는 문구 수와 발생 빈도가 증가한다. 메리의 일반·칼 스프라이트와 Noise는 컨트롤러 Inspector 참조로 교체할 수 있다. 창·얼굴 이미지도 씬의 `RawImage` 참조를 바꾸면 된다. 타이밍은 같은 컴포넌트의 `시간` 항목에서 조절한다. 세 번째 완료는 `BloodyMaryPhaseTwoController.CompleteFromExternal()`을 통해 기존 봉인 연출에 연결된다.

## 글리치 이미지

`Mary Glitch Frames`는 `Assets/Sprite/Effect/Glitch_Sheet.png`의 5종, `Word Glitch Frames`는 `Glitch_R_Sheet.png`의 5종을 사용한다. 효과가 발생할 때 순서를 섞고 `Glitch Frame Seconds`(기본 0.06초)마다 교대한다. 동시에 나타나는 문구 글리치는 서로 다른 프레임부터 시작한다. 기존 RawImage가 각 Sprite의 분할 영역을 표시하므로 이 두 시트는 Sprite Atlas에 넣지 않고 사용한다. 문구 선택 제한과 회차별 발생 빈도, 메리의 5초 주기·1초 지속시간은 그대로 유지한다. 통화 종료 오입력은 아래 창 파손 설정을 사용한다.

문구 글리치는 Sprite의 투명 여백을 제외한 영역을 표시한다. `Word Glitch Size Multiplier`의 X/Y로 문구 대비 가로·세로 배율(기본 1.15/1.2)을, `Word Glitch Padding`으로 추가 크기(기본 24/12)를 조절한다. 크기는 `문구 RectTransform 크기 × 배율 + 추가 크기`다. 영구 적용하려면 Play를 종료하고 Inspector에서 값을 편집한 뒤 씬을 저장한다. 메리 글리치의 크기는 이 설정에 영향을 받지 않는다.

## 메리 공격 이미지와 시간
Battle02_Proto의 ExploreProtoMessenger 오브젝트 > Battle02RitualController에서 모습 A/B/C 표시 시간(초)을 조절한다. 기본값은 1.6 / 1.2 / 1.2초이며 모습을 보여주는 시간의 합계는 4초다. 각 전환의 암전 시간이 별도로 추가되어 기본 공격 제한시간은 5.5초다. 공격 시작 때 세 시간을 확정하므로 Play 중 변경한 값은 다음 공격부터 적용된다. Normal Mary는 시트 _0, Knife Mary는 _1(A), Knife Mary B는 _2(B), Knife Mary C는 _3(C)에 연결한다. 통화 종료로 공격을 막거나 회차가 끝나면 평상시로 복귀하며 다음 공격은 A부터 시작한다.

메리 Artwork는 씬에서 편집한 RectTransform 크기와 배치를 그대로 사용하고 Sprite만 교체한다. 첫 공격은 기믹 시작 1초 뒤에 등장하며, 이후에는 기존 무작위 등장 간격을 사용한다. 게임오버 재시도 시에도 첫 공격은 1초 뒤에 등장한다.

## 게임오버 손 애니메이션

`ExploreProtoMessenger > Battle02RitualController > 메리 이미지 전환 > 이미지 전환 암전 시간 (초)`에서 모든 메리 모션 사이의 숨김 시간을 조절한다(기본 0.5초, 0이면 즉시 교체). 창 프레임은 유지하며 메리 Artwork만 숨긴다. `Attack Success Mary`는 시트 _4로, 게임오버 직전 이 모습으로 전환을 마친 뒤 손 애니메이션을 시작한다. `Hurt Mary`는 _5로, 정답 문장 완성 성공 때만 `Pain Seconds`(기본 0.85초) 동안 보여준 뒤 평상시로 복귀한다. 마지막 회차도 피격 연출 후 봉인으로 연결한다. 실제 임포트된 하위 스프라이트 이름은 `Merry_Cam_Knif_Sheet_0`~`_5`다.

`Battle02_Proto`는 게임오버 즉시 현재 게임 화면 전체에 맞춰 `Assets/Animation/Hand_Animation.aseprite`를 한 번 재생하고, 클립 종료 시 SignalLost 화면과 재시도 버튼을 표시한다. 재생 중 게임 입력을 차단하며 기존 손바닥 효과음은 재생하지 않는다. SignalLost 전환 시에는 해당 화면의 효과음을 재생한다. 기존 검은 배경·12개 손바닥 연출은 이 씬에서 사용하지 않는다.

`BattleGameOverCanvas/HandAnimation`은 화면 전체 Stretch, 여백 0, Scale (1, 1, 1)로 설정한다. 원본 캔버스 전체를 이 영역의 가로·세로에 각각 맞춰 표시하므로 화면 비율이 달라도 원본 가장자리를 자르지 않는다. 표시 영역을 조절하려면 부모 HandAnimation의 RectTransform 여백·Scale을 편집한다. 자식 `Artwork`의 크기·위치는 매 프레임 갱신된다. `Source Canvas Size`는 원본 Aseprite 캔버스 크기이며 현재 512×300이다. Aseprite에서 캔버스 크기 자체를 변경하면 이 값도 맞춘다. 프레임 그림이나 재생 시간만 바꿀 때는 수정할 필요 없다.

`HandAnimation`의 Animator는 스프라이트 애니메이션 적용에 필요하므로 유지한다. `AsepriteGameOverAnimation`은 Aseprite Importer가 만든 `Hand_Animation_Clip`을 직접 참조한다. 같은 원본 파일을 Aseprite에서 편집·저장하고 Unity 임포트가 끝나면 프레임·빈 프레임·타이밍과 SignalLost 전환 시점에 반영된다. 별도 PNG 추출이나 고정 시간 수정은 필요 없다. Importer의 레이어 병합과 Pivot Space=Canvas, Pivot Alignment=Center 설정을 유지한다. 애니메이션 태그를 추가하거나 클립 이름을 바꿔 클립 구성을 변경하면 `Clip` 참조도 확인한다. 기존 `Battle_Proto`는 이전 연출을 유지한다.

## 통화창 파손

BattleCallWindow/BrokenCallWindow는 통화창 내부에 여백 40으로 늘려 배치한 Image다. RectTransform 여백으로 균열 영역을 조정하며 Raycast Target은 끈다. Broken Window Stages에는 Broken_Window_Sheet.png의 _0, _1, _2를 순서대로 연결한다. 회차 완료나 정상 방어로 균열은 회복되지 않으며 재시도 시 초기화된다. 3회째 오입력 후에도 메리 공격 시간은 계속 흐른다. 통화 종료로 방어할 수 없지만 문장 완성에 따른 피격·다음 회차·최종 봉인 진행은 정상적으로 처리한다.

## 현재 문구와 음원

세 회차의 정답은 ‘네 아이가 기다리고 있어’다. 붉은 문구는 ‘다 부숴버리겠어’, ‘네 몸을 내놔’, ‘너무 고통스러워’, ‘아무도 도망갈 수 없어’, ‘이제 끝이야’, ‘더 많은 생기가 있어야 해’다. 문구는 씬의 Phrase 하위 음절 버튼과 해당 Round의 words·glitchTargets 참조로 관리한다. 향후 문구를 바꿀 때 필요한 정답 음절을 모두 남기고, word와 TMP 표시 글자를 함께 바꾼다. 원문에서 가져간 음절은 기존처럼 숨겨진다.

Interaction Audio는 기믹 전용 2D AudioSource다. Glass Break Clip은 앞 무음을 제거한 Break_Glass_Immediate.wav, Syllable Click Clip은 mouse_click001.mp3를 연결한다. 선택이 실제로 수락되었을 때만 클릭음을 재생하며, 글리치로 막힌 음절·소모한 음절·빈 슬롯은 소리를 내지 않는다. 음량은 Battle02InteractionAudio에서 조절한다.

Battle02는 메리 등장 음성 Mary_Female_NumberStation_Low와 그 음성이 재생되는 등장 대기 구간을 건너뛴다. 기존 암전·화면 전환 이후 Bell과 카메라 켜짐 연출을 재생하고(카메라가 켜질 때 01_noise_01을 한 번 재생), 통화 종료 버튼을 활성화하면서 기믹과 전투 음악을 시작한다. 기존 Battle_Proto의 도입 연출에는 적용하지 않는다.
