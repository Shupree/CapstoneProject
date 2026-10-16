# Universe — 세계관·서사 기반 데이터

```yaml
document:
  id: universe
  language: ko
  updated_at: 2026-10-16
  purpose: 대사·시나리오 작성 시 참조할 세계관 및 도입부 설정
  sources:
    - id: source_universe
      description: 사용자가 제공한 세계관·주인공 배경·도입부 배경 1·2
    - id: source_character
      description: 사용자가 앞서 제공한 캐릭터 설정과 대화 예시
      reference: Char_profile.md
  conventions:
    confirmed: 사용자가 명시한 확정 설정
    unknown: 사용자가 아직 정하지 않은 정보. 임의로 확정하지 않는다.
    ids: 이 문서에서 참조하기 위한 식별자. 게임 코드·CSV ID가 아니다.
    chronology: 사건 ID의 숫자 순서가 시간 순서다. 구체적인 연도·기간을 뜻하지 않는다.

entities:
  - id: mina
    name: 유미나
    role: 주인공
    character_reference: Char_profile.md
  - id: rin
    name: 채 린
    alias: 린
    role: 베테랑 퇴마사이자 주인공의 조력자
    character_reference: Char_profile.md
  - id: mina_family
    name: 유미나의 가문
    description: 대대로 이름난 퇴마사를 배출해 온 가문
  - id: mina_parents
    name: 유미나의 부모님
  - id: first_encounter_ghost
    name: 도입부에서 처음 만난 귀신
    traits: [작음, 귀여움, 악하지 않음, 협조적]
  - id: exorcism_program
    name: 퇴마 프로그램
    creator: mina

world_facts:
  - id: W001
    status: confirmed
    source: source_universe
    fact: 오래전부터 괴이로 인한 이상현상이 존재했다.
  - id: W002
    status: confirmed
    source: source_universe
    fact: 이상현상을 처리하는 퇴마사들이 대대로 활동해 왔다.
  - id: W003
    status: confirmed
    source: source_universe
    fact: 현대에 사람들의 생활이 인터넷으로 옮겨 가면서 이상현상도 디지털 매체를 통해 나타나기 시작했다.
  - id: W004
    status: confirmed
    source: source_universe
    fact: 기존 퇴마사들의 현실 의식은 인터넷의 귀신에게 영향을 끼치지 못한다. 따라서 그 방식으로 인터넷의 이상현상을 퇴마할 수 없다.
  - id: W005
    status: confirmed
    source: source_universe
    fact: 기존 감지 의식도 인터넷의 귀신에게 활용할 수 없어, 기존 퇴마사들은 인터넷 귀신의 존재를 감지하는 것조차 어렵다.
  - id: W006
    status: confirmed
    source: source_universe
    fact: 인터넷에서는 아직 해결되지 않은 미지의 사건들이 진행되고 있다.
  - id: W007
    status: confirmed
    source: source_character
    fact: 유미나는 현실의 귀신을 보지 못하지만 인터넷에서는 귀신의 본체를 유일하게 볼 수 있다.

character_background:
  subject: mina
  status: confirmed
  source: source_universe
  facts:
    - id: B001
      fact: 유미나는 명문 퇴마사 가문의 딸로 태어났다.
    - id: B002
      fact: 어린 시절부터 영적인 힘이 강하다는 평가를 받았으며, 가문은 유미나가 뛰어난 퇴마사가 되리라 기대했다.
    - id: B003
      fact: 유미나는 가문의 기대에 부응하려고 노력했지만 귀신을 보거나 괴이의 기척을 느끼지 못했다.
    - id: B004
      fact: 시간이 지나면서 가족들의 기대는 실망으로 바뀌었다. 가족들이 유미나를 미워한 것은 아니다.
    - id: B005
      fact: 유미나는 가족의 미묘한 분위기 변화를 알아차렸고, 겉으로는 밝게 웃어넘기면서도 자신이 가족을 실망시켰다는 자괴감을 품었다.
    - id: B006
      fact: 유미나는 결국 퇴마사의 꿈을 접고 프리랜서 개발자로 활동했다.

chronology:
  - id: T001
    status: confirmed
    source: source_universe
    event: 퇴마사의 꿈을 접고 프리랜서 개발자로 활동하던 유미나가 어떤 사이트의 버그 수정 의뢰를 받는다.
    participants: [mina]
  - id: T002
    status: confirmed
    source: source_universe
    event: 코드를 수정하고 정보를 찾아보지만 문제가 해결되지 않는다.
    participants: [mina]
    follows: T001
  - id: T003
    status: confirmed
    source: source_universe
    event: 유미나가 작은 귀여운 귀신이 지나가는 것을 발견하고 말을 건다. 귀신은 악하지 않아 협조적이다.
    participants: [mina, first_encounter_ghost]
    follows: T002
  - id: T004
    status: confirmed
    source: source_universe
    event: 유미나가 방에 있던 퇴마 서적을 활용해 귀신을 무사히 성불시킨다.
    participants: [mina, first_encounter_ghost]
    follows: T003
    constraint: 퇴마 프로그램을 제작하기 전의 사건이다. 당시의 구체적인 성불 방법은 미정이다.
  - id: T005
    status: confirmed
    source: source_universe
    event: 처음 귀신을 본 것이 신기했던 유미나가 부모님에게 물어본다. 부모님은 인터넷 귀신 때문에 골치를 앓고 있다는 사실과 기존 의식·감지 방식의 한계를 알려 준다.
    participants: [mina, mina_parents]
    follows: T004
    related_facts: [W004, W005]
  - id: T006
    status: confirmed
    source: source_universe
    event: 유미나는 자신만이 유일하게 할 수 있는 일이라는 것을 깨닫고, 개발자를 그만두고 퇴마사가 되기로 한다.
    participants: [mina]
    follows: T005
  - id: T007
    status: confirmed
    source: source_universe
    event: 얼마 뒤 유미나가 부모님의 퇴마 의식서에서 가장 많이 쓰인다는 의식들을 컴퓨터에서 흉내 내는 퇴마 프로그램을 만든다.
    participants: [mina, exorcism_program]
    follows: T006
  - id: T008
    status: confirmed
    source: source_universe
    event: 부모님의 부탁을 받은 린이 유미나를 찾아온다. 린은 퇴마 일에 관한 조언을 주는 역할로 왔다.
    participants: [mina, rin]
    follows: T007
  - id: T009
    status: confirmed
    source: source_universe
    event: 린은 퇴마 프로그램에 대응하는 각 퇴마 의식이 어떤 기능을 하는지 유미나에게 알려 준다.
    participants: [mina, rin, exorcism_program]
    follows: T008
  - id: T010
    status: confirmed
    source: source_universe
    event: 첫 퇴마 의뢰가 이메일로 도착한다.
    participants: [mina]
    follows: T009
    constraint: T001의 개발 업무 의뢰와 구분한다. 도입부 서술은 이 지점에서 끝난다.

program_facts:
  - id: P001
    status: confirmed
    source: source_universe
    fact: 프로그램은 부모님의 퇴마 의식서에서 자주 쓰이는 의식을 컴퓨터에서 흉내 내도록 제작되었다.
  - id: P002
    status: confirmed
    source: source_character
    fact: 현실 의식을 프로그램으로 따라 한 것이지만 성능 자체는 좋다.
  - id: P003
    status: confirmed
    source: source_universe
    fact: 린은 프로그램의 각 기능에 대응하는 퇴마 의식의 역할을 설명해 주는 사람이다.
  - id: P004
    status: confirmed
    source: source_character
    fact: 캐릭터 대화 예시에는 의뢰인에게 감지 의식 프로그램을 사용하자 숨어 있던 귀신이 등장하는 장면이 있다.
    scope: 대화 예시에서 확인된 기능이다. 도입부 연표상의 정확한 위치는 아직 정하지 않았다.

causal_links:
  - from: [W003]
    to: [W004, W005, W006]
    relation: 디지털 매체의 이상현상에 기존 퇴마 방식이 대응하지 못해 미해결 문제가 남는다.
  - from: [B003, B004]
    to: [B005, B006]
    relation: 기대에 부응하지 못한 경험과 가족의 반응이 자괴감 및 퇴마사 진로 포기의 배경이다.
  - from: [T003, T004, T005, W007]
    to: [T006]
    relation: 인터넷 귀신을 볼 수 있는 자신의 능력과 기존 퇴마사들의 한계를 알게 되어 진로를 바꾼다.
  - from: [T006]
    to: [T007]
    relation: 퇴마사가 되기로 한 뒤 개발 경험과 의식서를 활용해 프로그램을 제작한다.

writing_constraints:
  - id: C001
    rule: 가족의 실망과 유미나의 자괴감을 가족의 증오·학대·절연으로 확대하지 않는다.
    basis: [B004, B005]
  - id: C002
    rule: 영력이 강하다는 평가와 현실의 귀신을 보거나 기척을 느끼는 능력을 동일시하지 않는다.
    basis: [B002, B003, W007]
  - id: C003
    rule: 인터넷에서 귀신을 보는 능력과 현실에서 귀신을 보지 못하는 설정을 함께 유지한다. 도입부 귀신 조우를 현실 귀신을 보게 된 능력 변화로 쓰지 않는다.
    basis: [W007, T003]
  - id: C004
    rule: 유미나가 프로그램을 개발했다는 사실을 퇴마 실무 전반에 이미 능숙하다는 뜻으로 해석하지 않는다. 린이 의식의 기능을 설명하는 흐름을 유지한다.
    basis: [T007, T009]
  - id: C005
    rule: 처음 만난 협조적인 귀신의 성불과 프로그램 제작 이후 첫 이메일 퇴마 의뢰를 서로 다른 사건으로 다룬다.
    basis: [T004, T007, T010]
  - id: C006
    rule: 기존 현실 의식의 한계와 디지털로 흉내 낸 프로그램의 성능은 확정 설정으로 함께 보존하되, 둘의 차이를 설명하는 작동 원리는 임의로 추가하지 않는다.
    basis: [W004, P001, P002]
  - id: C007
    rule: 이 문서의 사건 연표와 캐릭터 대화 예시를 자동으로 같은 장면에 배정하지 않는다.
    basis: [T003, T008, P004]

unknowns:
  - id: U001
    topic: 구체적인 시대·국가·지역·가문 이름과 인물 나이
  - id: U002
    topic: 괴이와 귀신의 엄밀한 분류 및 포함 관계
  - id: U003
    topic: 귀신이 디지털 매체에 나타나는 원리와 현실·인터넷 사이의 이동 가능 여부
  - id: U004
    topic: 유미나의 영력이 강한데도 현실에서 감지하지 못하는 이유 및 인터넷에서만 본체를 보는 원리
  - id: U005
    topic: 도입부 사이트의 정체·귀신의 이름·귀신이 보이는 구체적인 화면 연출·버그의 정확한 원인
  - id: U006
    topic: 최초 성불에 사용한 서적의 소유자와 구체적인 절차. 이후 참고한 부모님의 의식서와 같은 책인지 여부
  - id: U007
    topic: 퇴마 프로그램의 이름·전체 의식 목록·각 기능·사용 조건·대가·한계·실제 작동 원리
  - id: U008
    topic: 첫 이메일 퇴마 의뢰의 세부 내용 및 기존 프로토타입 의뢰와의 대응 관계
  - id: U009
    topic: 린이 인터넷의 귀신을 직접 보거나 디지털 퇴마를 수행할 수 있는지 여부
  - id: U010
    topic: 유미나가 인터넷 괴이의 기척까지 직접 감지할 수 있는지 여부. 본체를 볼 수 있다는 사실만 확정되어 있다.
```

관련 자료: [캐릭터 설정집](Char_profile.md), [대화 CSV 가이드](../Docs/DIALOGUE_CSV_GUIDE.md).
