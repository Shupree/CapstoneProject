using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class MatchBattleCallUI
{
    public static string Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit mode required");
        var explore = SceneManager.GetSceneByPath("Assets/Scenes/Explore_Proto.unity");
        var battle = SceneManager.GetSceneByPath("Assets/Scenes/Battle_Proto.unity");
        if (!explore.isLoaded) explore=EditorSceneManager.OpenScene("Assets/Scenes/Explore_Proto.unity",OpenSceneMode.Additive);
        if (!battle.isLoaded) battle=EditorSceneManager.OpenScene("Assets/Scenes/Battle_Proto.unity",OpenSceneMode.Additive);
        var source = explore.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RectTransform>(true)).Single(t => t.name == "EmbeddedVideoCall");
        var target = battle.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RectTransform>(true)).Single(t => t.name == "VideoCallWindow");
        var conversation = battle.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BattleProtoConversation>(true)).Single();
        string before = JsonUtility.ToJson(conversation);
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Match battle call UI to explore");
        try
        {
            CopyVisuals(source, target);
            foreach (string name in new[] { "CallTitleDragArea", "CallCloseX" })
            {
                var extra = target.Find(name);
                if (extra) { Undo.RecordObject(extra.gameObject, "Hide old call control"); extra.gameObject.SetActive(false); }
            }
            var indicator = target.Find("ConnectingLabel");
            if (conversation.callConnection && indicator)
            {
                Undo.RecordObject(conversation.callConnection, "Connect matching call indicator");
                conversation.callConnection.connectingIndicator = indicator.gameObject;
                EditorUtility.SetDirty(conversation.callConnection);
            }
            if (before != JsonUtility.ToJson(conversation)) throw new InvalidOperationException("Conversation data changed");
            EditorSceneManager.MarkSceneDirty(battle);
            if (!EditorSceneManager.SaveScene(battle)) throw new InvalidOperationException("Save failed");
            Undo.CollapseUndoOperations(group);
            return "Saved matching call UI in place; conversation data and button events retained.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static void CopyVisuals(RectTransform s, RectTransform d)
    {
        Undo.RecordObject(d, "Match call layout");
        d.anchorMin=s.anchorMin; d.anchorMax=s.anchorMax; d.pivot=s.pivot;
        d.sizeDelta=s.sizeDelta; d.anchoredPosition3D=s.anchoredPosition3D;
        d.localRotation=s.localRotation; d.localScale=s.localScale;
        var si=s.GetComponent<UnityEngine.UI.Image>(); var di=d.GetComponent<UnityEngine.UI.Image>();
        if(si && di)
        {
            Undo.RecordObject(di,"Match call image");
            di.sprite=si.sprite; di.overrideSprite=si.overrideSprite; di.color=si.color; di.material=si.material;
            di.type=si.type; di.preserveAspect=si.preserveAspect; di.fillCenter=si.fillCenter;
            di.pixelsPerUnitMultiplier=si.pixelsPerUnitMultiplier; di.raycastTarget=si.raycastTarget;
            di.maskable=si.maskable; di.enabled=si.enabled;
        }
        var st=s.GetComponent<TMPro.TextMeshProUGUI>(); var dt=d.GetComponent<TMPro.TextMeshProUGUI>();
        if(st && dt)
        {
            Undo.RecordObject(dt,"Match call label");
            dt.text=st.text; dt.font=st.font; dt.fontSharedMaterial=st.fontSharedMaterial;
            dt.fontSize=st.fontSize; dt.fontStyle=st.fontStyle; dt.color=st.color;
            dt.enableAutoSizing=st.enableAutoSizing; dt.fontSizeMin=st.fontSizeMin; dt.fontSizeMax=st.fontSizeMax;
            dt.alignment=st.alignment; dt.margin=st.margin; dt.textWrappingMode=st.textWrappingMode;
            dt.overflowMode=st.overflowMode; dt.characterSpacing=st.characterSpacing;
            dt.lineSpacing=st.lineSpacing; dt.raycastTarget=st.raycastTarget; dt.enabled=st.enabled;
        }
        var sb=s.GetComponent<UnityEngine.UI.Button>(); var db=d.GetComponent<UnityEngine.UI.Button>();
        if(sb && db) { Undo.RecordObject(db,"Match button style"); db.colors=sb.colors; db.transition=sb.transition; }
        // Lifecycle-controlled roots keep their battle state. Decorative child visibility follows the source.
        if(s.name != "EmbeddedVideoCall" && s.name != "ConnectedControls" && !s.name.EndsWith("SpeakingBorder"))
        { Undo.RecordObject(d.gameObject,"Match visible UI"); d.gameObject.SetActive(s.gameObject.activeSelf); }
        foreach (Transform child in s)
        {
            if(child.GetComponent<TMPro.TMP_SubMeshUI>()) continue;
            var match=d.Find(child.name) as RectTransform;
            if(!match)
            {
                if(child.name!="ConnectingLabel") throw new InvalidOperationException("Unexpected missing UI: "+child.name);
                var added=UnityEngine.Object.Instantiate(child.gameObject,d,false);
                Undo.RegisterCreatedObjectUndo(added,"Add connection label");
                added.name=child.name; match=(RectTransform)added.transform;
            }
            CopyVisuals((RectTransform)child,match);
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(d);
    }
}
