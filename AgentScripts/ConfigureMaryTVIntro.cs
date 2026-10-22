using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

// Scene/renderer authoring only; never starts the intro or renders a test frame.
public static class ConfigureMaryTVIntro
{
 public static string Apply()
 {
  var scene = SceneManager.GetActiveScene();
  if(EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity") throw new InvalidOperationException("Battle edit mode required.");
  var intro = scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<BattleMaryIntroController>(true)).Single();
  var phase = intro.phaseOne;
  var camera = Camera.main;
  var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>("Assets/Settings/Renderer2D.asset");
  var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Explore_Proto/BattleInterference.shader");
  if(!camera || !rendererData || !shader || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Camera, renderer or shader missing/invalid.");
  var bell = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SoundEffect/universfield-church-bell-toll-156464.mp3");
  var click = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/SoundEffect/light-switch-on.mp3");
  var normal = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/ProtoType/card_merry_02.png").OfType<Sprite>().Single();
  var knife = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/ProtoType/card_merry_02_knife.png").OfType<Sprite>().Single();
  if(!bell || !click) throw new InvalidOperationException("Audio missing.");
  Undo.RegisterFullObjectHierarchyUndo(intro.blackoutRoot.transform.root.gameObject,"Update Mary intro");
  Undo.RecordObject(intro,"Update Mary sequence parameters");
  var state = camera.GetComponent<BattleScreenInterference>();
  if(!state) state = Undo.AddComponent<BattleScreenInterference>(camera.gameObject);
  intro.screenInterference = state;
  // Overlay canvases bypass camera post-processing. Route these saved canvases through the same camera.
  foreach(var canvas in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Canvas>(true)).Where(c=>c.isRootCanvas))
  {
   Undo.RecordObject(canvas,"Include whole UI in camera post-processing");
   canvas.renderMode = RenderMode.ScreenSpaceCamera;
   canvas.worldCamera = camera;
   canvas.planeDistance = 10f;
  }
  var feature = rendererData.rendererFeatures.OfType<BattleInterferenceRendererFeature>().SingleOrDefault();
  if(!feature)
  {
   feature = ScriptableObject.CreateInstance<BattleInterferenceRendererFeature>();
   feature.name = "Battle full-screen interference";
   AssetDatabase.AddObjectToAsset(feature,rendererData);
   rendererData.rendererFeatures.Add(feature);
  }
  feature.shader = shader;
  var rendererSerialized = new SerializedObject(rendererData);
  var featureMap = rendererSerialized.FindProperty("m_RendererFeatureMap");
  featureMap.arraySize = rendererData.rendererFeatures.Count;
  for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
  {
   AssetDatabase.TryGetGUIDAndLocalFileIdentifier(rendererData.rendererFeatures[i], out string guid, out long localId);
   featureMap.GetArrayElementAtIndex(i).longValue = localId;
  }
  rendererSerialized.ApplyModifiedPropertiesWithoutUndo();
  feature.SetActive(true); feature.Create();
  EditorUtility.SetDirty(feature); rendererData.SetDirty(); EditorUtility.SetDirty(rendererData);
  intro.bellClip = bell; intro.appearanceClip = click;
  intro.maryArtwork = intro.maryCard.transform.Find("Artwork").GetComponent<UnityEngine.UI.Image>();
  intro.glitchPulseSeconds = .18f; intro.glitchStartGapSeconds = 1f;
  intro.crtFadeSeconds = 2f; intro.blackoutSeconds = 1f;
  intro.knifeFadeSeconds = 2f;
  intro.secondFaceDelay = 1.5f; intro.thirdFaceDelay = 1f; intro.remainingFacesDelay = 1f; intro.remainingFaceGap = .2f;
  var knifeRect = (RectTransform)intro.knifeRoot.transform;
  var knifePosition = knifeRect.anchoredPosition;
  Undo.SetTransformParent(knifeRect,phase.cardArea,"Place knife above incoming frames");
  knifeRect.anchoredPosition = knifePosition;
  knifeRect.SetAsLastSibling();
  intro.knifeFadeGroup = knifeRect.GetComponent<CanvasGroup>();
  if(!intro.knifeFadeGroup) intro.knifeFadeGroup = Undo.AddComponent<CanvasGroup>(knifeRect.gameObject);
  intro.knifeFadeGroup.alpha = 1;
  intro.knifeFadeGroup.interactable = false; intro.knifeFadeGroup.blocksRaycasts = false;
  intro.knifeRoot.SetActive(false);
  intro.introRoot.transform.SetAsFirstSibling();
  var cards = phase.stages[0].cards;
  // These opaque frames exactly cover each initial portrait before it is hidden.
  Cover(cards[0],(RectTransform)intro.maryCard.transform);
  Cover(cards[1],(RectTransform)intro.heroineCard.transform);
  intro.knifeCardIndex = 2;
  var target = (RectTransform)cards[intro.knifeCardIndex].transform;
  // Knife artwork points left at approximately (1.3%,52.7%) within its Image rect.
  float finalKnifeX = Mathf.Min(knifePosition.x, phase.cardArea.rect.xMax - knifeRect.rect.width * knifeRect.localScale.x * .5f - 50f);
  float tipX = finalKnifeX + (0.013f - knifeRect.pivot.x) * knifeRect.rect.width * knifeRect.localScale.x;
  target.anchoredPosition = new Vector2(tipX - target.rect.width*(1-target.pivot.x) + 12f, target.anchoredPosition.y);
  for(int n=0;n<cards.Length;n++)
  {
   cards[n].isKnife = n==intro.knifeCardIndex;
   cards[n].artwork.sprite = cards[n].isKnife ? knife : normal;
   PrefabUtility.RecordPrefabInstancePropertyModifications(cards[n]);
   PrefabUtility.RecordPrefabInstancePropertyModifications(cards[n].artwork);
   PrefabUtility.RecordPrefabInstancePropertyModifications(cards[n].transform);
  }
  target.SetAsLastSibling();
  EditorUtility.SetDirty(intro);
  EditorSceneManager.MarkSceneDirty(scene);
  AssetDatabase.SaveAssets();
  if(!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
  return "Saved camera post-process for both canvases; bell/click assigned; knife target="+target.name+" pos="+target.anchoredPosition+" tipX="+tipX+"; one knife; later waves untouched. Shader errors="+ShaderUtil.ShaderHasError(shader);
 }
 static void Cover(BloodyMaryCallCard card, RectTransform portrait)
 {
  var r=(RectTransform)card.transform;
  r.anchorMin=portrait.anchorMin; r.anchorMax=portrait.anchorMax; r.pivot=portrait.pivot;
  r.sizeDelta=portrait.sizeDelta; r.anchoredPosition=portrait.anchoredPosition; r.localScale=portrait.localScale;
 }
}
