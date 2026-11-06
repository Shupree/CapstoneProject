# Battle02 단어 기믹 편집

`Battle02_Proto`의 `ExploreProtoMessenger`에 있는 `Battle02RitualController`가 메리와 첫 조우한 뒤 새 기믹을 진행한다. 기존 `Battle_Proto`는 이 컴포넌트를 연결하지 않아 이전 전투를 유지한다. 이 씬의 UI는 `ExploreProtoCanvas`에 저장되어 있으며 실행 중 새로 만들지 않는다.

## 씬 구성

- `BattleCallWindow/Battle02HangupButton`: 첫 조우부터 스프라이트 아래에 표시되며, 기믹 시작 전에는 누를 수 없다. 칼을 든 메리가 보일 때 누르면 모습과 4초 제한시간을 초기화한다. 평소에 누르면 메신저의 선택 문장을 글리치와 `Noise` 효과음과 함께 지운다. 클릭 후 0.5초 동안 다시 누를 수 없다. 메신저가 열리면 창 아래의 접근 가능한 자리로 이동하고 닫히면 원래 위치로 돌아온다.
- `Battle02DarkMessenger`: 하단 `Button_Messenger`나 창의 X로 열고 닫는다. 닫았다 다시 열어도 선택한 단어는 남는다.
- `RoundWordsGroup/Round_1`~`Round_5`: 각 회차의 고정 문구와 클릭 가능한 단어 오브젝트다. `FakePhrase_*`는 눌러도 아무 일도 일어나지 않는 문구다. 4·5회에는 각각 `TextBabyImage`, `TextFaceImage`를 사용하며, 선택 문구와 페이크 문구에는 이미지와 같은 색의 불투명한 배경을 둔다. 이미지 안에 없는 정답 단어는 그 위에 배치된 붉은 텍스트 버튼으로 선택한다.
- `SelectedWords`: 고른 단어 네 개를 순서대로 보여준다. 잘못 고른 단어는 붉은색이며, 누르면 제거할 수 있다.

각 단어의 `Battle02WordSource.word`는 실제 선택되는 단어다. 보이는 문구를 바꿀 때 이 값과 위치를 함께 확인한다. 현재 정답은 코드의 `Answer` 배열에 있는 `네 / 아기는 / 죽지 / 않았어`이며 다섯 회차가 같은 문장을 사용한다. 각 회차에는 정답 단어 네 개가 적어도 하나씩 있어야 한다. 회차가 끝나면 창이 닫히고, 다음 회차는 플레이어가 메신저 아이콘으로 직접 연다.

글리치는 현재 회차의 `glitchTargets`에 연결된 문구 위치와 크기를 따라가며, 1회부터 5회까지 동시에 영향을 받는 문구 수와 발생 빈도가 증가한다. 메리의 일반·칼 스프라이트와 Noise는 컨트롤러 Inspector 참조로 교체할 수 있다. 창·얼굴 이미지도 씬의 `RawImage` 참조를 바꾸면 된다. 타이밍은 같은 컴포넌트의 `시간` 항목에서 조절한다. 다섯 번째 완료는 `BloodyMaryPhaseTwoController.CompleteFromExternal()`을 통해 기존 봉인 연출에 연결된다.
