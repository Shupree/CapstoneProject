using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class RestyleExploreCallCards
{
    static void Bounds(RectTransform r, Vector2 min, Vector2 max)
    {
        Undo.RecordObject(r, "Lay out call cards");
        r.anchorMin = min; r.anchorMax = max; r.pivot = new Vector2(.5f,.5f);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity")
            throw new InvalidOperationException("Explore edit mode required");
        var m = scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
        var p = m.callPortraits;
        var root = m.embeddedCallPanel.transform;
        var frame = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/UI/Window/CallChat_Card.png").OfType<Sprite>().Single(s=>s.name=="CallChat_Card_0");
        var icon = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/UI/Icon/Uncall_Icon.png").OfType<Sprite>().Single(s=>s.name=="Uncall_Icon_0");
        Undo.IncrementCurrentGroup(); int group=Undo.GetCurrentGroup();
        try
        {
            Undo.RecordObject(p,"Connect speaker frame tint");
            for(int n=0;n<2;n++)
            {
                var portrait = n==0?p.heroineImage:p.clientImage;
                var pane = (RectTransform)portrait.transform.parent;
                Bounds(pane,new Vector2(n==0?.052f:.503f,.24f),new Vector2(n==0?.475f:.926f,.835f));
                var image = pane.GetComponent<UnityEngine.UI.Image>();
                if(!image) image=Undo.AddComponent<UnityEngine.UI.Image>(pane.gameObject);
                Undo.RecordObject(image,"Replace call card frame");
                image.enabled=true;
                image.sprite=frame; image.type=UnityEngine.UI.Image.Type.Simple;
                image.color=Color.black; image.preserveAspect=false; image.raycastTarget=false;
                if(n==0)p.heroineFrame=image; else p.clientFrame=image;
                Bounds(portrait.rectTransform,new Vector2(.12f,.008f),new Vector2(.88f,.855f));
                Undo.RecordObject(portrait,"Fit portrait inside frame"); portrait.preserveAspect=true;
                var name = pane.Find(n==0?"PlayerName":"SiyulName") as RectTransform;
                if(name) Bounds(name,new Vector2(.012f,.012f),new Vector2(.94f,.075f));
            }
            p.idleFrameTint=Color.black; p.speakingFrameTint=Color.white;
            if(p.heroineHighlight)Undo.DestroyObjectImmediate(p.heroineHighlight);
            if(p.clientHighlight)Undo.DestroyObjectImmediate(p.clientHighlight);
            p.heroineHighlight=null; p.clientHighlight=null;
            var label = (RectTransform)m.connectingLabel.transform;
            Bounds(label,new Vector2(.503f,.24f),new Vector2(.926f,.835f));
            if(root.Find("CallEndIcon"))throw new InvalidOperationException("CallEndIcon already exists");
            var go=new GameObject("CallEndIcon",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            Undo.RegisterCreatedObjectUndo(go,"Add call end icon"); go.layer=root.gameObject.layer;
            var r=(RectTransform)go.transform; r.SetParent(root,false);
            r.anchorMin=r.anchorMax=new Vector2(.49f,.112f); r.pivot=new Vector2(.5f,.5f);
            // Match the reference's 70px icon against its 1057px-wide window.
            float size=((RectTransform)root).rect.width*(70f/1057f);
            r.sizeDelta=new Vector2(size,size); r.anchoredPosition=Vector2.zero;
            var ui=go.GetComponent<UnityEngine.UI.Image>(); ui.sprite=icon; ui.preserveAspect=true; ui.raycastTarget=false;
            EditorUtility.SetDirty(p);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Undo.CollapseUndoOperations(group);
            return "Saved two tinted call frames, portrait/name layout, centered connection label and call end icon.";
        }
        catch {Undo.RevertAllDownToGroup(group);throw;}
    }
}
