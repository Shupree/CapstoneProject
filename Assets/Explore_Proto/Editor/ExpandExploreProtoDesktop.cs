using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ExpandExploreProtoDesktop
{
    const string SpriteRoot="Assets/Sprite/ProtoType/";
    static Sprite Spr(string file)=>AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot+file+".png");
    static Transform Rect(Transform parent,string name,Vector2 pos,Vector2 size)
    {
        var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);
        var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    static UnityEngine.UI.Image Graphic(Transform parent,string name,Vector2 pos,Vector2 size,Color color,Sprite sprite=null,bool raycast=false)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(UnityEngine.UI.Image));go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;
        var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.sprite=sprite;image.preserveAspect=sprite!=null;image.raycastTarget=raycast;return image;
    }
    static TMP_Text Label(Transform parent,string name,string value,Vector2 pos,Vector2 size,int font,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;
        var t=go.GetComponent<TextMeshProUGUI>();t.text=value;t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GalmuriMono11 SDF 1.asset");t.fontSize=font;t.color=color;t.alignment=align;t.enableWordWrapping=true;t.raycastTarget=false;return t;
    }
    static Button Btn(Transform p,string name,string label,Vector2 pos,Vector2 size,Color color,int font=34,Sprite sprite=null)
    {
        var image=Graphic(p,name,pos,size,color,sprite,true);var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;
        if(!string.IsNullOrEmpty(label))Label(image.transform,"Label",label,Vector2.zero,size-new Vector2(20,10),font,new Color(.18f,.16f,.16f),TextAlignmentOptions.Center);
        return b;
    }
    static void AddDrag(Transform window,Transform hitArea)
    {var drag=hitArea.gameObject.AddComponent<ExploreProtoWindowDrag>();drag.window=window as RectTransform;}
    static Button CloseX(Transform parent,string name,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action)
    {
        var b=Btn(parent,name,"",pos,size,new Color(1,1,1,0));b.transition=Button.Transition.None;b.onClick.AddListener(action);return b;
    }
    static Button VisibleClose(Transform parent,string name,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action)
    {var b=Btn(parent,name,"×",pos,size,new Color(.16f,.15f,.15f),62);b.transition=Button.Transition.None;b.onClick.AddListener(action);return b;}
    [MenuItem("Explore Proto/Expand Desktop Apps")]
    public static void Build()
    {
        var scene=SceneManager.GetSceneByPath("Assets/Scenes/Explore_Proto.unity");
        if(!scene.IsValid()||scene.path!="Assets/Scenes/Explore_Proto.unity")throw new System.Exception("Open Assets/Scenes/Explore_Proto.unity first.");
        var canvas=GameObject.Find("ExploreProtoCanvas");var root=canvas.transform;
        if(root.Find("MemoWindow")||root.Find("InternetWindow"))throw new System.Exception("Memo or Internet UI already exists. Edit the saved scene instead of rebuilding it.");
        var ctrl=Object.FindObjectOfType<ExploreProtoMessenger>();if(!ctrl)throw new System.Exception("ExploreProtoMessenger is missing.");
        var dark=new Color(.19f,.17f,.16f);var ink=new Color(.20f,.18f,.17f);var paper=new Color(.99f,.96f,.91f);

        // Replace the plain messenger panel with the supplied window frame and preserve its contents.
        var messenger=ctrl.messengerWindow.transform;var messengerImage=messenger.GetComponent<UnityEngine.UI.Image>();
        messengerImage.sprite=Spr("Messenger_Window");messengerImage.color=Color.white;messengerImage.preserveAspect=true;
        var title=messenger.Find("TitleBar");var titleImage=title.GetComponent<UnityEngine.UI.Image>();titleImage.color=new Color(1,1,1,0);titleImage.raycastTarget=true;
        title.Find("AppTitle").GetComponent<TMP_Text>().text="";
        var close=title.Find("CloseMessenger");close.GetComponent<UnityEngine.UI.Image>().color=new Color(1,1,1,0);close.GetComponent<Button>().transition=Button.Transition.None;close.Find("Label").gameObject.SetActive(false);
        var contactHeader=messenger.Find("ContactPane/ContactHeader");contactHeader.gameObject.SetActive(false);
        messenger.Find("ContactPane").GetComponent<UnityEngine.UI.Image>().color=new Color(1,1,1,0);
        messenger.Find("ConversationPane").GetComponent<UnityEngine.UI.Image>().color=new Color(1,1,1,0);
        messenger.Find("ConversationPane/ConversationHeader").GetComponent<TMP_Text>().text="시율  ·  온라인";
        messenger.Find("ConversationPane/InternetLink").name="AddBloodyMaryKeyword";
        var add=messenger.Find("ConversationPane/AddBloodyMaryKeyword").GetComponent<Button>();
        add.GetComponentInChildren<TMP_Text>().text="'블러디 메리' 키워드 추가";add.interactable=true;
        messenger.Find("ConversationPane/LinkMessage").GetComponent<TMP_Text>().text="통화 내용을 기억할 수 있도록 메모장에 키워드를 적어 두자.";
        AddDrag(messenger,title);

        // Memo app: keep only the keyword tab; the article area remains empty until later research.
        var memoImage=Graphic(root,"MemoWindow",new Vector2(90,-5),new Vector2(2520,1410),Color.white,Spr("Memo_Window"));
        var memo=memoImage.transform;memo.gameObject.SetActive(false);
        var memoHeader=Graphic(memo,"MemoTitleDragArea",new Vector2(0,650),new Vector2(2130,105),new Color(1,1,1,0),null,true).transform;
        AddDrag(memo,memoHeader);
        var memoX=CloseX(memo,"MemoCloseX",new Vector2(1050,650),new Vector2(112,102),ctrl.CloseMemo);
        var keyword=Label(memo,"MemoKeyword","블러디 메리",new Vector2(-1055,530),new Vector2(220,90),36,ink,TextAlignmentOptions.Center);
        keyword.gameObject.SetActive(false);

        // Internet app uses the provided frame and a proper clipped, scrollable static results list.
        var browserImage=Graphic(root,"InternetWindow",new Vector2(-45,0),new Vector2(3040,1704),Color.white,Spr("Internet_Window"));
        var browser=browserImage.transform;browser.gameObject.SetActive(false);
        var browserDrag=Graphic(browser,"InternetTitleDragArea",new Vector2(-90,790),new Vector2(2570,112),new Color(1,1,1,0),null,true).transform;
        AddDrag(browser,browserDrag);
        CloseX(browser,"InternetCloseX",new Vector2(1340,795),new Vector2(128,112),ctrl.CloseInternet);

        // Keyword selector overlays the empty search field; the field itself opens it.
        Graphic(browser,"SearchFieldOutline",new Vector2(-18,405),new Vector2(1880,142),Color.white,Spr("UI_Find"));
        var keywordField=Graphic(browser,"KeywordFieldHitArea",new Vector2(35,405),new Vector2(1660,128),new Color(1,1,1,0),null,true);
        var keywordFieldButton=keywordField.gameObject.AddComponent<Button>();keywordFieldButton.targetGraphic=keywordField;
        var keywordChip=Graphic(browser,"SearchKeywordChip",new Vector2(-475,405),new Vector2(390,100),Color.white,Spr("UI_Keyword"));
        var clear=Btn(browser,"ClearKeyword","",new Vector2(-300,405),new Vector2(72,72),new Color(1,1,1,0),30,Spr("Icon_Erase"));clear.transition=Button.Transition.None;clear.gameObject.SetActive(false);
        var find=Btn(browser,"Icon_Find","",new Vector2(-850,405),new Vector2(92,92),Color.white,30,Spr("Icon_Find"));find.transition=Button.Transition.None;
        var keywordValue=Label(keywordChip.transform,"SearchKeywordText","블러디 메리",Vector2.zero,new Vector2(305,82),36,ink,TextAlignmentOptions.Center);keywordValue.gameObject.SetActive(false);
        var placeholder=Label(browser,"SearchFieldPlaceholder","검색할 키워드를 선택하세요",new Vector2(190,405),new Vector2(900,84),34,new Color(.48f,.43f,.40f));
        var hint=Label(browser,"SearchHint","획득한 키워드를 선택해 검색하세요.",new Vector2(-385,285),new Vector2(1300,80),38,ink);

        var popup=Rect(root,"KeywordPopup",new Vector2(-520,115),new Vector2(740,420));
        Graphic(popup,"PopupBackground",Vector2.zero,new Vector2(740,420),paper);
        Label(popup,"PopupTitle","보유한 키워드",new Vector2(-30,135),new Vector2(520,78),40,ink,TextAlignmentOptions.Center);
        var popupClose=Btn(popup,"PopupClose","×",new Vector2(310,155),new Vector2(70,70),new Color(.78f,.74f,.69f),36);
        var popupKeyword=Btn(popup,"PopupKeyword","블러디 메리",new Vector2(-20,10),new Vector2(530,115),paper,38,Spr("UI_Keyword"));
        popup.gameObject.SetActive(false);

        var scrollGo=new GameObject("SearchResults",typeof(RectTransform),typeof(CanvasRenderer),typeof(UnityEngine.UI.Image),typeof(ScrollRect));scrollGo.transform.SetParent(browser,false);
        var scrollRect=(RectTransform)scrollGo.transform;scrollRect.anchorMin=scrollRect.anchorMax=new Vector2(.5f,.5f);scrollRect.pivot=new Vector2(.5f,.5f);scrollRect.anchoredPosition=new Vector2(0,-300);scrollRect.sizeDelta=new Vector2(2840,1010);
        var scrollGraphic=scrollGo.GetComponent<UnityEngine.UI.Image>();scrollGraphic.color=new Color(1,1,1,0);scrollGraphic.raycastTarget=true;
        var scroll=scrollGo.GetComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=48;
        var viewport=Graphic(scrollGo.transform,"Viewport",Vector2.zero,new Vector2(2840,1010),new Color(1,1,1,0));viewport.raycastTarget=false;var vp=viewport.rectTransform;vp.anchorMin=Vector2.zero;vp.anchorMax=Vector2.one;vp.pivot=new Vector2(.5f,.5f);vp.anchoredPosition=Vector2.zero;vp.sizeDelta=Vector2.zero;viewport.gameObject.AddComponent<Mask>().showMaskGraphic=false;
        var content=Rect(viewport.transform,"Content",Vector2.zero,new Vector2(0,0));var cr=(RectTransform)content;cr.anchorMin=new Vector2(0,1);cr.anchorMax=new Vector2(1,1);cr.pivot=new Vector2(.5f,1);cr.anchoredPosition=Vector2.zero;cr.sizeDelta=new Vector2(0,0);
        var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=12;layout.padding=new RectOffset(32,32,24,28);layout.childControlWidth=true;layout.childControlHeight=false;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
        var fitter=content.gameObject.AddComponent<ContentSizeFitter>();fitter.horizontalFit=ContentSizeFitter.FitMode.Unconstrained;fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport=vp;scroll.content=cr;scrollGo.SetActive(false);

        string[] categories={"괴담 게시판","괴담 게시판","블로그","도시전설 아카이브","괴담 게시판","괴담 위키","제보 게시판"};
        string[] titles={"블러디 메리 괴담, 정말일까?","블러디 메리 강령술 하는 방법","블러디 메리 (미국괴담)","거울 속 이름을 부르는 의식","블러디 메리 경험담","거울 괴담의 지역별 전승","친구가 강령회 뒤 달라졌어요"};
        string[] summaries={"거울 앞에서 이름을 부른다는 유명한 도시 괴담","옛날에 유행했던 강령 의식과 그 유래","메리를 소환해 나타나게 되면 자신을 부른 사람을 죽인다고 합니다.","촛불과 거울을 사용하는 강령회에 대한 기록","의식을 한 뒤 이상한 일을 겪었다는 이야기","블러디 메리 이야기의 다른 형태를 정리했습니다.","강령회 뒤 친구가 달라졌다는 익명 제보"};
        string[] articles={
            "블러디 메리는 거울 앞에서 이름을 여러 차례 부르면 나타난다는 도시 괴담이다. 지역과 전승에 따라 부르는 횟수와 의식의 방식은 다르게 전해진다.\n\n이 이야기는 구전 괴담이며 확인된 사건 기록은 아니다.",
            "옛날에 유행한 블러디 메리 강령회는 어두운 방에서 거울을 바라보며 이름을 반복해 부르는 방식으로 알려져 있다.\n\n시율 일행은 실제로 이 의식을 따라 했다고 한다. 구체적인 절차를 재현하지 말고, 당시 접속한 사이트와 기록을 조사하자.",
            "블러디 메리는 미국에서 널리 알려진 거울 괴담이다. 전승에서는 거울에 이름을 부르면 형체가 나타나거나 자신을 부른 사람을 해친다고 이야기한다.\n\n출처: 괴담 블로그의 민속 이야기 정리.",
            "일부 기록은 거울을 매개로 한 소환담을 블러디 메리 전승과 연결한다. 이야기마다 세부는 다르며, 시대와 지역에 따라 내용이 바뀌었다.",
            "게시글 작성자는 강령회 뒤 친구가 달라졌다고 주장한다. 시율이 전한 사건과 비슷한 점이 있지만, 게시물만으로 원인을 단정할 수는 없다.",
            "거울 앞에서 이름을 부르는 이야기는 다양한 지역에서 변형되어 전해진다. 등장인물과 의식의 세부는 이야기마다 다르다.",
            "친구들과 거울 괴담을 따라 한 뒤 친구가 이상하게 변했다는 익명 글이다. 시율의 메시지와 비슷한 대목이 있다."
        };
        var entries=new List<ExploreProtoMessenger.SiteEntry>();
        for(int i=0;i<titles.Length;i++)
        {
            var row=Btn(content,"SiteResult_"+(i+1),"",Vector2.zero,new Vector2(2700,205),new Color(.99f,.96f,.91f,0));
            var le=row.gameObject.AddComponent<LayoutElement>();le.preferredHeight=205;le.minHeight=205;
            row.GetComponent<UnityEngine.UI.Image>().color=new Color(1,1,1,0);row.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            Label(row.transform,"Category",categories[i],new Vector2(-920,60),new Vector2(1700,48),28,new Color(.48f,.40f,.39f));
            Label(row.transform,"Title",titles[i],new Vector2(-920,0),new Vector2(2200,62),40,ink);
            Label(row.transform,"Summary",summaries[i],new Vector2(-920,-65),new Vector2(2200,52),30,ink);
            Graphic(row.transform,"Divider",new Vector2(0,-99),new Vector2(2600,2),new Color(.55f,.51f,.47f,.48f));
            entries.Add(new ExploreProtoMessenger.SiteEntry{category=categories[i],title=titles[i],summary=summaries[i],article=articles[i],button=row});
        }

        var detail=Graphic(browser,"SiteDetailPanel",new Vector2(0,-390),new Vector2(2840,1010),paper);
        Label(detail.transform,"SiteCategory","",new Vector2(-1050,400),new Vector2(1700,65),30,new Color(.48f,.40f,.39f));
        Label(detail.transform,"SiteTitle","",new Vector2(-850,310),new Vector2(2100,100),54,ink);
        var article=Label(detail.transform,"SiteArticle","",new Vector2(0,-120),new Vector2(2300,700),40,ink);article.alignment=TextAlignmentOptions.TopLeft;article.overflowMode=TextOverflowModes.Truncate;
        var back=Btn(detail.transform,"BackToResults","검색 결과로",new Vector2(0,-440),new Vector2(370,92),new Color(.85f,.81f,.75f),34);
        detail.gameObject.SetActive(false);

        ctrl.memoWindow=memo.gameObject;ctrl.internetWindow=browser.gameObject;ctrl.keywordPopup=popup.gameObject;
        ctrl.searchResults=scrollGo;ctrl.siteDetailPanel=detail.gameObject;
        ctrl.memoKeyword=keyword;ctrl.searchKeywordText=keywordValue;ctrl.searchHint=hint;ctrl.searchKeywordChip=keywordChip.gameObject;ctrl.searchKeywordPlaceholder=placeholder.gameObject;
        ctrl.siteCategoryText=detail.transform.Find("SiteCategory").GetComponent<TMP_Text>();ctrl.siteTitleText=detail.transform.Find("SiteTitle").GetComponent<TMP_Text>();ctrl.siteArticleText=article;
        ctrl.emojiButton=GameObject.Find("Button_Emoji").GetComponent<Button>();ctrl.internetButton=GameObject.Find("Button_Internet").GetComponent<Button>();
        ctrl.memoCloseButton=memoX;ctrl.internetCloseButton=browser.transform.Find("InternetCloseX").GetComponent<Button>();
        ctrl.addKeywordButton=add;ctrl.keywordFieldButton=keywordFieldButton;ctrl.popupCloseButton=popupClose;ctrl.popupKeywordButton=popupKeyword;
        ctrl.findButton=find;ctrl.clearKeywordButton=clear;ctrl.siteBackButton=back;ctrl.siteEntries=entries.ToArray();

        // Floating call UI can be repositioned and dismissed like the other app windows.
        var call=ctrl.callWindow.transform;var callHeader=Graphic(call,"CallTitleDragArea",new Vector2(0,820),new Vector2(2750,100),new Color(1,1,1,0),null,true).transform;
        AddDrag(call,callHeader);ctrl.callCloseButton=VisibleClose(call,"CallCloseX",new Vector2(1510,820),new Vector2(100,100),ctrl.CloseCall);
        var transcript=ctrl.transcriptWindow.transform;var transcriptHeader=Graphic(transcript,"TranscriptTitleDragArea",new Vector2(0,650),new Vector2(1850,100),new Color(1,1,1,0),null,true).transform;
        AddDrag(transcript,transcriptHeader);VisibleClose(transcript,"TranscriptCloseX",new Vector2(1030,620),new Vector2(100,100),ctrl.CloseTranscript);

        // Add a drag area to the keyword picker so its window treatment remains consistent.
        var popupDrag=Graphic(popup,"PopupDragArea",new Vector2(-25,170),new Vector2(580,72),new Color(1,1,1,0),null,true).transform;AddDrag(popup,popupDrag);
        EditorUtility.SetDirty(ctrl);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("Explore_Proto memo and internet apps saved.");
    }

    public static void SaveClosedDesktopState()
    {
        var scene=SceneManager.GetSceneByPath("Assets/Scenes/Explore_Proto.unity");
        var root=GameObject.Find("ExploreProtoCanvas").transform;
        foreach(var name in new[]{"MessengerWindow","VideoCallWindow","TranscriptWindow","MemoWindow","InternetWindow","KeywordPopup"})
            if(root.Find(name)) root.Find(name).gameObject.SetActive(false);
        foreach(var name in new[]{"SearchResults","SiteDetailPanel"})
            if(root.Find("InternetWindow/"+name)) root.Find("InternetWindow/"+name).gameObject.SetActive(false);
        var ctrl=Object.FindObjectOfType<ExploreProtoMessenger>();
        ctrl.unreadBadge.SetActive(true);ctrl.linkButtonObject.SetActive(false);ctrl.memoKeyword.gameObject.SetActive(false);
        ctrl.searchKeywordChip.SetActive(false);ctrl.searchKeywordText.gameObject.SetActive(false);ctrl.searchKeywordPlaceholder.SetActive(true);
        ctrl.clearKeywordButton.gameObject.SetActive(false);ctrl.findButton.interactable=false;
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
}
