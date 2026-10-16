using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// CSV의 대사 한 행을 담는다. Image와 EventID는 실제 이미지나 이벤트가 아니라
/// 다른 시스템에서 찾아 사용할 이름이다.
/// </summary>
public sealed class DialogueLine
{
    /// <summary>이 대사가 속한 장면의 식별자.</summary>
    public string SceneID { get; }
    /// <summary>장면 안에서의 대사 순서. 장면마다 1부터 시작한다.</summary>
    public int Order { get; }
    /// <summary>대사를 말하는 인물의 식별자.</summary>
    public string SpeakerID { get; }
    /// <summary>화면에 표시할 대사 본문.</summary>
    public string Dialogue { get; }
    /// <summary>대사와 함께 사용할 이미지의 이름. 빈 문자열일 수 있다.</summary>
    public string Image { get; }
    /// <summary>이 대사와 연결할 이벤트의 이름. 빈 문자열일 수 있다.</summary>
    public string EventID { get; }
    /// <summary>오류를 확인할 때 참고할 원본 CSV의 시작 행 번호.</summary>
    public int SourceLine { get; }

    /// <summary>파싱한 CSV 열의 값을 대사 한 건으로 묶는다.</summary>
    /// <param name="sceneId">대사가 속한 장면 ID.</param>
    /// <param name="order">장면 안에서의 대사 순서.</param>
    /// <param name="speakerId">화자 ID.</param>
    /// <param name="dialogue">대사 본문.</param>
    /// <param name="image">이미지 이름.</param>
    /// <param name="eventId">이벤트 이름.</param>
    /// <param name="sourceLine">원본 CSV의 시작 행 번호.</param>
    internal DialogueLine(string sceneId, int order, string speakerId, string dialogue,
        string image, string eventId, int sourceLine)
    {
        SceneID = sceneId;
        Order = order;
        SpeakerID = speakerId;
        Dialogue = dialogue;
        Image = image;
        EventID = eventId;
        SourceLine = sourceLine;
    }
}

/// <summary>
/// Inspector에 지정한 CSV TextAsset을 읽고 SceneID와 Order로 대사를 찾게 해 준다.
/// 연속된 행의 SceneID는 비워 둘 수 있으며, Order는 장면마다 1부터 시작한다.
/// </summary>
public sealed class DialogueCsvReader : MonoBehaviour
{
    /// <summary>CSV의 첫 번째 내용 있는 행에 반드시 있어야 하는 열 이름들.</summary>
    private static readonly string[] RequiredColumns =
    {
        "SceneID", "Order", "SpeakerID", "Dialogue", "Image", "EventID"
    };

    // Inspector에서 연결할 CSV 파일. Unity가 .csv를 TextAsset으로 가져온다.
    [SerializeField] private TextAsset dialogueCsv;
    // 참이면 Awake에서 CSV를 읽는다. 거짓이면 Load 또는 조회 메서드의 첫 호출 때 읽는다.
    [SerializeField] private bool loadOnAwake = true;

    // 장면 식별자별 대사 목록. 목록은 Order 순으로 정렬하며 외부에서 수정할 수 없게 제공한다.
    private Dictionary<string, ReadOnlyCollection<DialogueLine>> scenes =
        new Dictionary<string, ReadOnlyCollection<DialogueLine>>(StringComparer.Ordinal);

    /// <summary>마지막 CSV 읽기가 성공했는지 나타낸다.</summary>
    public bool IsLoaded { get; private set; }
    /// <summary>Inspector에서 연결한 원본 CSV 파일.</summary>
    public TextAsset CsvAsset => dialogueCsv;
    /// <summary>읽어 들인 장면 식별자 목록.</summary>
    public IEnumerable<string> SceneIDs => scenes.Keys;

    /// <summary>설정에 따라 컴포넌트가 시작될 때 CSV를 미리 읽는다.</summary>
    private void Awake()
    {
        if (loadOnAwake && !IsLoaded)
            Load();
    }

    /// <summary>CSV를 다시 읽는다. 실패하면 이전에 읽은 데이터도 비운다.</summary>
    /// <returns>CSV를 정상적으로 읽고 검증했으면 참.</returns>
    public bool Load()
    {
        IsLoaded = false;
        scenes.Clear();

        if (dialogueCsv == null)
        {
            Debug.LogError("DialogueCsvReader: CSV TextAsset이 지정되지 않았습니다.", this);
            return false;
        }

        try
        {
            scenes = ReadDialogue(dialogueCsv.text);
            IsLoaded = true;
            return true;
        }
        // exception에는 형식 오류의 원인과 원본 행 번호가 담겨 있다.
        catch (FormatException exception)
        {
            Debug.LogError($"DialogueCsvReader: '{dialogueCsv.name}' CSV를 읽지 못했습니다. {exception.Message}", this);
            return false;
        }
    }

    /// <summary>장면에 속한 대사 전체를 Order 순서로 가져온다.</summary>
    /// <param name="sceneId">찾을 장면의 SceneID.</param>
    /// <param name="lines">성공하면 수정할 수 없는 대사 목록, 실패하면 null.</param>
    /// <returns>장면을 찾았으면 참.</returns>
    public bool TryGetScene(string sceneId, out IReadOnlyList<DialogueLine> lines)
    {
        lines = null;
        if (!IsLoaded && !Load())
            return false;

        // sceneLines는 요청한 장면에 저장된 읽기 전용 대사 목록이다.
        if (string.IsNullOrEmpty(sceneId) || !scenes.TryGetValue(sceneId, out var sceneLines))
            return false;

        lines = sceneLines;
        return true;
    }

    /// <summary>지정한 장면의 특정 순서에 해당하는 대사 한 건을 가져온다.</summary>
    /// <param name="sceneId">찾을 장면의 SceneID.</param>
    /// <param name="order">장면 안에서의 Order 값.</param>
    /// <param name="line">성공하면 대사 데이터, 실패하면 null.</param>
    /// <returns>해당 대사를 찾았으면 참.</returns>
    public bool TryGetLine(string sceneId, int order, out DialogueLine line)
    {
        line = null;
        // lines는 해당 장면의 전체 대사다. Order가 1부터 연속되므로 위치는 order - 1이다.
        if (order < 1 || !TryGetScene(sceneId, out var lines) || order > lines.Count)
            return false;

        line = lines[order - 1];
        return true;
    }

    /// <summary>CSV 레코드를 열 이름에 맞춰 읽고 장면별 대사 목록으로 변환한다.</summary>
    /// <param name="csv">TextAsset에서 가져온 CSV 전체 문자열.</param>
    /// <returns>장면 ID를 키로 사용하는, Order 순서의 읽기 전용 대사 목록.</returns>
    private static Dictionary<string, ReadOnlyCollection<DialogueLine>> ReadDialogue(string csv)
    {
        // records는 큰따옴표 안의 줄바꿈까지 처리한 CSV 행 목록이다.
        var records = ReadCsvRecords(csv);
        // headerRecord는 앞쪽 빈 행을 제외한 첫 헤더 행의 목록 위치다.
        var headerRecord = 0;
        while (headerRecord < records.Count && IsBlank(records[headerRecord]))
            headerRecord++;

        if (headerRecord == records.Count)
            throw new FormatException("헤더 행이 없습니다.");

        // header는 열 이름이 있는 행, columns는 열 이름에서 열 번호로 가는 표다.
        var header = records[headerRecord];
        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        // i는 헤더에서 검사할 열의 위치다.
        for (var i = 0; i < header.Fields.Count; i++)
        {
            // name은 앞뒤 공백과 UTF-8 BOM 표시를 제거한 열 이름이다.
            var name = header.Fields[i].Trim().TrimStart('\uFEFF');
            if (name.Length == 0)
                continue;

            if (columns.ContainsKey(name))
                throw new FormatException($"{header.Line}행: 중복된 열 이름 '{name}'입니다.");
            columns.Add(name, i);
        }

        // required는 누락 여부를 확인할 필수 열 이름이다.
        foreach (var required in RequiredColumns)
        {
            if (!columns.ContainsKey(required))
                throw new FormatException($"{header.Line}행: 필수 열 '{required}'이(가) 없습니다.");
        }

        // grouped는 검증·정렬 전의 장면별 대사 목록이다.
        var grouped = new Dictionary<string, List<DialogueLine>>(StringComparer.Ordinal);
        // sceneId는 마지막으로 명시된 장면 ID이며, 빈 SceneID 행에서 이어서 사용한다.
        var sceneId = string.Empty;

        // i는 헤더 다음부터 검사할 CSV 행의 목록 위치다.
        for (var i = headerRecord + 1; i < records.Count; i++)
        {
            // record는 현재 CSV 행이다. 비어 있는 행은 대사로 취급하지 않는다.
            var record = records[i];
            if (IsBlank(record))
                continue;

            if (record.Fields.Count > header.Fields.Count)
                throw new FormatException($"{record.Line}행: 헤더보다 열이 많습니다. 대사에 쉼표가 있다면 큰따옴표로 감싸세요.");

            // explicitSceneId가 비어 있으면 직전에 읽은 sceneId를 이어받는다.
            var explicitSceneId = GetCell(record, columns["SceneID"]).Trim();
            if (explicitSceneId.Length > 0)
                sceneId = explicitSceneId;

            // orderText는 숫자로 바꾸기 전의 Order 셀, dialogue는 원본 대사 셀이다.
            var orderText = GetCell(record, columns["Order"]).Trim();
            var dialogue = GetCell(record, columns["Dialogue"]);

            // Order가 없는 행은 장면 제목이나 메모·연출 설명 행일 수 있다.
            if (orderText.Length == 0)
            {
                if (!string.IsNullOrWhiteSpace(dialogue))
                    throw new FormatException($"{record.Line}행: Dialogue가 있지만 Order가 비어 있습니다.");
                continue;
            }

            if (sceneId.Length == 0)
                throw new FormatException($"{record.Line}행: 첫 대사보다 앞에 SceneID를 지정해야 합니다.");

            // order는 정수로 변환한 대사 순서다.
            if (!int.TryParse(orderText, NumberStyles.None, CultureInfo.InvariantCulture, out var order) || order < 1)
                throw new FormatException($"{record.Line}행: Order는 1 이상의 정수여야 합니다.");

            // lines는 현재 장면의 대사 목록이다. 첫 대사라면 새 목록을 만든다.
            if (!grouped.TryGetValue(sceneId, out var lines))
            {
                lines = new List<DialogueLine>();
                grouped.Add(sceneId, lines);
            }

            lines.Add(new DialogueLine(sceneId, order,
                GetCell(record, columns["SpeakerID"]).Trim(), dialogue,
                GetCell(record, columns["Image"]).Trim(),
                GetCell(record, columns["EventID"]).Trim(), record.Line));
        }

        if (grouped.Count == 0)
            throw new FormatException("Order가 있는 대사 행이 없습니다.");

        // result는 정렬·검증을 마친 뒤 조회 메서드에 제공할 최종 목록이다.
        var result = new Dictionary<string, ReadOnlyCollection<DialogueLine>>(StringComparer.Ordinal);
        foreach (var pair in grouped)
        {
            // pair는 장면 ID와 그 장면의 대사 목록이다. i는 예상 Order의 0부터 시작하는 위치다.
            pair.Value.Sort((left, right) => left.Order.CompareTo(right.Order));
            for (var i = 0; i < pair.Value.Count; i++)
            {
                if (pair.Value[i].Order != i + 1)
                    throw new FormatException($"{pair.Value[i].SourceLine}행: SceneID '{pair.Key}'의 Order는 1부터 중복이나 누락 없이 이어져야 합니다. 예상값: {i + 1}, 실제값: {pair.Value[i].Order}.");
            }

            result.Add(pair.Key, pair.Value.AsReadOnly());
        }

        return result;
    }

    /// <summary>행에서 지정한 열을 읽는다. 끝에 생략된 빈 셀은 빈 문자열로 처리한다.</summary>
    /// <param name="record">읽을 CSV 행.</param>
    /// <param name="column">읽을 열 번호.</param>
    /// <returns>해당 셀의 문자열 또는 빈 문자열.</returns>
    private static string GetCell(CsvRecord record, int column)
    {
        return column < record.Fields.Count ? record.Fields[column] : string.Empty;
    }

    /// <summary>행의 모든 셀이 비어 있거나 공백만 있는지 확인한다.</summary>
    /// <param name="record">검사할 CSV 행.</param>
    /// <returns>행이 비어 있으면 참.</returns>
    private static bool IsBlank(CsvRecord record)
    {
        // field는 현재 검사 중인 셀이다. 내용이 하나라도 있으면 빈 행이 아니다.
        foreach (var field in record.Fields)
        {
            if (!string.IsNullOrWhiteSpace(field))
                return false;
        }

        return true;
    }

    /// <summary>CSV 한 행의 시작 줄 번호와 셀 문자열들을 담는 내부 자료형.</summary>
    private sealed class CsvRecord
    {
        /// <summary>원본 CSV에서 이 행이 시작된 줄 번호.</summary>
        public readonly int Line;
        /// <summary>쉼표와 큰따옴표 규칙을 처리한 각 셀의 문자열.</summary>
        public readonly List<string> Fields;

        /// <summary>읽은 줄 번호와 셀 목록을 CSV 행으로 묶는다.</summary>
        /// <param name="line">원본 CSV에서 행이 시작된 줄 번호.</param>
        /// <param name="fields">행에서 읽은 셀 목록.</param>
        public CsvRecord(int line, List<string> fields)
        {
            Line = line;
            Fields = fields;
        }
    }

    /// <summary>CSV 문자열을 행과 셀로 나눈다. 인용된 셀 안의 쉼표·줄바꿈·큰따옴표를 처리한다.</summary>
    /// <param name="csv">분리할 CSV 전체 문자열.</param>
    /// <returns>원본의 줄 번호를 함께 보관한 CSV 행 목록.</returns>
    private static List<CsvRecord> ReadCsvRecords(string csv)
    {
        // records는 완료된 행 목록, fields는 현재 행의 완료된 셀 목록이다.
        var records = new List<CsvRecord>();
        var fields = new List<string>();
        // field는 현재 셀의 문자들을 모은다.
        var field = new StringBuilder();
        // inQuotes는 인용된 셀 내부인지, afterQuote는 닫는 큰따옴표를 막 지난 상태인지 나타낸다.
        var inQuotes = false;
        var afterQuote = false;
        // line은 현재 물리적 줄 번호, recordLine은 현재 CSV 행이 시작된 줄 번호다.
        var line = 1;
        var recordLine = 1;

        // i는 원본 문자열의 문자 위치, character는 지금 검사하는 문자다.
        for (var i = 0; i < csv.Length; i++)
        {
            var character = csv[i];
            if (inQuotes)
            {
                // 인용된 셀 안에서는 쉼표를 내용으로 두고, 연속된 큰따옴표는 한 개로 바꾼다.
                if (character == '"')
                {
                    if (i + 1 < csv.Length && csv[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                        afterQuote = true;
                    }
                }
                else if (character == '\r' || character == '\n')
                {
                    if (character == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                        i++;
                    field.Append('\n');
                    line++;
                }
                else
                {
                    field.Append(character);
                }

                continue;
            }

            if (character == ',')
            {
                // 쉼표를 만나면 현재 셀을 마치고 다음 셀을 준비한다.
                fields.Add(field.ToString());
                field.Clear();
                afterQuote = false;
            }
            else if (character == '\r' || character == '\n')
            {
                // 인용 밖의 줄바꿈은 현재 CSV 행의 끝이다.
                if (character == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                    i++;
                fields.Add(field.ToString());
                records.Add(new CsvRecord(recordLine, new List<string>(fields)));
                fields.Clear();
                field.Clear();
                afterQuote = false;
                line++;
                recordLine = line;
            }
            else if (character == '"')
            {
                // 큰따옴표는 셀의 첫 문자일 때만 인용 시작으로 인정한다.
                if (afterQuote || field.Length > 0)
                    throw new FormatException($"{line}행: 큰따옴표 위치가 잘못되었습니다.");
                inQuotes = true;
            }
            else if (afterQuote)
            {
                // 인용이 끝난 뒤에는 구분자까지 공백만 허용한다.
                if (character != ' ' && character != '\t')
                    throw new FormatException($"{line}행: 닫는 큰따옴표 뒤에는 쉼표 또는 줄바꿈이 와야 합니다.");
            }
            else
            {
                field.Append(character);
            }
        }

        if (inQuotes)
            throw new FormatException($"{recordLine}행: 닫히지 않은 큰따옴표가 있습니다.");

        if (fields.Count > 0 || field.Length > 0 || afterQuote)
        {
            fields.Add(field.ToString());
            records.Add(new CsvRecord(recordLine, fields));
        }

        return records;
    }
}
