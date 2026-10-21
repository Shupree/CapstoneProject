using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>기존 Battle_Proto 통화창과 카드 프리팹에 2페이즈용 저장 UI를 한 번 추가한다.</summary>
public static class AuthorBattleCallPhaseTwo
{
    const string ScenePath = "Assets/Scenes/Battle_Proto.unity";
    const string CardPath = "Assets/Prefab/BloodyMaryCall/CallCard.prefab";
    const string FontPath = "Assets/Explore_Proto/Fonts/ExploreProtoKorean.asset";
    static readonly Color Red = new(1f, .07f, .1f, 1f);
    static TMP_FontAsset font;
    // 레퍼런스처럼 윗부분은 촘촘히 채우고 눈 주변은 특수기호로 남긴다.
    // 아랫부분은 입력창 양옆으로 내려오는 머리카락이다.
    static readonly float[] FigureY =
    {
        .4704f, .4032f, .336f, .2688f, .2016f, .1344f, .0672f, 0f,
        -.0672f, -.1344f, -.2016f, -.2688f, -.336f, -.4032f, -.448f, -.504f
    };
    static readonly float[][] FigureX =
    {
        new[] { -.20f, -.12f, 0f, .12f, .20f },
        new[] { -.30f, -.22f, -.14f, 0f, .14f, .22f, .30f },
        new[] { -.36f, -.29f, -.22f, -.15f, -.08f, 0f, .08f, .15f, .22f, .29f, .36f },
        new[] { -.39f, -.32f, -.25f, -.18f, -.11f, -.04f, .04f, .11f, .18f, .25f, .32f, .39f },
        new[] { -.40f, -.33f, -.26f, -.19f, -.12f, 0f, .12f, .19f, .26f, .33f, .40f },
        new[] { -.41f, -.34f, -.27f, -.20f, -.13f, .13f, .20f, .27f, .34f, .41f },
        new[] { -.41f, -.34f, -.27f, -.20f, -.13f, .13f, .20f, .27f, .34f, .41f },
        new[] { -.40f, -.33f, -.26f, -.19f, -.07f, .07f, .19f, .26f, .33f, .40f },
        new[] { -.39f, -.32f, -.25f, -.18f, -.04f, .04f, .18f, .25f, .32f, .39f },
        new[] { -.37f, -.30f, -.23f, -.06f, .06f, .23f, .30f, .37f },
        new[] { -.42f, -.36f, -.025f, .025f, .36f, .42f },
        new[] { -.42f, -.36f, .36f, .42f },
        new[] { -.48f, -.43f, .43f, .48f },
        new[] { -.48f, -.43f, .43f, .48f },
        new[] { -.48f, -.43f, .43f, .48f },
        new[] { -.48f, -.43f, .43f, .48f }
    };
    static readonly string[] FigureFragments =
    {
        "네가", "죽였어.", "왜 아무도", "구해주지", "않았어?", "울지마렴",
        "내 아가..", "가증스러운", "것들", "용서 못해", "더 많은", "생기가",
        "필요해", "너는", "죽어야 해", "돌아가게", "두지 않겠어", "그 아이의",
        "몸은", "내꺼야", "죽어"
    };
    static readonly string[] HairCrown =
    {
        "울지마렴", "왜 아무도", "돌아가게 두지 않겠어", "그 아이의 몸은", "가증스러운"
    };
    static readonly string[] HairForehead =
    {
        "네가 죽였어", "용서 못해", "더 많은 생기가", "왜 아무도 구해주지 않았어?",
        "그 아이의 몸은", "두지 않겠어", "네가 죽였어"
    };
    const int FigureSlotCount = 120;

    static string FigurePhrase(int row, int column)
    {
        float x = FigureX[row][column];
        if (row == 0) return HairCrown[column];
        if (row == 1) return HairForehead[column];
        if (row == 4 && Mathf.Abs(Mathf.Abs(x) - .12f) < .001f) return "◆◆◆";
        if (row == 5 && Mathf.Abs(Mathf.Abs(x) - .13f) < .001f) return "◇●◇";
        if (row == 6 && Mathf.Abs(Mathf.Abs(x) - .13f) < .001f) return "▲●▲";
        return FigureFragments[(row * 13 + column * 7) % FigureFragments.Length];
    }

    static float FigureSize(int row, int column, string phrase)
    {
        if (phrase == "◆◆◆" || phrase == "◇●◇" || phrase == "▲●▲") return 38f;
        if (row >= 10) return 33f + (column % 3) * 3f;
        return 33f + ((row + column * 3) % 5) * 3f;
    }

    public static string Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Play 모드에서 씬을 편집할 수 없습니다.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath) throw new InvalidOperationException("Battle_Proto 씬을 연 뒤 실행하세요.");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (!font) throw new InvalidOperationException("한국어 TMP 폰트를 찾지 못했습니다.");
        const string phraseGlyphs = "네가 죽였어.왜 아무도 구해주지 않았어?울지마렴 내 아가..가증스러운 것들용서 못해더 많은 생기가 필요해너는 죽어야 해돌아가게 두지 않겠어그 아이의 몸은 내꺼야네 아기는 죽지 않았어◆◇●▲";
        if (!font.TryAddCharacters(phraseGlyphs, out string missing) || !string.IsNullOrEmpty(missing))
            throw new InvalidOperationException("폰트에 추가하지 못한 글자: " + missing);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        AddCardThreat();
        var canvas = GameObject.Find("ExploreProtoCanvas");
        var host = GameObject.Find("ExploreProtoMessenger");
        if (!canvas || !host) throw new InvalidOperationException("씬의 Canvas 또는 controller가 없습니다.");
        var call = canvas.transform.Find("BattleCallWindow");
        var area = call ? call.Find("CallArea") : null;
        if (!area) throw new InvalidOperationException("BattleCallWindow/CallArea가 없습니다.");
        if (area.Find("PhaseTwoRoot")) throw new InvalidOperationException("PhaseTwoRoot가 이미 있습니다. 중복 생성하지 않았습니다.");
        var phaseOne = host.GetComponent<BloodyMaryPhaseController>();
        var phaseTwo = host.GetComponent<BloodyMaryPhaseTwoController>();
        var gameOver = host.GetComponent<BloodyMaryGameOverController>();
        if (!phaseOne || !phaseTwo || !gameOver) throw new InvalidOperationException("배틀 페이즈 컴포넌트가 없습니다.");

        var root = NewRect("PhaseTwoRoot", area);
        Stretch(root, 0f, 0f, 0f, 0f);
        var backdrop = root.gameObject.AddComponent<UnityEngine.UI.Image>();
        backdrop.color = new Color(.045f, .035f, .045f, 1f);
        backdrop.raycastTarget = false;

        var phraseArea = NewRect("PhraseArea", root);
        Stretch(phraseArea, 40f, 80f, 40f, 45f);
        phraseArea.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();

        var sources = new BloodyMarySyllableSource[FigureSlotCount];
        for (int i = 0; i < sources.Length; i++)
        {
            var text = NewText("SyllablePhrase_" + (i + 1).ToString("00"), phraseArea, 58f, Red);
            var rect = text.rectTransform;
            rect.sizeDelta = new Vector2(650f, 80f);
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = true;
            text.richText = true;
            var group = text.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            var source = text.gameObject.AddComponent<BloodyMarySyllableSource>();
            source.phase = phaseTwo;
            source.label = text;
            source.group = group;
            sources[i] = source;
            text.gameObject.SetActive(false);
        }

        var instruction = NewText("Instruction", root, 48f, Color.white);
        Top(instruction.rectTransform, 0f, -38f, 2700f, 80f);
        instruction.raycastTarget = false;
        var timer = NewText("RemainingSeconds", root, 66f, Red);
        Top(timer.rectTransform, 1510f, -36f, 180f, 90f);
        timer.raycastTarget = false;

        var input = NewRect("SyllableInput", root);
        input.anchorMin = input.anchorMax = new Vector2(.5f, 0f);
        input.pivot = new Vector2(.5f, 0f);
        input.anchoredPosition = new Vector2(0f, 150f);
        input.sizeDelta = new Vector2(2500f, 250f);
        var inputImage = input.gameObject.AddComponent<UnityEngine.UI.Image>();
        inputImage.color = new Color(.015f, .015f, .02f, 1f);
        inputImage.raycastTarget = false;
        var inputOutline = input.gameObject.AddComponent<UnityEngine.UI.Outline>();
        inputOutline.effectColor = new Color(.8f, .8f, .8f, 1f);
        inputOutline.effectDistance = new Vector2(3f, -3f);
        var selected = new BloodyMarySelectedSyllable[9];
        float x = -1100f;
        for (int i = 0; i < selected.Length; i++)
        {
            var text = NewText("SelectedSyllable_" + (i + 1).ToString("00"), input, 66f, Color.white);
            text.rectTransform.sizeDelta = new Vector2(80f, 110f);
            text.rectTransform.anchoredPosition = new Vector2(x, 0f);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = true;
            var button = text.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = text;
            button.transition = UnityEngine.UI.Selectable.Transition.None;
            var slot = text.gameObject.AddComponent<BloodyMarySelectedSyllable>();
            slot.phase = phaseTwo;
            slot.label = text;
            slot.button = button;
            slot.index = i;
            selected[i] = slot;
            x += 85f + (i == 0 || i == 3 || i == 5 ? 45f : 0f);
        }

        var completed = NewText("CompletedSentence", root, 72f, Color.white);
        completed.rectTransform.sizeDelta = new Vector2(2500f, 150f);
        completed.rectTransform.anchoredPosition = new Vector2(0f, 160f);
        completed.raycastTarget = false;
        completed.gameObject.SetActive(false);
        root.gameObject.SetActive(false);

        var warning = NewRect("PhaseTwoDeathWarning", area);
        Stretch(warning, 0f, 0f, 0f, 0f);
        var warningImage = warning.gameObject.AddComponent<UnityEngine.UI.Image>();
        warningImage.color = new Color(.02f, .01f, .02f, 1f);
        warningImage.raycastTarget = true;
        var warningText = NewText("WarningText", warning, 90f, Red);
        Stretch(warningText.rectTransform, 80f, 100f, 80f, 100f);
        warningText.text = "너는 죽어야 해";
        warningText.raycastTarget = false;
        warning.gameObject.SetActive(false);

        phaseOne.threatSeconds = 1f;
        phaseTwo.window = (RectTransform)call;
        phaseTwo.phaseRoot = root;
        phaseTwo.phraseArea = phraseArea;
        phaseTwo.timerText = timer;
        phaseTwo.instructionText = instruction;
        phaseTwo.completedText = completed;
        phaseTwo.sources = sources;
        phaseTwo.selectedSlots = selected;
        ConfigureFigureLayout(phaseTwo);
        phaseTwo.partOneSeconds = 20f;
        phaseTwo.partTwoBonusSeconds = 15f;
        phaseTwo.partTwoWaveSeconds = 4f;
        gameOver.phaseTwoWarning = warning.gameObject;
        EditorUtility.SetDirty(phaseOne);
        EditorUtility.SetDirty(phaseTwo);
        EditorUtility.SetDirty(gameOver);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Battle_Proto 통화창 2페이즈 UI와 카드 경고 화면 저장 완료";
    }

    // 이미 작성된 씬의 문구 슬롯을 보존하고 레퍼런스 배치에 필요한 슬롯만 추가한다.
    public static string ApplyFixedFigureLayout()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Play 모드에서 씬을 편집할 수 없습니다.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath || scene.isDirty) throw new InvalidOperationException("저장된 Battle_Proto 씬을 연 뒤 실행하세요.");
        var host = GameObject.Find("ExploreProtoMessenger");
        var phaseTwo = host ? host.GetComponent<BloodyMaryPhaseTwoController>() : null;
        if (!phaseTwo || !phaseTwo.phraseArea || phaseTwo.sources == null ||
            (phaseTwo.sources.Length != 26 && phaseTwo.sources.Length != 55 &&
             phaseTwo.sources.Length != FigureSlotCount))
            throw new InvalidOperationException("기존 2페이즈 문구 슬롯 구성을 확인하세요.");
        phaseTwo.phraseArea.offsetMin = new Vector2(40f, 80f);
        phaseTwo.phraseArea.offsetMax = new Vector2(-40f, -45f);
        EditorUtility.SetDirty(phaseTwo.phraseArea);
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (!font) throw new InvalidOperationException("한국어 TMP 폰트를 찾지 못했습니다.");
        font.TryAddCharacters("◆◇●▲", out _);
        foreach (char character in "◆◇●▲")
            if (!font.characterLookupTable.ContainsKey(character))
                throw new InvalidOperationException("눈 특수기호 글리프를 준비할 수 없습니다: " + character);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        if (phaseTwo.sources.Length < FigureSlotCount)
        {
            int existing = phaseTwo.sources.Length;
            var sources = new BloodyMarySyllableSource[FigureSlotCount];
            Array.Copy(phaseTwo.sources, sources, existing);
            for (int i = existing; i < sources.Length; i++)
            {
                var clone = UnityEngine.Object.Instantiate(sources[existing - 1].gameObject, phaseTwo.phraseArea, false);
                clone.name = "SyllablePhrase_" + (i + 1).ToString("00");
                var source = clone.GetComponent<BloodyMarySyllableSource>();
                source.phase = phaseTwo;
                source.label = clone.GetComponent<TextMeshProUGUI>();
                source.group = clone.GetComponent<CanvasGroup>();
                source.Clear();
                sources[i] = source;
                Undo.RegisterCreatedObjectUndo(clone, "Add fixed Bloody Mary phrase");
            }
            phaseTwo.sources = sources;
        }
        ConfigureFigureLayout(phaseTwo);
        ConfigureInput(phaseTwo);
        EditorUtility.SetDirty(phaseTwo);
        EditorSceneManager.SaveScene(scene);
        return "레퍼런스 문구 120개와 입력창 정렬을 저장했습니다.";
    }

    static void ConfigureFigureLayout(BloodyMaryPhaseTwoController phaseTwo)
    {
        if (phaseTwo.sources.Length != FigureSlotCount || FigureY.Length != FigureX.Length)
            throw new InvalidOperationException("고정 문구 배치 수가 맞지 않습니다.");
        int index = 0;
        var area = phaseTwo.phraseArea.rect;
        for (int row = 0; row < FigureY.Length; row++)
        {
            for (int column = 0; column < FigureX[row].Length; column++)
            {
                var source = phaseTwo.sources[index++];
                source.figureRow = row;
                source.figurePhrase = FigurePhrase(row, column);
                bool eye = source.figurePhrase == "◆◆◆" || source.figurePhrase == "◇●◇" ||
                    source.figurePhrase == "▲●▲";
                float offsetX = eye ? 0f : ((row * 11 + column * 7) % 5 - 2) * .006f;
                float offsetY = eye ? 0f : ((row * 3 + column * 5) % 3 - 1) * .006f;
                source.figurePosition = new Vector2(FigureX[row][column] + offsetX, FigureY[row] + offsetY);
                source.figureAngle = 0f;
                source.figureFontSize = FigureSize(row, column, source.figurePhrase);
                source.label.text = source.figurePhrase;
                source.label.fontSize = source.figureFontSize;
                source.label.color = Red;
                source.Rect.anchoredPosition = new Vector2(source.figurePosition.x * area.width,
                    source.figurePosition.y * area.height);
                source.Rect.localRotation = Quaternion.identity;
                source.Rect.sizeDelta = new Vector2(Mathf.Max(72f, source.figurePhrase.Length * source.figureFontSize + 20f), 72f);
                source.group.alpha = 0f;
                source.group.blocksRaycasts = false;
                source.gameObject.SetActive(false);
                EditorUtility.SetDirty(source);
                EditorUtility.SetDirty(source.label);
                EditorUtility.SetDirty(source.Rect);
            }
        }
        if (index != FigureSlotCount) throw new InvalidOperationException("고정 문구 수가 맞지 않습니다: " + index);
    }

    static void ConfigureInput(BloodyMaryPhaseTwoController phaseTwo)
    {
        var input = phaseTwo.selectedSlots[0].transform.parent as RectTransform;
        if (!input || !input.GetComponent<UnityEngine.UI.Image>())
            throw new InvalidOperationException("2페이즈 입력창을 찾지 못했습니다.");
        input.anchoredPosition = new Vector2(0f, 150f);
        input.sizeDelta = new Vector2(2500f, 250f);
        var image = input.GetComponent<UnityEngine.UI.Image>();
        image.color = new Color(.015f, .015f, .02f, 1f);
        image.raycastTarget = false;
        var outline = input.GetComponent<UnityEngine.UI.Outline>();
        if (!outline) outline = input.gameObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(.8f, .8f, .8f, 1f);
        outline.effectDistance = new Vector2(3f, -3f);
        float x = -1100f;
        for (int i = 0; i < phaseTwo.selectedSlots.Length; i++)
        {
            var slot = phaseTwo.selectedSlots[i].transform as RectTransform;
            if (!slot || slot.parent != input) throw new InvalidOperationException("입력 음절 슬롯의 부모를 확인하세요.");
            slot.anchoredPosition = new Vector2(x, 0f);
            slot.sizeDelta = new Vector2(80f, 110f);
            EditorUtility.SetDirty(slot);
            x += 85f + (i == 0 || i == 3 || i == 5 ? 45f : 0f);
        }
        EditorUtility.SetDirty(input);
        EditorUtility.SetDirty(image);
        EditorUtility.SetDirty(outline);
    }

    static void AddCardThreat()
    {
        var prefab = PrefabUtility.LoadPrefabContents(CardPath);
        if (!prefab) throw new InvalidOperationException("CallCard 프리팹을 찾지 못했습니다.");
        try
        {
            var card = prefab.GetComponent<BloodyMaryCallCard>();
            if (!card) throw new InvalidOperationException("CallCard 컴포넌트가 없습니다.");
            if (card.threatOverlay) return;
            var overlay = NewRect("ThreatOverlay", prefab.transform);
            Stretch(overlay, 14f, 14f, 14f, 14f);
            var image = overlay.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            var label = NewText("ThreatText", overlay, 49f, Red);
            Stretch(label.rectTransform, 18f, 18f, 18f, 18f);
            label.text = "너는 죽어야 해";
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            card.threatOverlay = overlay.gameObject;
            overlay.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(prefab, CardPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.localScale = Vector3.one;
        return rect;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, float size, Color color)
    {
        var rect = NewRect(name, parent);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.text = "";
        return text;
    }

    static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    static void Top(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
        rect.pivot = new Vector2(.5f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }
}
