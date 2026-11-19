using System;
using System.Collections.Generic;
using UnityEngine;

public enum SiteSlotKind { Text, Image, Body }
public enum SiteBlockKind { Paragraph, Image, Link, Prefab }

[Serializable] public class SiteArea
{
    public Rect rect;
    public string eventId;
}

[Serializable] public class SiteBlock
{
    public SiteBlockKind kind;
    public string text, assetId, destinationId;
    public List<SiteArea> areas = new List<SiteArea>();
}

[Serializable] public class SiteSlotContent
{
    public string slotId;
    public List<SiteBlock> blocks = new List<SiteBlock>();
}

[Serializable] public class SitePageData
{
    public string id, address, templateId;
    public List<SiteSlotContent> slots = new List<SiteSlotContent>();

    public static string Normalize(string input)
    {
        var url = (input ?? "").Trim().ToLowerInvariant();
        if (url.StartsWith("https://")) url = url.Substring(8);
        else if (url.StartsWith("http://")) url = url.Substring(7);
        return url.TrimEnd('/');
    }
}
