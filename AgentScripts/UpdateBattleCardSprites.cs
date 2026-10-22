using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Edits saved card artwork only. Does not run the battle or alter call layout.
public static class UpdateBattleCardSprites
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Open Battle_Proto in edit mode.");
        var phase = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BloodyMaryPhaseController>(true)).Single();
        var normal = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/ProtoType/card_merry_02.png").OfType<Sprite>().Single();
        var knife = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/ProtoType/card_merry_02_knife.png").OfType<Sprite>().Single();
        int normalCount = 0, knifeCount = 0;
        foreach (var stage in phase.stages)
            foreach (var card in stage.cards)
            {
                Undo.RecordObject(card.artwork, "Update battle card sprite");
                card.artwork.sprite = card.isKnife ? knife : normal;
                PrefabUtility.RecordPrefabInstancePropertyModifications(card.artwork);
                if (card.isKnife) knifeCount++; else normalCount++;
            }
        Undo.RecordObject(phase.retrySequence.backgroundMusic, "Disable automatic BGM playback");
        phase.retrySequence.backgroundMusic.playOnAwake = false;
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Saved normal=" + normalCount + ", knife=" + knifeCount + "; BGM playOnAwake=false. Call layout unchanged.";
    }
}
