# Battle02 단어 기믹 편집

`Battle02_Proto`의 `ExploreProtoMessenger`에 있는 `Battle02RitualController`가 메리와 첫 조우한 뒤 새 기믹을 진행한다. 기존 `Battle_Proto`는 이 컴포넌트를 연결하지 않아 이전 전투를 유지한다. 이 씬의 UI는 `ExploreProtoCanvas`에 저장되어 있으며 실행 중 새로 만들지 않는다.

## 씬 구성

- `BattleCallWindow/Battle02HangupButton`: 첫 조우부터 스프라이트 아래에 표시되며, 기믹 시작 전에는 누를 수 없다. 칼을 든 메리가 보일 때 누르면 모습과 4초 제한시간을 초기화한다. 평소에 누르면 메신저의 선택 문장을 글리치와 `Noise` 효과음과 함께 지운다. 클릭 후 0.5초 동안 다시 누를 수 없다. 메신저가 열리면 창 아래의 접근 가능한 자리로 이동하고 닫히면 원래 위치로 돌아온다.
- `Battle02DarkMessenger`: 하단 `Button_Messenger`나 창의 X로 열고 닫는다. 닫았다 다시 열어도 선택한 단어는 남는다.
- `RoundWordsGroup/Round_1`~`Round_5`: 각 회차의 고정 문구와 클릭 가능한 단어 오브젝트다. `FakePhrase_*`는 눌러도 아무 일도 일어나지 않는 문구다. 4·5회에는 각각 `TextBabyImage`, `TextFaceImage`를 사용하며, 선택 문구와 페이크 문구에는 이미지와 같은 색의 불투명한 배경을 둔다. 이미지 안에 없는 정답 단어는 그 위에 배치된 붉은 텍스트 버튼으로 선택한다.
- `SelectedWords`: 고른 단어 네 개를 순서대로 보여준다. 잘못 고른 단어는 붉은색이며, 누르면 제거할 수 있다.

각 단어의 `Battle02WordSource.word`는 실제 선택되는 단어다. 보이는 문구를 바꿀 때 이 값과 위치를 함께 확인한다. 현재 정답은 코드의 `Answer` 배열에 있는 `네 / 아기는 / 죽지 / 않았어`이며 다섯 회차가 같은 문장을 사용한다. 각 회차에는 정답 단어 네 개가 적어도 하나씩 있어야 한다. 회차가 끝나면 창이 닫히고, 다음 회차는 플레이어가 메신저 아이콘으로 직접 연다.

글리치는 현재 회차의 `glitchTargets`에 연결된 문구 위치와 크기를 따라가며, 1회부터 5회까지 동시에 영향을 받는 문구 수와 발생 빈도가 증가한다. 메리의 일반·칼 스프라이트와 Noise는 컨트롤러 Inspector 참조로 교체할 수 있다. 창·얼굴 이미지도 씬의 `RawImage` 참조를 바꾸면 된다. 타이밍은 같은 컴포넌트의 `시간` 항목에서 조절한다. 다섯 번째 완료는 `BloodyMaryPhaseTwoController.CompleteFromExternal()`을 통해 기존 봉인 연출에 연결된다.

## 글리치 이미지

`Mary Glitch Frames`는 `Assets/Sprite/Effect/Glitch_Sheet.png`의 5종, `Word Glitch Frames`는 `Glitch_R_Sheet.png`의 5종을 사용한다. 효과가 발생할 때 순서를 섞고 `Glitch Frame Seconds`(기본 0.06초)마다 교대한다. 동시에 나타나는 문구 글리치는 서로 다른 프레임부터 시작한다. 기존 RawImage가 각 Sprite의 분할 영역을 표시하므로 이 두 시트는 Sprite Atlas에 넣지 않고 사용한다. 문구 선택 제한과 회차별 발생 빈도, 메리의 5초 주기·1초 지속시간은 그대로 유지한다. 평상시 통화 종료 버튼으로 선택 문장을 지우는 입력란 효과는 기존 이미지를 사용한다.

문구 글리치는 Sprite의 투명 여백을 제외한 영역을 표시한다. `Word Glitch Size Multiplier`의 X/Y로 문구 대비 가로·세로 배율(기본 1.15/1.2)을, `Word Glitch Padding`으로 추가 크기(기본 24/12)를 조절한다. 크기는 `문구 RectTransform 크기 × 배율 + 추가 크기`다. 영구 적용하려면 Play를 종료하고 Inspector에서 값을 편집한 뒤 씬을 저장한다. 메리 글리치의 크기는 이 설정에 영향을 받지 않는다.

## 메리 공격 이미지와 시간
Battle02_Proto의 ExploreProtoMessenger 오브젝트 > Battle02RitualController에서 모습 A/B/C 표시 시간(초)을 조절한다. 기본값은 1.6 / 1.2 / 1.2초이며 합계 4초가 게임오버 제한 시간이다. 공격 시작 때 세 시간을 확정하므로 Play 중 변경한 값은 다음 공격부터 적용된다. Normal Mary는 시트 _0, Knife Mary는 _1(A), Knife Mary B는 _2(B), Knife Mary C는 _3(C)에 연결한다. 통화 종료로 공격을 막거나 회차가 끝나면 평상시로 복귀하며 다음 공격은 A부터 시작한다.

메리 Artwork는 씬에서 편집한 RectTransform 크기와 배치를 그대로 사용하고 Sprite만 교체한다. 첫 공격은 기믹 시작 1초 뒤에 등장하며, 이후에는 기존 무작위 등장 간격을 사용한다. 게임오버 재시도 시에도 첫 공격은 1초 뒤에 등장한다.
