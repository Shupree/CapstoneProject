# 스토리·스크립트 작업 안내

## 적용 범위와 문서 참조

- 이 파일의 규칙은 스토리·대사 문서 작업에만 적용한다. 여기서 ‘스크립트’는 대사·연출 대본을 뜻하며 C# 코드를 뜻하지 않는다.
- 일반 C# 코드·씬·UI 개발 작업에는 아래 스토리 규칙을 적용하지 않으며, 이 파일을 이유로 연결된 문서를 읽지 않는다. 참조 제외 대상은 STORY_WRITING_RULES.md, Universe.md, Char_profile.md, story_index.md, Story_Settings.md, 세부 대본 MD·CSV 및 Dialogue_CSV_Format.md다.
- 사용자가 해당 문서 참조를 요청하거나 개발 작업에 대사 데이터 연동이 필요한 경우에만, 요청이나 연동에 필요한 문서를 한정해서 확인한다. 연결된 문서 전체를 연쇄적으로 읽지 않는다.
- 이 제한은 스토리용 문서 참조에만 적용한다. 상위 AGENTS.md 등 기존 프로젝트 개발 규칙은 계속 적용한다.

## 스토리·대사 작업 규칙

이 폴더에서 스토리 및 대사 작업을 할 때 [STORY_WRITING_RULES.md](STORY_WRITING_RULES.md)를 먼저 읽는다.

- 전체 설정은 Universe.md와 Char_profile.md, 파트 흐름은 story_index.md 및 대상 파트의 Story_Settings.md를 기준으로 한다.
- 새 스크립트 작성이나 story_index 갱신을 위해 기존 세부 스크립트들을 읽지 않는다.
- Universe와 Char_profile은 사용자가 해당 문서의 갱신을 요청했을 때만 수정한다. 스크립트가 설정과 충돌하면 갱신 여부를 묻는다.
- MD를 먼저 작성하고, CSV는 변환 요청 때만 Dialogue_CSV_Format.md에 맞춰 만든다.
