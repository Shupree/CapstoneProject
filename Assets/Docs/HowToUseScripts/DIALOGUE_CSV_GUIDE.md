# 대사 CSV 사용 가이드

이 문서는 팀원이 정리한 `엑셀 파일 사용법.txt`를 프로젝트 안에서 공유하기 위한 안내다. 현재 구현은 [DialogueCsvReader.cs](../../Scripts/DialogueReader/DialogueCsvReader.cs)를 사용한다. CSV를 사용하는 코드를 작성하기 전에 실제 리더와 호출부도 확인한다.

## CSV 열의 의미

| 열 | 의미 |
| --- | --- |
| `SceneID` | 대화 묶음을 찾는 ID. Unity 씬 이름과 자동으로 연결되지 않는다. |
| `Order` | 같은 `SceneID` 안에서의 대사 순서. 각 묶음마다 1부터 시작한다. |
| `SpeakerID` | 화자 ID 또는 화면에 표시할 이름. |
| `Dialogue` | 대사 본문. |
| `Image` | 이미지 이름 또는 ID. 리더가 Sprite를 자동으로 찾지 않는다. |
| `EventID` | 이벤트 이름 또는 ID. 리더가 이벤트를 자동으로 실행하지 않는다. |

첫 번째 내용 있는 행에는 위 여섯 열의 헤더가 모두 필요하다. 열 순서는 달라도 되고 추가 열은 무시된다. `Image`와 `EventID` 셀은 비어 있어도 된다.

## Excel에서 CSV 준비하기

1. Excel에서 대사를 작성하고 **CSV UTF-8(쉼표로 분리)** 형식의 `.csv`로 저장한다. `.xlsx` 파일은 현재 리더가 직접 읽지 않는다.
2. CSV를 `Assets` 아래에 넣는다. 현재 예시는 [Explore_FirstCall.csv](../../Explore_Proto/Data/Explore_FirstCall.csv)다.
3. 한 파일에 여러 대화를 넣을 수 있다. `Explore_FirstCall`의 `Order`가 1~9라면 `Explore_SecondCall`은 다시 1부터 시작한다. 같은 `SceneID`에서 번호가 중복되거나 빠지면 CSV 전체 읽기가 실패한다.
4. `SceneID`가 빈 행은 앞에서 마지막으로 지정한 ID를 이어받는다. 첫 대사에는 ID가 필요하다. `Order`가 빈 설명 행은 건너뛰지만, `Dialogue`가 있는데 `Order`가 비어 있으면 오류가 난다.
5. 쉼표나 줄바꿈이 포함된 셀은 큰따옴표로 감싸고, 셀 안의 큰따옴표는 두 번 쓴다. Excel의 CSV 저장 기능을 쓰면 이 처리를 맡길 수 있다.

한글이 깨진다면 Excel의 **데이터 → 텍스트/CSV에서**를 통해 파일 원본을 UTF-8, 구분 기호를 쉼표로 지정해 연다. 현재 `Explore_FirstCall.csv`는 Excel의 자동 인식을 돕는 UTF-8 BOM을 포함한다. 수정 후 Unity에서 사용할 파일도 CSV UTF-8로 다시 저장한다.

```csv
SceneID,Order,SpeakerID,Dialogue,Image,EventID
Explore_FirstCall,1,시율,첫 통화 대사,Client_1,
Explore_FirstCall,2,주인공,첫 통화 답변,Heroine_1,
Explore_SecondCall,1,시율,두 번째 통화 대사,Client_1,
Explore_SecondCall,2,주인공,두 번째 통화 답변,Heroine_1,
```

## Unity에서 연결하고 조회하기

1. Hierarchy의 GameObject에 `DialogueCsvReader` 컴포넌트를 붙인다.
2. Inspector의 **Dialogue Csv**에 `Assets` 안의 CSV를 연결한다. **Load On Awake**를 켜면 `Awake`에서 읽고, 꺼도 첫 조회 때 자동으로 읽는다.
3. 대사를 사용할 스크립트에서 이 컴포넌트를 참조한다. 다른 Unity 씬에서 사용할 때에는 그 씬에서도 리더와 CSV 참조가 필요하다.
4. 대화를 시작하는 코드에서 `TryGetScene(sceneId, out var lines)`로 원하는 `SceneID`를 명시한다. 한 줄만 필요하면 `TryGetLine(sceneId, order, out var line)`을 사용한다. ID의 대소문자는 구분된다.
5. 반환된 `DialogueLine`의 `SpeakerID`, `Dialogue`, `Image`, `EventID`를 사용할 UI나 시스템에 연결한다. `TryGetScene`은 `Order` 순으로 정렬한 읽기 전용 목록을 돌려준다.

아래는 `DialogueCsvUsageExample.cs`로 작성할 수 있는 **사용 예시**다. Inspector에서 리더와 텍스트 필드를 연결하고, 원하는 시점에 `StartConversation("Explore_FirstCall")` 또는 `StartConversation("Explore_SecondCall")`을 호출한다. `NextDialogue()`는 다음 대사 버튼 등에 연결할 수 있다.

```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueCsvUsageExample : MonoBehaviour
{
    [SerializeField] private DialogueCsvReader dialogueReader;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    private IReadOnlyList<DialogueLine> lines;
    private int index;

    public void StartConversation(string sceneId)
    {
        if (dialogueReader == null ||
            !dialogueReader.TryGetScene(sceneId, out var loadedLines) ||
            loadedLines.Count == 0)
        {
            lines = null;
            Debug.LogError($"대사를 찾지 못했습니다: {sceneId}", this);
            return;
        }

        lines = loadedLines;
        index = 0;
        ShowCurrentLine();
    }

    public void NextDialogue()
    {
        if (lines == null) return;
        index++;
        if (index >= lines.Count)
        {
            lines = null; // 실제 게임에서는 여기서 대화 종료 처리를 한다.
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        var line = lines[index];
        speakerText.text = line.SpeakerID;
        dialogueText.text = line.Dialogue;
        // line.Image와 line.EventID는 사용하는 시스템에서 별도로 해석한다.
    }
}
```

모든 대사를 `foreach`로 순회하며 같은 텍스트 UI에 대입하면 마지막 대사만 남는다. 클릭마다 한 줄씩 보여줄 때에는 위처럼 현재 위치를 유지한다.

## 현재 프로젝트의 적용 범위

[ExploreProtoMessenger.cs](../../Explore_Proto/Scripts/ExploreProtoMessenger.cs)는 현재 `Explore_FirstCall`을 고정해서 요청한다. 받은 `SpeakerID`와 `Dialogue`를 첫 화상통화 UI와 녹음본에 사용하고, `Image`의 `Heroine_1`과 `Client_1`을 Inspector에 연결된 Sprite에 대응시킨다. `EventID`는 자동 실행하지 않는다. 다른 통화를 같은 CSV에 추가하더라도 그 통화가 시작될 때 새로운 `SceneID`를 요청하고 결과를 표시하는 호출 코드가 필요하다.

리더는 지정된 CSV를 한 번 파싱해 `SceneID`별 목록을 보관한다. CSV 형식에 오류가 있거나 요청한 ID가 없으면 조회가 실패하므로 호출부에서 실패를 처리한다. 리더의 동작을 중복 구현하기 전에 기존 `TryGetScene`·`TryGetLine`을 사용한다.
