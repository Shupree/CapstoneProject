using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time edit-mode authoring. Does not invoke any gameplay or preview functions.
public static class AuthorBloodyMaryPhaseTwo
{
    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Author phase two UI");
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static BloodyMaryWordSlot Word(string name, Transform parent, TMPro.TextMeshProUGUI source,
        BloodyMaryPhaseTwoController phase, BloodyMaryPhraseSlot phrase, int index, bool selected)
    {
        var rect = Rect(name, parent);
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = Vector2.zero;
        var text = rect.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
        EditorUtility.CopySerialized(source, text);
        text.text = string.Empty;
        text.richText = false;
        text.enableAutoSizing = false;
        text.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        text.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = true;
        if (!selected) text.color = new Color(.85f, .035f, .065f, 1f);
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = text;
        button.transition = UnityEngine.UI.Selectable.Transition.None;
        button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
        var word = rect.gameObject.AddComponent<BloodyMaryWordSlot>();
        word.label = text;
        word.button = button;
        word.phase = phase;
        word.phrase = phrase;
        word.wordIndex = index;
        word.selectedWord = selected;
        return word;
    }

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Battle_Proto edit mode required.");
        var one = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BloodyMaryPhaseController>(true)).Single();
        var window = one.clearedInternetWindow;
        var input = window.GetComponentInChildren<TMPro.TMP_InputField>(true);
        if (!input || one.GetComponent<BloodyMaryPhaseTwoController>() || window.Find("PhaseTwoPhraseArea"))
            throw new InvalidOperationException("Missing input or phase two already authored.");
        string dialogue = JsonUtility.ToJson(one.GetComponent<BattleProtoConversation>());
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Author Bloody Mary phase two");
        try
        {
            Undo.RecordObject(one, "Connect phase two");
            Undo.RecordObject(input, "Lock input frame");
            Undo.RecordObject(input.textComponent, "Hide old input text");
            var two = Undo.AddComponent<BloodyMaryPhaseTwoController>(one.gameObject);
            two.phaseOne = one;
            two.window = window;
            two.inputFrame = input;
            one.phaseTwo = two;
            input.readOnly = true;
            input.enabled = false;
            input.textComponent.text = string.Empty;
            input.textComponent.enabled = false;
            var source = (TMPro.TextMeshProUGUI)input.textComponent;

            var area = Rect("PhaseTwoPhraseArea", window);
            area.anchorMin = new Vector2(.06f, .30f);
            area.anchorMax = new Vector2(.94f, .85f);
            area.offsetMin = area.offsetMax = Vector2.zero;
            area.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            two.phraseArea = area;
            two.slots = new BloodyMaryPhraseSlot[4];
            for (int i = 0; i < 4; i++)
            {
                var rect = Rect("PhraseSlot_" + (i + 1).ToString("00"), area);
                rect.anchorMin = rect.anchorMax = Vector2.one * .5f;
                rect.pivot = Vector2.zero;
                var slot = rect.gameObject.AddComponent<BloodyMaryPhraseSlot>();
                slot.fade = rect.gameObject.AddComponent<CanvasGroup>();
                slot.fade.alpha = 0f;
                slot.fade.blocksRaycasts = false;
                slot.words = new BloodyMaryWordSlot[4];
                for (int j = 0; j < 4; j++) slot.words[j] = Word("Word_" + (j + 1).ToString("00"), rect, source, two, slot, j, false);
                rect.gameObject.SetActive(false);
                two.slots[i] = slot;
            }

            two.selectedSlots = new BloodyMaryWordSlot[4];
            for (int i = 0; i < 4; i++)
            {
                var word = Word("SelectedWord_" + (i + 1).ToString("00"), input.textViewport, source, two, null, i, true);
                var rect = (RectTransform)word.transform;
                rect.anchorMin = new Vector2(i * .25f, 0f);
                rect.anchorMax = new Vector2((i + 1) * .25f, 1f);
                rect.offsetMin = new Vector2(8f, 0f);
                rect.offsetMax = new Vector2(-8f, 0f);
                two.selectedSlots[i] = word;
            }

            var send = Rect("PhaseTwoSubmitButton", window);
            var inputRect = (RectTransform)input.transform;
            send.anchorMin = new Vector2(.85f, inputRect.anchorMin.y);
            send.anchorMax = new Vector2(.95f, inputRect.anchorMax.y);
            send.offsetMin = send.offsetMax = Vector2.zero;
            var background = send.gameObject.AddComponent<UnityEngine.UI.Image>();
            background.color = new Color(.13f, .15f, .20f, 1f);
            var submit = send.gameObject.AddComponent<BloodyMarySubmitButton>();
            submit.targetGraphic = background;
            submit.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            submit.interactable = false;
            var labelRect = Rect("Label", send);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var label = labelRect.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            EditorUtility.CopySerialized(source, label);
            label.enabled = true;
            label.text = "전송";
            label.color = Color.white;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.raycastTarget = false;
            two.submitButton = submit;
            UnityEventTools.AddPersistentListener(submit.onClick, two.Submit);
            UnityEventTools.AddPersistentListener(one.onPhaseCompleted, two.BeginPhase);
            if (dialogue != JsonUtility.ToJson(one.GetComponent<BattleProtoConversation>()))
                throw new InvalidOperationException("Conversation was modified.");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved phase two controller, 4 x 4 phrase words, 4 selected words and pointer-only submit; conversation preserved. No gameplay invoked.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }
}
