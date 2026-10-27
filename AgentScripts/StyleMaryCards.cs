using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class StyleMaryCards
{
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit mode required");
        var s=SceneManager.GetSceneByPath("Assets/Scenes/Battle_Proto.unity");
        var all=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        var window=all.Single(t=>t.name=="BattleCallWindow");
        var cards=window.GetComponentsInChildren<BloodyMaryCallCard>(true);
        var template=cards.Single(c=>c.name=="Card03"&&c.transform.parent.name=="Part1");
        var tr=(RectTransform)template.transform;
        float ratio=tr.rect.width/tr.rect.height;
        var frame=template.GetComponent<UnityEngine.UI.Image>();
        var frameSprite=frame.sprite;
        var frameColor=frame.color;
        var color=window.Find("CallArea").GetComponent<UnityEngine.UI.Image>().color;
        color.a=1;
        var art=template.artwork.rectTransform;
        Vector2 amin=art.anchorMin+new Vector2(art.offsetMin.x/tr.rect.width,art.offsetMin.y/tr.rect.height);
        Vector2 amax=art.anchorMax+new Vector2(art.offsetMax.x/tr.rect.width,art.offsetMax.y/tr.rect.height);
        if(cards.Any(c=>c.transform.Find("CardFrame")))throw new InvalidOperationException("Already styled; edit existing UI");
        Undo.IncrementCurrentGroup(); int group=Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Landscape opaque Mary cards");
        try
        {
            foreach(var card in cards)
            {
                var r=(RectTransform)card.transform;
                Undo.RecordObject(r,"Landscape card");
                float width=Mathf.Sqrt(r.rect.width*r.rect.height*ratio);
                r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);
                r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,width/ratio);
                var image=card.GetComponent<UnityEngine.UI.Image>();
                Undo.RecordObject(image,"Opaque card background");
                image.sprite=null; image.overrideSprite=null; image.color=color;
                image.type=UnityEngine.UI.Image.Type.Simple; image.material=null;
                image.preserveAspect=false; image.raycastTarget=true;
                var go=new GameObject("CardFrame",typeof(RectTransform),typeof(UnityEngine.UI.Image));
                Undo.RegisterCreatedObjectUndo(go,"Add card frame"); go.layer=card.gameObject.layer;
                var fr=(RectTransform)go.transform; fr.SetParent(r,false); fr.SetAsFirstSibling();
                fr.anchorMin=Vector2.zero; fr.anchorMax=Vector2.one; fr.offsetMin=fr.offsetMax=Vector2.zero;
                var fi=go.GetComponent<UnityEngine.UI.Image>(); fi.sprite=frameSprite; fi.color=frameColor; fi.raycastTarget=false;
                var ar=card.artwork.rectTransform;
                Undo.RecordObject(ar,"Match Card03 artwork inset");
                ar.anchorMin=amin; ar.anchorMax=amax; ar.offsetMin=ar.offsetMax=Vector2.zero;
                Undo.RecordObject(card.artwork,"Keep portrait proportions"); card.artwork.preserveAspect=true;
                foreach(var obj in new UnityEngine.Object[]{r,image,ar,card.artwork})
                { EditorUtility.SetDirty(obj); PrefabUtility.RecordPrefabInstancePropertyModifications(obj); }
            }
            EditorSceneManager.MarkSceneDirty(s);
            if(!EditorSceneManager.SaveScene(s))throw new InvalidOperationException("Save failed");
            Undo.CollapseUndoOperations(group);
            return "Saved "+cards.Length+" opaque landscape cards; ratio="+ratio+", background="+color+". Existing positions, sprites, and gameplay references retained.";
        }
        catch{Undo.RevertAllDownToGroup(group);throw;}
    }
}
