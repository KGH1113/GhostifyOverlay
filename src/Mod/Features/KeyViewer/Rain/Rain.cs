// Derived from JipperResourcePack by Jongyeol. Copyright (c) 2024, Jongyeol.
// BSD-3-Clause: see Licenses/JipperResourcePack-BSD-3-Clause.txt and THIRD-PARTY-NOTICES.md.
using UnityEngine;
using UnityEngine.UI;

namespace DonQuixoteOverlay.KeyViewerContents
{
    public class Rain
    {
        public readonly RainPool Pool;
        public readonly Graphic Image;
        public readonly GameObject GameObject;
        public readonly RectTransform Transform;
        public readonly bool IsGhost;
        public RawRain RawRain;
        public Rain(RainPool pool, bool isGhost)
        {
            Pool = pool;
            GameObject rainPrefab = GameObject = new GameObject("Rain");
            RectTransform rainTransform = Transform = rainPrefab.AddComponent<RectTransform>();
            rainTransform.SetParent(pool.Transform);
            rainTransform.anchorMin = rainTransform.anchorMax = rainTransform.pivot = new Vector2(0.5f, 1);
            rainTransform.anchoredPosition = rainTransform.sizeDelta = Vector2.zero;
            rainTransform.localScale = Vector3.one;
            var image = rainPrefab.AddComponent<Image>();
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.raycastTarget = false;
            Image = image;
            IsGhost = isGhost;
        }
    }
}
