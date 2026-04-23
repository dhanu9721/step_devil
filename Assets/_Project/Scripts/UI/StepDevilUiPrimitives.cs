using UnityEngine;
using UnityEngine.UI;

namespace StepDevil
{
    public static class StepDevilUiPrimitives
    {
        static Sprite s_White;

        public static Sprite WhiteSprite
        {
            get
            {
                if (s_White == null)
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    t.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                    t.Apply();
                    s_White = Sprite.Create(t, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
                }

                return s_White;
            }
        }

        public static Image CreateImage(Transform parent, string name, Color color, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = WhiteSprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }
}
