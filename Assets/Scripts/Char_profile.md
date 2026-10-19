# Char_profile — 캐릭터 기반 데이터

```yaml
document:
  id: char_profile
  language: ko
  updated_at: 2026-10-19
  purpose: 대사·시나리오·대화 CSV 작성 시 참조할 캐릭터 설정과 표현 기준
  sources:
    - id: source_first_meeting_revision
      description: 2026-10-19 사용자 수정. 첫 귀신 조우 예시와 그에 의존한 지침을 폐기한다. 린은 미나가 인터넷 귀신을 퇴마할 수 있다는 사실을 알고 직접 찾아와 방법을 묻는다.
    - id: source_opening_reboot
      description: 2026-10-19 최신 사용자 개편. 부모와 관계 단절, 작은 의뢰 해결 이후 시작, 도입부 린 미등장, 명랑하고 힘 있는 반응 유지, 속마음을 1인칭 나레이션으로 표기한다.
    - id: source_family_revision
      description: 2026-10-19 사용자 수정. 어머니의 안타까움과 아버지의 가문 계승에 대한 불만을 구분한다. 유미나는 귀신을 못 봐 퇴마사의 꿈을 포기했으며, 아버지에게 인정받고 싶은 마음으로 사이버 퇴마사를 결심한다.
    - id: source_character
      description: 사용자가 제공한 유미나·채 린의 기본 설정
    - id: source_mina_revision
      description: 2026-10-18 사용자 수정. 밝은 겉모습과 내면의 어둠을 대비하고, 어린 시절 기대와 실망의 경험에서 비롯된 인정 욕구를 추가한다.
  related_documents:
    universe: Universe.md
    dialogue_csv_guide: ../Docs/HowToUseScripts/DIALOGUE_CSV_GUIDE.md
  conventions:
    confirmed: 사용자가 명시한 설정
    interpreted: 확정 설정과 사용자 대화 예시를 바탕으로 해석한 표현 지침. 새로운 과거사나 세계관 사실이 아니다.
    reference_example: 사용자가 제공한 예시. 모든 상황에서 반복해야 할 고정 대사가 아니다.
    unknown: 아직 정하지 않은 정보. 임의로 확정하지 않는다.
    ids: 문서 참조용 식별자. 게임 코드·CSV ID가 아니다. 인물 ID는 Universe.md와 공유한다.
    priority: 명시된 설정과 장면의 상황을 MBTI 유형이나 예시 기반 해석보다 우선한다.
    scope: 세계관·과거사·도입부 연표는 Universe.md를 함께 참조한다.
    latest_priority: source_first_meeting_revision과 source_opening_reboot가 이전 설정과 예시에 우선한다. 과거 예시의 사건은 현재 연표로 확정하지 않는다.

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
          source: source_mina_revision
          value: ENFP. 평소에는 명랑하고 밝게 행동하지만, 내면에는 어린 시절의 좌절과 자괴감이 남아 있다. 밝은 겉모습과 어두운 내면의 대비가 성격의 중요한 축이다.
        - id: MINA_F002
          field: fear
          value: 일반적인 공포는 느낀다. 겁먹었을 때 공포에 떨며 오래 위축되기보다 우와아앗!!처럼 과장되게 소리치는 생생한 반응으로 표현한다. 시작 전에 작은 의뢰를 몇 건 해결했지만 베테랑은 아니다.
        - id: MINA_F003
          field: occupation
          value: 전직 개발자, 현직 사이버 퇴마사
        - id: MINA_F004
          field: family
          value: 명문 퇴마사 유 가문 출신이며 현재 부모님과 관계가 단절되어 있다. 단절의 계기와 시기는 미정이다.
        - id: MINA_F005
          field: perception
          value: 현실의 귀신을 보지 못하지만 인터넷에서는 귀신의 본체를 유일하게 볼 수 있어 사이버 퇴마사가 되었다.
        - id: MINA_F006
          field: technology
          value: 개발자 경험을 바탕으로 퇴마 프로그램을 발명했다. 현실의 퇴마 의식을 프로그램으로 따라 한 것이지만 성능 자체는 좋다.
        - id: MINA_F007
          field: emotional_background
          source: source_mina_revision
          value: 어린 시절 큰 기대를 받았으나 그 기대에 부응하지 못해 실망을 안긴 경험이 있다. 자신이 가족을 실망시켰다는 감정이 현재에도 남아 있다.
          universe_reference: B005
        - id: MINA_F008
          field: need_for_recognition
          source: source_mina_revision
          value: 남에게 인정받는 것에 조금 집착한다. 인정받고 싶다는 마음에는 다시 기대를 저버리거나 실망을 안기고 싶지 않다는 불안이 얽혀 있다.
        - id: MINA_F009
          field: outward_and_inner_contrast
          source: source_mina_revision
          value: 겉으로 밝게 웃고 넘어가는 순간에도 내면에는 상처와 불안이 남을 수 있다. 타인에게 보이는 반응과 혼자 품는 감정이 항상 같지는 않다.
        - id: MINA_F010
          field: abandoned_dream
          source: source_family_revision
          value: 스스로 퇴마사가 되고 싶었지만 귀신을 보지 못해 꿈을 포기했다. 가문의 요구만으로 퇴마사를 지망한 것이 아니다.
        - id: MINA_F011
          field: career_motivation
          source: source_opening_reboot
          value: 포기했던 퇴마사의 꿈을 인터넷에서 이어가고 있다. 돈을 많이 벌지 못하고 물어볼 곳도 없지만 퇴마 일을 할 수 있어서 좋다. 인정 욕구는 유지하되, 이전의 어머니와 통화 후 진로를 결심하는 사건은 폐기한다. 구체적인 전직 과정은 미정이다.
        - id: MINA_F012
          field: current_experience
          source: source_opening_reboot
          value: 시작 시점에 프로그램을 이미 개발했고 자잘한 의뢰 몇 건을 해결했다. 블러디 메리는 생애 첫 퇴마 의뢰가 아니다.
        - id: MINA_F013
          field: knowledge_of_peers
          source: source_opening_reboot
          value: 세계관상 유일한 사이버 퇴마사지만 미나 본인은 다른 사이버 퇴마사를 아직 만나 보지 못한 정도로 인식한다. 세계 유일임을 단정하는 대사는 쓰지 않는다.
    expression_guidelines:
      status: interpreted
      items:
        - id: MINA_G001
          guideline: 놀람·기대·당황은 밝고 생생하게 드러내되, 상처나 인정받지 못할까 하는 불안까지 늘 솔직하게 말하지는 않는다. 가벼운 말 뒤에 짧은 침묵이나 속마음을 두어 대비를 만든다.
          basis: [MINA_F001, MINA_F008, MINA_F009, EX01_L003, EX01_L008]
        - id: MINA_G002
          guideline: 처음 만난 린에게 존댓말을 쓴다. 유명한 베테랑 퇴마사 린이 자기 집에 찾아왔다는 사실에 놀라며 방문 이유를 궁금해한다.
          basis: [EX01_L003, EX01_L008]
        - id: MINA_G003
          guideline: 무례한 말을 들으면 당황하면서도 항의한다. 명랑함을 무조건적인 순종이나 무기력함으로 쓰지 않는다.
          basis: [EX01_L004, EX01_L005]
        - id: MINA_G004
          guideline: 귀신 조우 시 우와아앗!!처럼 과장되게 소리칠 수 있다. 공포에 떨며 무력해지는 표현보다 생생한 반응을 우선하고, 남과의 대화에서는 명랑하고 힘 있는 말투를 유지한다.
          basis: [MINA_F002]
        - id: MINA_G005
          guideline: 겉으로는 웃거나 괜찮다고 넘겨도 속으로는 상대를 실망시켰는지 되짚을 수 있다. 내면의 어둠은 자책·위축·불안으로 표현하되, 매번 직접 설명하는 독백을 붙이지 않는다.
          basis: [MINA_F007, MINA_F008, MINA_F009]
        - id: MINA_G006
          guideline: 개발 지식과 퇴마 실무 경험을 구분한다. 프로그램 개발 능력이 귀신의 성질이나 현장 대응에 대한 숙련을 뜻하지 않는다.
          basis: [MINA_F002, MINA_F006]
        - id: MINA_G007
          guideline: 칭찬을 받으면 기뻐하면서 상대의 반응을 한 번 더 살피거나, 자신이 도움이 되었는지 확인하고 싶어 할 수 있다. 모든 대사에서 노골적으로 칭찬을 요구하지는 않는다.
          basis: [MINA_F008]
        - id: MINA_G008
          guideline: 실수나 부정적인 평가를 받으면 당장의 문제보다 또 실망시켰다는 생각에 더 오래 붙잡힐 수 있다. 겉으로 빠르게 수습하는 말과 뒤늦게 남는 자책의 차이를 활용한다.
          basis: [MINA_F007, MINA_F008, MINA_F009]
        - id: MINA_G009
          guideline: 자신만 할 수 있는 일을 발견한 기쁨에는 능력을 인정받을 기회를 얻었다는 마음도 섞일 수 있다. 인정 욕구가 강해지는 장면에서는 잘 보이려는 조급함이나 무리를 표현할 수 있지만, 이를 항상 무모하게 행동하는 성격으로 고정하지 않는다.
          basis: [MINA_F005, MINA_F008]
        - id: MINA_G010
          guideline: 타인을 돕고 싶거나 일이 재미있어서 웃는 순간도 유지한다. 밝음 전체를 연기나 가면으로 취급하지 않고, 진짜 명랑함과 감추는 상처가 공존하도록 쓴다.
          basis: [MINA_F001, MINA_F009]

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
        - id: RIN_G005
          guideline: 귀찮아하는 말투를 쓰더라도 알려줄 것은 알려준다. 구체적인 귀신 대응이나 안심시키는 행동은 별도 장면이 정해지기 전까지 고정하지 않는다.
          basis: [RIN_F006]
        - id: RIN_G006
          guideline: 숨은 정은 과도한 다정한 말보다 도움과 행동으로 드러낸다. 특정 귀신 조우 장면을 근거로 삼지 않는다.
          basis: [RIN_F001, RIN_F006]
relationships:
  - id: REL002
    participants: [mina, mina_mother]
    status: confirmed
    source: source_family_revision
    facts:
      - 어머니는 귀신을 보지 못해 퇴마사가 되지 못한 딸의 상황을 안타까워했다.
      - 현재는 관계가 단절되어 있으며 도입부에 통화가 없다. 단절의 구체적인 경위는 미정이다.
    universe_reference:
      file: Universe.md
      id: B004
  - id: REL003
    participants: [mina, mina_father]
    status: confirmed
    source: source_family_revision
    facts:
      - 아버지는 명문 퇴마사 가문의 대가 끊길 수 있다는 점에서 딸을 못마땅해한다.
      - 아버지에게 인정받고 싶어 하는 마음은 내면의 배경으로 남아 있다. 현재는 부모와 관계가 단절된 상태다.
      - 새 전직 과정이나 화해 계획을 임의로 확정하지 않는다.
    universe_reference:
      file: Universe.md
      id: B007
  - id: REL001
    participants: [mina, rin]
    status: confirmed
    source: source_opening_reboot
    facts:
      - 린은 베테랑 퇴마사이자 조력자라는 인물 설정을 유지한다.
      - 현재 도입부 시점에는 등장하지 않는다. 린은 미나가 인터넷 귀신을 퇴마할 수 있다는 사실을 알고 직접 미나의 집을 찾아온다. 구체적인 등장 시기와 소식을 알게 된 경로는 미정이다.
      - 부모의 부탁으로 방문하는 것이 아니다. 미나는 유명한 린의 방문에 놀라고, 린은 인터넷 귀신을 어떻게 퇴마했는지 묻는다.
    universe_reference:
      file: Universe.md
      section: 인물 관계
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
      mina: 유명한 베테랑이 자기 집에 왜 찾아왔는지 놀라며 묻는다.
      rin: 명성 이야기를 끊고 인터넷 귀신을 대체 어떻게 퇴마했는지 묻는다.
      basis: [EX01_L008, EX01_L009]
    - id: PAIR_G005
      guideline: 유미나의 밝은 감정 표현과 린의 담담한 업무 태도 사이의 간격으로 대화 리듬을 만든다. 유미나의 명랑함·개발 능력과 린의 실력·숨은 정을 유지하면서, 유미나가 린의 평가를 속으로 오래 신경 쓸 수 있음을 반영한다.
      basis: [MINA_F001, MINA_F006, MINA_F008, RIN_F001, RIN_F005, EX01]

examples:
  status: external_reference
  first_meeting:
    file: Story_Parts/Rin_FirstMeeting/01_FirstMeeting.md
    id: EX01
    purpose: 첫 만남 대사와 EX01_L 계열 근거 ID의 원본. 구체적인 진행과 대사는 이 파일에서 관리한다.
writing_constraints:
  - id: CHAR_C001
    rule: MBTI를 정형화된 행동 공식으로 사용하지 않는다. 구체적인 성격과 장면 상황을 우선한다.
  - id: CHAR_C002
    rule: 유미나를 계속 비명만 지르는 인물로, 린을 계속 모욕만 하는 인물로 단순화하지 않는다.
  - id: CHAR_C003
    rule: 예시에서 관찰한 말투·행동을 모든 상대나 모든 상황에 적용하는 확정 설정으로 확대하지 않는다.
  - id: CHAR_C004
    rule: 이름 확정이나 문서용 ID 도입을 이유로 기존 CSV의 SpeakerID나 게임 코드를 자동 변경하지 않는다.
  - id: CHAR_C005
    rule: 내면의 어둠을 악의·잔혹함·임상적 진단으로 확대하지 않는다. 이번 수정의 중심은 어린 시절의 좌절, 자괴감, 인정 욕구와 밝은 겉모습의 대비다.
  - id: CHAR_C006
    rule: 인정 욕구가 조금 강하다는 설정을 모든 행동의 유일한 동기로 만들지 않는다. 평범한 즐거움·공포·호의와 자기 판단도 유지한다.
  - id: CHAR_C007
    rule: 어머니의 안타까움과 아버지의 못마땅한 태도를 구분한다. 아버지의 태도를 유미나의 오해로 처리하거나 부모 모두가 단순히 걱정하는 것으로 바꾸지 않는다. 현재 부모와 관계가 단절된 설정은 유지한다. 미정인 학대나 단절의 구체적인 사건·원인을 추가하지 않는다.

csv_usage:
  guide: ../Docs/HowToUseScripts/DIALOGUE_CSV_GUIDE.md
  current_first_call_file: ../Explore_Proto/Data/Explore_FirstCall.csv
  existing_mina_speaker_ids: [주인공, 주인공 속마음]
  rin_in_story_name: 린
  new_dialogue_speaker_policy: 유미나의 이름과 역할명 중 어느 것을 표시할지는 해당 대화의 표시 방식에 맞춰 정한다.
  inner_monologue_policy: 새 원고에서 유미나의 속마음은 나레이션으로 표기하고 괄호 없이 1인칭으로 쓴다. 기존 CSV의 주인공 속마음 ID는 현재 구현 기록이며 별도 요청 없이 일괄 변경하지 않는다.
  stage_direction_policy: 행동·효과음은 발화와 구분한다. 문서의 type 값이 CSV의 새 열이나 실행 기능을 뜻하지 않는다.
  asset_policy: Image와 EventID는 실제 연결된 자산·처리 코드에 맞춰 지정한다. 미구현 자산이나 이벤트가 존재한다고 가정하지 않는다.

unknowns:
  - id: CHAR_U001
    topic: 두 사람의 나이와 외형
  - id: CHAR_U002
    topic: 정확한 경력 연수. 가문 표기는 유 가문으로 확정되어 있다.
  - id: CHAR_U003
    topic: 두 사람의 장기적인 관계 변화
  - id: CHAR_U004
    topic: 예시에 등장하지 않은 상대에 대한 호칭·높임말 및 관계별 말투
```

관련 자료: [세계관·서사 기반 데이터](Universe.md), [대화 CSV 가이드](../Docs/HowToUseScripts/DIALOGUE_CSV_GUIDE.md).
