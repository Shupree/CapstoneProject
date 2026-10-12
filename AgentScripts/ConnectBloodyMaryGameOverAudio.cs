using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time edit-mode connection. No audio playback or gameplay/preview methods are invoked.
public static class ConnectBloodyMaryGameOverAudio
{
    static AudioSource Source(string name, Transform parent, AudioClip clip)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Connect game over audio");
        go.transform.SetParent(parent, false);
        var audio = go.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.playOnAwake = false;
        audio.loop = false;
        audio.spatialBlend = 0f;
        audio.dopplerLevel = 0f;
        audio.pitch = 1f;
        audio.volume = 1f;
        return audio;
    }

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || Application.dataPath != "D:/CWS/Zol_Project/Assets" ||
            scene.path != "Assets/Scenes/Battle_Proto.unity") throw new InvalidOperationException("Target edit mode required.");
        var over = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<BloodyMaryGameOverController>(true)).Single();
        var hands = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SoundEffect/GameOver_Hands.wav");
        var signal = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SoundEffect/No_signal.mp3");
        if (!hands || !signal || over.handsAudio || over.signalLostAudio || over.transform.Find("GameOverAudioSources"))
            throw new InvalidOperationException("Missing audio or connection already exists.");
        string dialogue = JsonUtility.ToJson(over.GetComponent<BattleProtoConversation>());
        var existingAudio = over.GetComponents<AudioSource>();
        string[] audioSettings = existingAudio.Select(x => EditorJsonUtility.ToJson(x)).ToArray();
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Connect game over audio");
        try
        {
            Undo.RecordObject(over, "Connect game over clips");
            var root = new GameObject("GameOverAudioSources");
            Undo.RegisterCreatedObjectUndo(root, "Connect game over audio");
            root.transform.SetParent(over.transform, false);
            over.handsAudio = Source("HandsAudio", root.transform, hands);
            over.signalLostAudio = Source("SignalLostAudio", root.transform, signal);
            over.signalFadeSeconds = 1f;
            if (dialogue != JsonUtility.ToJson(over.GetComponent<BattleProtoConversation>()) ||
                !audioSettings.SequenceEqual(existingAudio.Select(x => EditorJsonUtility.ToJson(x))))
                throw new InvalidOperationException("Existing dialogue/audio settings changed.");
            EditorUtility.SetDirty(over);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved dedicated 2D non-looping HandsAudio and SignalLostAudio sources, exact requested clips and 1-second ending fade. Existing dialogue/audio settings preserved. No playback/gameplay invoked.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }
}
