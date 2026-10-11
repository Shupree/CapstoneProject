using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildExploreProtoMessenger
{
    static Transform Make(Transform parent,string name,Vector2 pos,Vector2 size,Color color,Sprite sprite=null,bool raycast=false)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)); go.transform.SetParent(parent,false);
        var rt=(RectTransform)go.transform; rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f); rt.pivot=new Vector2(.5f,.5f); rt.anchoredPosition=pos; rt.sizeDelta=size;
        var im=go.GetComponent<Image>(); im.color=color; im.sprite=sprite; im.preserveAspect=sprite!=null; im.raycastTarget=raycast; return go.transform;
    }
    static TMP_Text Text(Transform p,string n,string value,Vector2 pos,Vector2 size,int font,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
    {
        var g=new GameObject(n,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI)); g.transform.SetParent(p,false);
        var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;
        var t=g.GetComponent<TextMeshProUGUI>(); t.text=value;t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/CyberExorcist/Fonts/PrototypeKorean.asset");t.fontSize=font;t.color=color;t.alignment=align;t.enableWordWrapping=true;t.raycastTarget=false; return t;
    }
    static Button Button(Transform p,string n,string label,Vector2 pos,Vector2 size,Color bg,Color fg,int font)
    {
        var tr=Make(p,n,pos,size,bg,null,true);var im=tr.GetComponent<Image>(); var b=tr.gameObject.AddComponent<Button>();b.targetGraphic=im;
        Text(tr,"Label",label,Vector2.zero,size- new Vector2(24,12),font,fg,TextAlignmentOptions.Center);return b;
    }
    static Transform Image(Transform p,string n,Sprite sp,Vector2 pos,Vector2 size,Color tint)
    {var tr=Make(p,n,pos,size,tint,sp,false);return tr;}
    [MenuItem("Explore Proto/Build Messenger UI")]
    public static void Build()
    {
        var sc=SceneManager.GetSceneByPath("Assets/Scenes/Explore_Proto.unity"); if(!sc.IsValid()) sc=SceneManager.GetActiveScene();
        if(sc.path!="Assets/Scenes/Explore_Proto.unity") throw new System.Exception("Explore_Proto scene must be open.");
        var canvas=GameObject.Find("ExploreProtoCanvas"); if(!canvas) throw new System.Exception("ExploreProtoCanvas not found");
        var root=canvas.transform;
        if(root.Find("MessengerWindow")) throw new System.Exception("Messenger UI already exists. Edit the saved scene instead of rebuilding it.");
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/CyberExorcist/Fonts/PrototypeKorean.asset");
        if(!font) throw new System.Exception("PrototypeKorean font missing");
        var player=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/ProtoType/MainCharacter_01.png");
        var caller=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/ProtoType/ExtraCharacter.png");
        var ctrlGo=new GameObject("ExploreProtoMessenger"); var ctrl=ctrlGo.AddComponent<ExploreProtoMessenger>();
        var badge=Make(root,"MessengerUnreadBadge",new Vector2(255, -137),new Vector2(84,84),new Color(.95f,.23f,.12f),null,false);
        Text(badge,"UnreadCount","2",Vector2.zero,new Vector2(80,78),48,Color.white,TextAlignmentOptions.Center);
        var mb=GameObject.Find("Button_Messenger").GetComponent<Button>(); badge.SetParent(mb.transform,false);
        ((RectTransform)badge).anchorMin=((RectTransform)badge).anchorMax=new Vector2(1,1);((RectTransform)badge).pivot=new Vector2(.5f,.5f);((RectTransform)badge).anchoredPosition=new Vector2(8,-8);

        var msg=Make(root,"MessengerWindow",Vector2.zero,new Vector2(2680,1500),new Color(.035f,.045f,.065f,.95f),null,false);msg.gameObject.SetActive(false);
        var bar=Make(msg,"TitleBar",new Vector2(0,690),new Vector2(2680,120),new Color(.92f,.40f,.58f),null,false);
        Text(bar,"AppTitle","",new Vector2(-930,0),new Vector2(760,90),54,Color.white);
        var close=Button(bar,"CloseMessenger","×",new Vector2(1240,0),new Vector2(100,90),new Color(.78f,.22f,.37f),Color.white,64);
        var left=Make(msg,"ContactPane",new Vector2(-950,-50),new Vector2(780,1370),new Color(.92f,.94f,.95f),null,false);
        Text(left,"ContactHeader","대화  ·  안 읽은 메시지",new Vector2(0,605),new Vector2(720,90),38,new Color(.18f,.22f,.28f));
        var contact=Make(left,"SiyulContact",new Vector2(0,420),new Vector2(760,220),new Color(.82f,.88f,.9f),null,false);
        Image(contact,"SiyulAvatar",caller,new Vector2(-290,0),new Vector2(150,180),Color.white);
        Text(contact,"ContactName","시율",new Vector2(30,50),new Vector2(400,70),44,new Color(.13f,.17f,.22f));
        Text(contact,"ContactPreview","인터넷 퇴마사 맞으시죠?",new Vector2(55,-35),new Vector2(500,65),32,new Color(.27f,.31f,.36f));
        var chat=Make(msg,"ConversationPane",new Vector2(465,-50),new Vector2(1830,1370),new Color(.98f,.98f,.97f),null,false);
        Text(chat,"ConversationHeader","시율   ·   온라인",new Vector2(0,605),new Vector2(1720,90),42,new Color(.16f,.2f,.25f));
        Image(chat,"SiyulChatAvatar",caller,new Vector2(-790,495),new Vector2(80,90),Color.white);
        var first=Make(chat,"FirstIncomingBubble",new Vector2(-220,445),new Vector2(1160,240),new Color(.88f,.91f,.93f),null,false);
        Text(first,"FirstMessage","당신이 그 인터넷 퇴마사인가그ㄱ거맞죠? 아니 친구가 지금 뭔가에 씌엿는지 저희를 해치려드ㄹ고 있어요!!",Vector2.zero,new Vector2(1080,210),38,new Color(.12f,.15f,.19f));
        var second=Make(chat,"CallRequestBubble",new Vector2(-230,200),new Vector2(1140,150),new Color(.88f,.91f,.93f),null,false);
        Text(second,"CallRequest","의뢰를 맡아주신다면 화상 통화로 상황을 설명드리고 싶어요.",Vector2.zero,new Vector2(1070,125),36,new Color(.12f,.15f,.19f));
        var callBtn=Button(chat,"StartVideoCall","화상 통화 걸기",new Vector2(550,75),new Vector2(360,100),new Color(.34f,.58f,.82f),Color.white,36);
        var recording=Button(chat,"VideoCallRecording","화상 통화 녹음본",new Vector2(-510,-200),new Vector2(530,96),new Color(.28f,.57f,.82f),Color.white,34);recording.gameObject.SetActive(false);
        var linkText=Text(chat,"LinkMessage","그때 접속했던 사이트 주소도 보내드릴게요.",new Vector2(-245,-330),new Vector2(1110,100),36,new Color(.12f,.15f,.19f));linkText.gameObject.SetActive(false);
        var link=Button(chat,"InternetLink","인터넷 창으로 가는 링크",new Vector2(-480,-430),new Vector2(620,92),new Color(.88f,.91f,.93f),new Color(.12f,.4f,.72f),34);link.gameObject.SetActive(false);
        // Keep future link visible but deliberately inactive until the next prototype phase.
        // It is revealed together with the post-call message.
        linkText.gameObject.SetActive(false);link.gameObject.SetActive(false);

        var call=Make(root,"VideoCallWindow",Vector2.zero,new Vector2(3200,1800),new Color(.015f,.02f,.035f),null,false);call.gameObject.SetActive(false);
        Text(call,"CallHeader","●  화상 통화  ·  일반",new Vector2(-1120,800),new Vector2(850,100),42,Color.white);
        var pane1=Make(call,"PlayerVideoPane",new Vector2(-810,110),new Vector2(1440,760),new Color(.78f,.78f,.8f),null,false);
        Image(pane1,"PlayerSprite",player,new Vector2(0,-80),new Vector2(560,710),Color.white);
        Text(pane1,"PlayerName","나",new Vector2(-540,-320),new Vector2(220,70),38,Color.white);
        var pane2=Make(call,"SiyulVideoPane",new Vector2(810,110),new Vector2(1440,760),new Color(.28f,.31f,.39f),null,false);
        Image(pane2,"SiyulSprite",caller,new Vector2(0,-60),new Vector2(540,700),Color.white);
        Text(pane2,"SiyulName","시율",new Vector2(-540,-320),new Vector2(220,70),38,Color.white);
        var dialog=Make(call,"DialogueBox",new Vector2(0,-590),new Vector2(2700,300),new Color(.05f,.06f,.09f,.96f),null,false);
        var speaker=Text(dialog,"Speaker","",new Vector2(-1110,85),new Vector2(420,65),36,new Color(.5f,.83f,1));
        var line=Text(dialog,"DialogueText","화면을 클릭해 대화를 시작하세요",new Vector2(0,-20),new Vector2(2450,150),42,Color.white);
        var advance=call.gameObject.AddComponent<Button>();advance.targetGraphic=call.GetComponent<Image>();call.GetComponent<Image>().raycastTarget=true;
        var hint=Button(call,"EndCallHint","화면을 클릭하면 다음 대사",new Vector2(0,-830),new Vector2(850,90),new Color(.1f,.12f,.16f),new Color(.75f,.78f,.82f),28);
        hint.GetComponent<Image>().raycastTarget=false;hint.transform.GetChild(0).GetComponent<TMP_Text>().raycastTarget=false;

        var transcript=Make(root,"TranscriptWindow",Vector2.zero,new Vector2(2200,1420),new Color(.06f,.075f,.10f,.98f),null,false);transcript.gameObject.SetActive(false);
        Text(transcript,"TranscriptTitle","화상 통화 녹음본",new Vector2(0,600),new Vector2(1900,100),52,Color.white,TextAlignmentOptions.Center);
        var transcriptBody=Text(transcript,"TranscriptText","",new Vector2(0,0),new Vector2(1900,1050),34,new Color(.92f,.94f,.97f));transcriptBody.alignment=TextAlignmentOptions.TopLeft;transcriptBody.enableWordWrapping=true;transcriptBody.overflowMode=TextOverflowModes.Truncate;
        var transClose=Button(transcript,"CloseTranscript","닫기",new Vector2(0,-610),new Vector2(300,90),new Color(.28f,.4f,.55f),Color.white,34);
        ctrl.messengerWindow=msg.gameObject;ctrl.callWindow=call.gameObject;ctrl.transcriptWindow=transcript.gameObject;ctrl.unreadBadge=badge.gameObject;ctrl.linkButtonObject=link.gameObject;
        ctrl.unreadCount=badge.GetChild(0).GetComponent<TMP_Text>();ctrl.callSpeaker=speaker;ctrl.callLineText=line;ctrl.transcriptText=transcriptBody;
        ctrl.messengerButton=mb;ctrl.callAdvanceButton=advance;ctrl.callButton=callBtn;ctrl.recordingButton=recording;ctrl.transcriptCloseButton=transClose;ctrl.messengerCloseButton=close;ctrl.linkMessage=linkText;
        ctrl.callLines=new ExploreProtoMessenger.CallLine[]{
          new(){speaker="시율",text="안녕하세요. 갑자기 연락드려서 죄송해요. 인터넷 퇴마사님 맞으시죠?"},
          new(){speaker="주인공",text="네, 맞아요! 첫 의뢰라 조금 긴장되지만 제가 도와드릴게요."},
          new(){speaker="주인공 속마음",text="(괜찮아, 밝게 말하자. 그런데 귀신은커녕 괴이를 본 적도 없는데… 정말 할 수 있을까?)"},
          new(){speaker="시율",text="친구들이랑 심심풀이로 블러디 메리 강령회를 했어요. 거울 앞에서 이름을 세 번 불렀거든요."},
          new(){speaker="시율",text="그날 이후 친구가 완전히 달라졌어요. 밤마다 혼잣말을 하고, 오늘은 저희를 해치려 들었어요."},
          new(){speaker="주인공",text="블러디 메리… 인터넷 괴담으로만 봤는데 실제로 그런 일이 생기다니."},
          new(){speaker="주인공 속마음",text="(무서워. 화면 너머인데도 손이 떨려. 그래도 지금 물러서면 안 돼.)"},
          new(){speaker="주인공",text="시율 씨, 혼자 해결하려 하지 말고 안전한 곳으로 대피해 주세요. 제가 원인을 찾아볼게요."},
          new(){speaker="시율",text="고마워요. 강령회 때 접속했던 인터넷 창 링크를 보낼게요. 그곳에서 뭔가 찾을 수 있을지도 몰라요."}
        };
        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);
        Debug.Log("Explore_Proto messenger UI built and saved.");
    }

    [MenuItem("Explore Proto/Use Local Prototype Font")]
    public static void UseLocalFont()
    {
        const string source="Assets/CyberExorcist/Fonts/PrototypeKorean.asset";
        const string local="Assets/Explore_Proto/Fonts/ExploreProtoKorean.asset";
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        if(!AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(local))
        {
            if(!AssetDatabase.CopyAsset(source,local)) throw new System.Exception("Could not copy prototype font into Explore_Proto.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(local);
        var canvas=GameObject.Find("ExploreProtoCanvas");
        if(!font||!canvas) throw new System.Exception("Local font or ExploreProtoCanvas was not found.");
        foreach(var text in canvas.GetComponentsInChildren<TMP_Text>(true)) text.font=font;
        var controller=Object.FindObjectOfType<ExploreProtoMessenger>();
        if(controller) controller.messengerButton=GameObject.Find("Button_Messenger").GetComponent<Button>();
        var scene=SceneManager.GetSceneByPath("Assets/Scenes/Explore_Proto.unity");
        if(controller) EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("Explore_Proto TMP labels now use their local font asset.");
    }
}
