using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace CyberExorcist.Editor
{
    public static class PrototypeSetup
    {
        public const string ScenePath = "Assets/Scenes/CyberExorcistPrototype.unity";

        [MenuItem("Tools/Cyber Exorcist/Setup Single Scene Prototype")]
        public static void Setup()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play mode before setup.");
            var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/CyberExorcist/Fonts/Malgun.ttf");
            if (!source) throw new System.InvalidOperationException("Korean source font has not imported yet.");
            const string fontPath = "Assets/CyberExorcist/Fonts/PrototypeKorean.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (!font)
            {
                font = TMP_FontAsset.CreateFontAsset(source, 42, 5, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
                font.name = "PrototypeKorean";
                AssetDatabase.CreateAsset(font, fontPath);
                AssetDatabase.AddObjectToAsset(font.material, font);
                foreach (var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture, font);
                var chars = File.ReadAllText("Assets/CyberExorcist/CyberExorcistPrototype.cs");
                font.TryAddCharacters(string.Concat(chars.Distinct()), out string missing);
                EditorUtility.SetDirty(font);
            }
            var prototype = Object.FindFirstObjectByType<CyberExorcistPrototype>();
            if (!prototype)
            {
                var root = new GameObject("CyberExorcistPrototype");
                Undo.RegisterCreatedObjectUndo(root, "Create cyber exorcist prototype");
                prototype = root.AddComponent<CyberExorcistPrototype>();
            }
            prototype.KoreanFont = font;
            prototype.CameraArtwork = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CyberExorcist/Art/Reference1.png");
            prototype.GhostReference = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CyberExorcist/Art/Reference0.png");
            EditorUtility.SetDirty(prototype);
            var camera = Camera.main;
            if (camera) { camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.72f,.79f,.94f); }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == ScenePath)) scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = prototype.gameObject;
            Debug.Log("Cyber Exorcist prototype ready: " + ScenePath);
        }
    }
}
