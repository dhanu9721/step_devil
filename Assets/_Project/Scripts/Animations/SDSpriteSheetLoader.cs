using UnityEngine;

namespace StepDevil
{
    // ══════════════════════════════════════════════════════════════
    //  SDSpriteSheetLoader — helper to load sliced sprite sheets
    //  from Resources folders at runtime.
    //
    //  Usage:
    //    // Load all slices from "Resources/Sprites/devil_idle"
    //    var frames = SDSpriteSheetLoader.Load("Sprites/devil_idle");
    //    animator.Play(frames, fps: 12);
    //
    //  Setup in Unity:
    //    1. Import your sprite sheet PNG (e.g. devil_idle.png)
    //    2. Set Texture Type → Sprite (2D and UI)
    //    3. Set Sprite Mode → Multiple
    //    4. Open Sprite Editor → slice into frames
    //    5. Place in a Resources/ folder
    //    6. Call SDSpriteSheetLoader.Load("path/without/extension")
    // ══════════════════════════════════════════════════════════════
    public static class SDSpriteSheetLoader
    {
        /// <summary>
        /// Loads all sprite slices from a multi-sprite texture in Resources.
        /// Returns them sorted by name (frame_0, frame_1, ...).
        /// </summary>
        /// <param name="resourcePath">Path relative to Resources folder, no extension.
        /// E.g. "Sprites/devil_idle" for Resources/Sprites/devil_idle.png</param>
        public static Sprite[] Load(string resourcePath)
        {
            var sprites = Resources.LoadAll<Sprite>(resourcePath);
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"[SDSpriteSheetLoader] No sprites found at Resources/{resourcePath}");
                return System.Array.Empty<Sprite>();
            }

            // Sort by name to ensure correct frame order
            System.Array.Sort(sprites, (a, b) => NaturalCompare(a.name, b.name));
            return sprites;
        }

        /// <summary>
        /// Loads a single sprite from Resources.
        /// </summary>
        public static Sprite LoadSingle(string resourcePath)
        {
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
                Debug.LogWarning($"[SDSpriteSheetLoader] Sprite not found at Resources/{resourcePath}");
            return sprite;
        }

        /// <summary>Natural string comparison so "frame_2" sorts before "frame_10".</summary>
        static int NaturalCompare(string a, string b)
        {
            int ia = 0, ib = 0;
            while (ia < a.Length && ib < b.Length)
            {
                if (char.IsDigit(a[ia]) && char.IsDigit(b[ib]))
                {
                    long na = 0, nb = 0;
                    while (ia < a.Length && char.IsDigit(a[ia]))
                        na = na * 10 + (a[ia++] - '0');
                    while (ib < b.Length && char.IsDigit(b[ib]))
                        nb = nb * 10 + (b[ib++] - '0');
                    if (na != nb) return na.CompareTo(nb);
                }
                else
                {
                    if (a[ia] != b[ib]) return a[ia].CompareTo(b[ib]);
                    ia++; ib++;
                }
            }
            return a.Length.CompareTo(b.Length);
        }
    }
}
