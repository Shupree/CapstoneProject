# Char_profile — 캐릭터 기반 데이터

```yaml
document:
  id: char_profile
  language: ko
  updated_at: 2026-10-16
  purpose: 대사·시나리오·대화 CSV 작성 시 참조할 캐릭터 설정과 표현 기준
  sources:
    - id: source_character
      description: 사용자가 제공한 유미나·채 린의 설정과 두 개의 대화 예시
  related_documents:
    universe: Universe.md
    dialogue_csv_guide: ../Docs/DIALOGUE_CSV_GUIDE.md
  conventions:
    confirmed: 사용자가 명시한 설정
    interpreted: 사용자 대화 예시에서 해석한 표현 지침. 새로운 과거사나 세계관 사실이 아니다.
    reference_example: 사용자가 제공한 예시. 모든 상황에서 반복해야 할 고정 대사가 아니다.
    unknown: 아직 정하지 않은 정보. 임의로 확정하지 않는다.
    ids: 문서 참조용 식별자. 게임 코드·CSV ID가 아니다. 인물 ID는 Universe.md와 공유한다.
    priority: 명시된 설정과 장면의 상황을 MBTI 유형이나 예시 기반 해석보다 우선한다.
    scope: 세계관·과거사·도입부 연표는 Universe.md를 함께 참조한다.

characters:
  - id: mina
    name: 유미나
    role: 주인공
    facts:
      status: confirmed
      source: source_character
      items:
        - id: MINA_F001
          field: personality
          value: ENFP. 명랑하며 현실의 보통 사람과 크게 다르지 않은 성격이다.
        - id: MINA_F002
          field: fear
          value: 특별히 겁이 많은 것은 아니지만 일반적인 공포를 느낀다. 퇴마업으로 살아온 사람이 아니므로 귀신에 대한 공포도 있다.
        - id: MINA_F003
          field: occupation
          value: 전직 개발자, 현직 사이버 퇴마사
        - id: MINA_F004
          field: family
          value: 명문 퇴마사 가문 출신이다.
        - id: MINA_F005
          field: perception
          value: 현실의 귀신을 보지 못하지만 인터넷에서는 귀신의 본체를 유일하게 볼 수 있어 사이버 퇴마사가 되었다.
        - id: MINA_F006
          field: technology
          value: 개발자 경험을 바탕으로 퇴마 프로그램을 발명했다. 현실의 퇴마 의식을 프로그램으로 따라 한 것이지만 성능 자체는 좋다.
    expression_guidelines:
      status: interpreted
      items:
        - id: MINA_G001
          guideline: 밝고 감정 반응이 솔직하다. 놀람·기대·당황을 말에 자연스럽게 드러낸다.
          basis: [EX01_L003, EX01_L005, EX01_L008, EX02_L001]
        - id: MINA_G002
          guideline: 처음 만난 린에게 존댓말을 쓴다. 유명한 퇴마사라는 것을 알면 기대와 반가움을 표현한다.
          basis: [EX01_L003, EX01_L008]
        - id: MINA_G003
          guideline: 무례한 말을 들으면 당황하면서도 항의한다. 명랑함을 무조건적인 순종이나 무기력함으로 쓰지 않는다.
          basis: [EX01_L004, EX01_L005]
        - id: MINA_G004
          guideline: 귀신 조우 시 비명을 지르거나 말을 더듬을 수 있다. 모든 상황에서 겁에 질리는 인물로 고정하지 않는다.
          basis: [MINA_F002, EX02_L001]
        - id: MINA_G005
          guideline: 겉으로 예의를 지키면서도 속마음으로 상대를 어렵게 느끼는 등 발화와 내면의 온도 차이를 활용할 수 있다.
          basis: [EX01_L010, EX01_L011]
        - id: MINA_G006
          guideline: 개발 지식과 퇴마 실무 경험을 구분한다. 프로그램 개발 능력이 귀신의 성질이나 현장 대응에 대한 숙련을 뜻하지 않는다.
          basis: [MINA_F002, MINA_F006, EX02_L002]

  - id: rin
    name: 채 린
    in_story_name: 린
    role: 조력자
    facts:
      status: confirmed
      source: source_character
      items:
        - id: RIN_F001
          field: personality
          value: ISTJ. 쿨하고 여유가 넘친다. 겉으로 내색하지 않지만 정이 많다.
        - id: RIN_F002
          field: occupation
          value: 베테랑 퇴마사
        - id: RIN_F003
          field: fear
          value: 귀신에 대한 공포가 전혀 없다. 오히려 이런 것이 왜 무서운지 의아해한다.
        - id: RIN_F004
          field: work_attitude
          value: 퇴마를 직장인의 일처럼 생각한다.
        - id: RIN_F005
          field: ability_and_reputation
          value: 영력이 높고 재능이 있으며 퇴마 업계에서 알아주는 베테랑이다.
        - id: RIN_F006
          field: help_style
          value: 귀찮아하는 말투를 쓰지만 알려줄 것은 모두 알려준다.
    expression_guidelines:
      status: interpreted
      items:
        - id: RIN_G001
          guideline: 유미나에게 반말을 쓰고 짧고 직접적으로 말한다. 첫 만남에서도 거리낌 없는 평가를 던진다.
          basis: [EX01_L002, EX01_L004]
        - id: RIN_G002
          guideline: 칭찬이나 자신의 명성에 관한 이야기를 길게 받아 주기보다 해야 할 일로 화제를 돌린다.
          basis: [EX01_L008, EX01_L009]
        - id: RIN_G003
          guideline: 귀신의 등장을 의식의 당연한 결과로 받아들이고 다음 행동을 안내한다.
          basis: [RIN_F003, RIN_F004, EX02_L002]
        - id: RIN_G004
          guideline: 초보자의 공포를 자신도 느끼는 것처럼 표현하기보다 상대의 수준과 귀신의 강도를 판단해 필요한 정보를 전한다.
          basis: [RIN_F003, EX02_L003, EX02_L004]
        - id: RIN_G005
          guideline: 무심하거나 귀찮아 보이는 말투 안에 실질적인 안내와 안심할 근거를 담는다. 귀찮다는 이유로 필요한 설명을 생략하지 않는다.
          basis: [RIN_F006, EX02_L002, EX02_L004]
        - id: RIN_G006
          guideline: 숨은 정은 과도한 다정한 말보다 도움과 행동으로 드러내는 편이 예시 분위기에 맞는다.
          basis: [RIN_F001, RIN_F006, EX02_L004]
        - id: RIN_G007
          guideline: 방을 나가는 예시는 약한 귀신이라는 판단과 유미나가 직접 해야 한다는 안내 뒤의 행동이다. 위험한 상황에서도 무조건 방치한다고 일반화하지 않는다.
          basis: [EX02_L002, EX02_L004, EX02_A001]

relationships:
  - id: REL001
    participants: [mina, rin]
    status: confirmed
    source: source_character
    facts:
      - 유미나의 부모님이 부탁해 린이 유미나를 찾아오면서 처음 만난다.
      - 유미나는 린의 이름을 듣고 업계에서 유명한 퇴마사임을 알아본다.
      - 린은 유미나가 퇴마 일을 시작한다는 것을 알고 필요한 것을 알려주는 조력자다.
    basis: [EX01_L006, EX01_L007, EX01_L008, EX01_L009]
    universe_reference:
      file: Universe.md
      id: T008

interaction_guidelines:
  status: interpreted
  participants: [mina, rin]
  items:
    - id: PAIR_G001
      situation: 첫 대면
      mina: 예의를 갖추지만 갑작스러운 평가에는 당황하고 항의한다.
      rin: 친근한 인사보다 확인과 용건을 먼저 꺼낸다.
      basis: [EX01]
    - id: PAIR_G002
      situation: 린의 명성이 화제가 될 때
      mina: 기대와 감탄을 드러낸다.
      rin: 명성 이야기를 끊고 실무로 넘어간다.
      basis: [EX01_L008, EX01_L009]
    - id: PAIR_G003
      situation: 귀신이 등장할 때
      mina: 실제로 나타났다는 사실에 놀라고 두려움을 느낀다.
      rin: 의식의 당연한 결과로 받아들이고 다음 행동을 안내한다.
      basis: [EX02_L001, EX02_L002]
    - id: PAIR_G004
      situation: 유미나가 직접 퇴마해야 할 때
      mina: 익숙하지 않은 상황에 긴장한다.
      rin: 직접 해야 한다고 말하고 상대의 강도에 관한 판단을 전한다.
      basis: [EX02]
    - id: PAIR_G005
      guideline: 유미나의 솔직한 감정 반응과 린의 담담한 업무 태도 사이의 간격으로 대화 리듬을 만든다. 유미나의 명랑함·개발 능력과 린의 실력·숨은 정을 함께 유지한다.
      basis: [MINA_F001, MINA_F006, RIN_F001, RIN_F005, EX01, EX02]

examples:
  status: reference_example
  source: source_character
  text_policy: 대사 문구는 원문을 보존하고 발화·속마음·행동·효과음을 분리했다.
  items:
    - id: EX01
      title: 둘의 첫 만남
      sequence:
        - id: EX01_S001
          type: sound
          text: 똑똑똑
        - id: EX01_L001
          type: speech
          speaker: mina
          text: '누구세요?'
          accompanying_action: 문을 연다
        - id: EX01_L002
          type: speech
          speaker: rin
          text: '너가 유미나야?'
        - id: EX01_L003
          type: speech
          speaker: mina
          text: '엇, 네.. 맞는데요..?'
        - id: EX01_L004
          type: speech
          speaker: rin
          text: '흠, 좀 맹하게 생겼네.'
        - id: EX01_L005
          type: speech
          speaker: mina
          text: '네..? 아니 누구신데 갑자기 그러세요!'
        - id: EX01_L006
          type: speech
          speaker: rin
          text: '난 채 린, 그냥 린이라고 불러.'
        - id: EX01_L007
          type: speech
          speaker: rin
          text: '너네 부모님이 부탁해서 온 거야. 너 뭐 퇴마 일 시작한다며?'
        - id: EX01_L008
          type: speech
          speaker: mina
          text: '아! 당신이었군요! 퇴마 업계에서는 모르는 사람이 없다는 그..!!'
        - id: EX01_L009
          type: speech
          speaker: rin
          text: '그런 건 됐고, 내가 뭐 알려줘야 되는지부터 말 해.'
        - id: EX01_L010
          type: speech
          speaker: mina
          text: '아.. 네에..'
        - id: EX01_L011
          type: inner_monologue
          speaker: mina
          text: '(되게 어려운 사람이네..)'
    - id: EX02
      title: 첫 귀신 조우
      situation: 의뢰인에게 감지 의식 프로그램을 사용하자 숨어 있던 귀신이 등장했다.
      chronology_constraint: 예시 제목의 '첫'만으로 Universe.md의 최초 성불 사건 T003~T004와 동일한 사건으로 간주하지 않는다. 연표상의 정확한 위치는 미정이다.
      sequence:
        - id: EX02_L001
          type: speech
          speaker: mina
          text: '으.. 으아아앗!! 진, 진짜로 나타났어요!!'
        - id: EX02_L002
          type: speech
          speaker: rin
          text: '불렀으니까 나왔겠지. 이제 여기서부터는 너가 직접 해야 해.'
        - id: EX02_L003
          type: speech
          speaker: rin
          text: '솔직히 영 못 미덥긴 하지만 어쩔 수 없지.'
        - id: EX02_L004
          type: speech
          speaker: rin
          text: '그래도 뭐, 센 놈은 아니니까 별 일은 없을 거야.'
        - id: EX02_L005
          type: speech
          speaker: rin
          text: '다 끝나고 불러~'
        - id: EX02_A001
          type: action
          actor: rin
          text: 린이 방에서 나간다.

writing_constraints:
  - id: CHAR_C001
    rule: MBTI를 정형화된 행동 공식으로 사용하지 않는다. 구체적인 성격과 장면 상황을 우선한다.
  - id: CHAR_C002
    rule: 유미나를 계속 비명만 지르는 인물로, 린을 계속 모욕만 하는 인물로 단순화하지 않는다.
  - id: CHAR_C003
    rule: 예시에서 관찰한 말투·행동을 모든 상대나 모든 상황에 적용하는 확정 설정으로 확대하지 않는다.
  - id: CHAR_C004
    rule: 이름 확정이나 문서용 ID 도입을 이유로 기존 CSV의 SpeakerID나 게임 코드를 자동 변경하지 않는다.

csv_usage:
  guide: ../Docs/DIALOGUE_CSV_GUIDE.md
  current_first_call_file: ../Explore_Proto/Data/Explore_FirstCall.csv
  existing_mina_speaker_ids: [주인공, 주인공 속마음]
  rin_in_story_name: 린
  new_dialogue_speaker_policy: 유미나의 이름과 역할명 중 어느 것을 표시할지는 해당 대화의 표시 방식에 맞춰 정한다.
  inner_monologue_policy: 일반 발화와 구분하고 본문에 괄호를 사용한다.
  stage_direction_policy: 행동·효과음은 발화와 구분한다. 문서의 type 값이 CSV의 새 열이나 실행 기능을 뜻하지 않는다.
  asset_policy: Image와 EventID는 실제 연결된 자산·처리 코드에 맞춰 지정한다. 미구현 자산이나 이벤트가 존재한다고 가정하지 않는다.

unknowns:
  - id: CHAR_U001
    topic: 두 사람의 나이와 외형
  - id: CHAR_U002
    topic: 정확한 경력 연수와 가문 이름
  - id: CHAR_U003
    topic: 두 사람의 장기적인 관계 변화
  - id: CHAR_U004
    topic: 예시에 등장하지 않은 상대에 대한 호칭·높임말 및 관계별 말투
```

관련 자료: [세계관·서사 기반 데이터](Universe.md), [대화 CSV 가이드](../Docs/DIALOGUE_CSV_GUIDE.md).
