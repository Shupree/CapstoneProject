using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class ExploreLinkCase : MonoBehaviour
{
    public GameObject internet;
    public TMP_Text article, conversation, toolbar;
    public TMP_InputField address;
    readonly List<string> history = new List<string> { "browser.local" };
    int position;
    [Serializable] class Page { public string url, body, heading, meta, content, comments; }
    [Serializable] class Catalog { public Page[] pages; }
    public GameObject communityRoot;
    public TMP_Text postTitle, postMeta, postBody, postComments;
    readonly Dictionary<string, Page> threads = new Dictionary<string, Page>();
    public TextAsset pageCatalog;
    readonly Dictionary<string, string> pages = new Dictionary<string, string>();
    readonly HashSet<string> findings = new HashSet<string>();
    string chatThought;
    bool receivedAddress, failed, asked, askedTaboo, confessed;
    string observation;
    int sheet = 1;

    public void TurnSheet(int delta)
    {
        sheet = Mathf.Clamp(sheet + delta, 1, Mathf.Max(1, article.textInfo.pageCount));
        article.pageToDisplay = sheet;
    }

    static string Link(string destination, string label) =>
        $"<link=\"{destination}\"><color=#386489><u>{label}</u></color></link>";

    void RenderConversation()
    {
        const string greeting = "저, 메일 보낸 이서연이에요. 연락 주셔서 감사해요.";
        const string request = "그날 보신 사이트 주소를 보내 주실래요?";
        const string question = "주소가 열리지 않아요. 어디서 찾으셨어요?";
        conversation.text = greeting + "\n\n";
        if (!receivedAddress)
        {
            conversation.text += Link("askAddress", request);
            return;
        }
        conversation.text += request + "\n\n네. 그날은 여기로 들어갔어요. " + Link("bloody467.org","Bloody467.org");
        if (asked)
            conversation.text += "\n\n" + question + "\n\n그날은 분명히 됐는데… 이상하네요. 여기서 봤어요. " +
                Link("community.local/thread/467","community.local/thread/467");
        else if (failed)
            conversation.text += "\n\n" + Link("ask",question);
        if (chatThought != null) conversation.text += "\n\n<size=85%>" + chatThought + "</size>";
        if (findings.Contains("taboo"))
            conversation.text += "\n\n" + (askedTaboo ? "그날 누가 금지어를 말했나요?\n서연   …모르겠네요." : Link("askTaboo","그날 누가 금지어를 말했나요?"));
        if (askedTaboo && findings.Contains("question"))
            conversation.text += "\n\n" + (confessed ? "서연   …네. 그 글은 제가 썼어요. 친구를 놀래키려고 제가 그 말을 했어요." : Link("confront","부작용을 묻는 게시글을 보여 준다."));
    }

    void Awake()
    {
        // Resolve scene-authored controls even when the editor reloads an older set of serialized bindings.
        var canvas = internet.GetComponentInParent<Canvas>(true);
        foreach (var label in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            switch (label.name)
            {
                case "ClientMessages": conversation = label; break;
                case "LinkArticle": article = label; break;
                case "LinkNavigation": toolbar = label; break;
                case "ThreadTitle": postTitle = label; break;
                case "ThreadAuthor": postMeta = label; break;
                case "ThreadBody": postBody = label; break;
                case "ThreadComments": postComments = label; break;
            }
        }
        if (!communityRoot) communityRoot = internet.transform.Find("LinkBrowserSurface/CommunityPage")?.gameObject;
        if (!address) address = internet.GetComponentInChildren<TMP_InputField>(true);
        RenderConversation();
    }

    void Start()
    {
        if (!pageCatalog) pageCatalog = Resources.Load<TextAsset>("ExploreBrowserPages");
        foreach (var page in JsonUtility.FromJson<Catalog>(pageCatalog.text).pages)
        {
            pages.Add(page.url, page.body);
            if (!string.IsNullOrEmpty(page.heading)) threads.Add(page.url, page);
        }
        if (postBody && postComments) postBody.overflowMode = postComments.overflowMode = TextOverflowModes.Page;
        article.overflowMode = TextOverflowModes.Page;
        if (address) address.onSubmit.AddListener(Submit);
        Render();
    }

    void OnDestroy() { if (address) address.onSubmit.RemoveListener(Submit); }

    public void Follow(string id)
    {
        sheet = 1;
        observation = null;
        if (id == "askAddress")
        {
            receivedAddress = true;
            chatThought = "이게 그날 서연이 접속한 주소구나. 직접 열어 보자.";
        }
        else if (id.StartsWith("inspect/"))
        {
            string key = id.Substring(8);
            findings.Add(key);
            observation = Observation(key);
        }
        else if (id == "askTaboo") askedTaboo = findings.Contains("taboo");
        else if (id == "confront") confessed = askedTaboo && findings.Contains("question");
        else if (id == "ask")
        {
            if (receivedAddress && failed)
            {
                asked = true;
                chatThought = "서연도 이 글을 보고 들어갔구나. 주소를 올린 사람부터 살펴보자.";
            }
        }
        else if (id == "chat")
        {
            GetComponent<ExploreProtoMessenger>().ShowMessenger();
        }
        else if (id == "back") position = Mathf.Max(0, position - 1);
        else if (id == "forward") position = Mathf.Min(history.Count - 1, position + 1);
        else
        {
            internet.SetActive(true);
            internet.transform.SetAsLastSibling();
            if (history[position] != id)
            {
                history.RemoveRange(position + 1, history.Count - position - 1);
                history.Add(id);
                position++;
            }
            if (id == "bloody467.org" && !failed)
            {
                failed = true;
                observation = Observation("oldAddress");
                chatThought = null;
            }
        }
        Render();
    }

    void Submit(string value)
    {
        string url = value.Trim();
        foreach (var prefix in new[] { "https://", "http://" })
            if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) url = url.Substring(prefix.Length);
        if (url.Length == 0) return;
        url = url.TrimEnd('/').ToLowerInvariant();
        // Keep typed addresses separate from toolbar and conversation actions.
        if (!url.Contains(".")) url = url + ".invalid";
        Follow(url);
    }

    void Render()
    {
        string url = history[position];
        if (address) address.SetTextWithoutNotify(url);
        toolbar.text = (position > 0 ? Link("back","←") : "←") + " " +
            (position + 1 < history.Count ? Link("forward","→") : "→") + " " + Link("browser.local","홈");
        RenderConversation();
        string key = confessed && url == "bloody687.org" ? "bloody687.org/expired" : url;
        pages.TryGetValue(key, out var body);
        article.pageToDisplay = sheet;
        bool isThread = threads.TryGetValue(key, out var thread) && communityRoot && postTitle && postMeta && postBody && postComments;
        if (communityRoot) communityRoot.SetActive(isThread);
        article.gameObject.SetActive(!isThread);
        if (isThread)
        {
            postTitle.text = thread.heading;
            postMeta.text = thread.meta;
            postBody.text = (observation == null ? "" : observation + "\n") + thread.content;
            postComments.text = thread.comments;
            postBody.pageToDisplay = postComments.pageToDisplay = 1;
        }
        article.text = (observation == null ? "" : "조사   " + observation + "\n\n") +
            (body ?? "사이트를 찾을 수 없습니다. 주소를 확인해 주세요.");
    }

    static string Observation(string key) => key switch
    {
        "oldAddress" => "여기서는 열리지 않네. 서연에게 이 주소를 어디서 찾았는지 물어보자.",
        "question" => "음.. 오히려 강령술보다 부작용에 더 관심이 있어보이네..?",
        "current" => "예전 게시물에는 467이 있었지. 프로필 주소에서 XXX 자리에 이번 숫자를 넣으면 어떨까?",
        "older" => "아이콘은 그대로인데 숫자가 계속 바뀌네. 게시 날짜를 봐야겠어.",
        "camera" => "캠이 거울 역할을 하는구나. 내 모습이 화면에 나와야 해.",
        "taboo" => "설마 친구분의 강령술 때 누군가 이 말을 한 것일까?",
        "knife" => "칼이 보이면 통화를 끊는다. 실제로 마주치면 망설일 시간이 없겠어.",
        "hope" => "“네 아기는 죽지 않았어.” 메리에게 닿을 말이 하나는 있네.",
        "eggs" => "달걀 셋 다 묵비권을 행사했군. 냉장고 사건은 여기까지.",
        "cup" => "손잡이가 귀처럼 보였네. 컵 하나에 이렇게 놀랄 줄이야.",
        _ => ""
    };
}
