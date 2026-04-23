#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StepDevil.Editor
{
    /// <summary>
    /// Extract animated GIF frames to PNGs and import as sprites (single files or one horizontal sheet).
    /// Uses System.Drawing on Windows Editor only.
    /// </summary>
    public sealed class StepDevilGifToSpritesWindow : EditorWindow
    {
        enum OutputMode
        {
            SeparatePngs,
            HorizontalSpriteSheet,
        }

        string _gifPath = "";
        string _outputFolder = "Assets";
        OutputMode _mode = OutputMode.SeparatePngs;
        string _baseName = "frames";
        float _pixelsPerUnit = 100f;
        FilterMode _filterMode = FilterMode.Point;
        TextureImporterCompression _compression = TextureImporterCompression.Uncompressed;

        [MenuItem("Step Devil/GIF → Sprites…")]
        public static void Open()
        {
            var w = GetWindow<StepDevilGifToSpritesWindow>(true, "GIF → Sprites", true);
            w.minSize = new Vector2(420, 260);
            if (string.IsNullOrEmpty(w._outputFolder) || w._outputFolder == "Assets")
                w._outputFolder = Application.dataPath;
        }

        [MenuItem("Assets/Step Devil/Extract GIF to Sprites", false, 1300)]
        public static void ExtractFromSelection()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                return;

            Open();
            var w = GetWindow<StepDevilGifToSpritesWindow>();
            var full = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
            w._gifPath = full.Replace('\\', '/');
            w._baseName = Path.GetFileNameWithoutExtension(path);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                w._outputFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", dir)).Replace('\\', '/');
        }

        [MenuItem("Assets/Step Devil/Extract GIF to Sprites", true)]
        public static bool ValidateExtractFromSelection()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
        }

        void OnGUI()
        {
#if !UNITY_EDITOR_WIN
            EditorGUILayout.HelpBox(
                "GIF extraction uses System.Drawing and is only supported in the Windows Unity Editor. " +
                "On macOS/Linux, export frames as PNG externally, then slice in the Sprite Editor.",
                MessageType.Warning);
            return;
#else
            EditorGUILayout.LabelField("Source GIF", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _gifPath = EditorGUILayout.TextField(_gifPath);
            if (GUILayout.Button("Browse…", GUILayout.Width(80)))
            {
                var p = EditorUtility.OpenFilePanel("Select GIF", Application.dataPath, "gif");
                if (!string.IsNullOrEmpty(p))
                {
                    _gifPath = p.Replace('\\', '/');
                    if (string.IsNullOrEmpty(_baseName) || _baseName == "frames")
                        _baseName = Path.GetFileNameWithoutExtension(_gifPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _outputFolder = EditorGUILayout.TextField("Folder (inside project)", _outputFolder);
            if (GUILayout.Button("Browse…", GUILayout.Width(80)))
            {
                var p = EditorUtility.SaveFolderPanel("Output folder", _outputFolder, "");
                if (!string.IsNullOrEmpty(p))
                    _outputFolder = p.Replace('\\', '/');
            }
            EditorGUILayout.EndHorizontal();

            _baseName = EditorGUILayout.TextField("Base name", _baseName);
            _mode = (OutputMode)EditorGUILayout.EnumPopup("Import as", _mode);
            _pixelsPerUnit = EditorGUILayout.FloatField("Pixels per unit", _pixelsPerUnit);
            _filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter mode", _filterMode);
            _compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression", _compression);

            EditorGUILayout.Space(10);
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_gifPath) || !File.Exists(_gifPath)))
            {
                if (GUILayout.Button("Convert", GUILayout.Height(32)))
                    Convert();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(
                "Separate PNGs: one sprite per frame (frame_000, frame_001, …), easy to assign to SDSpriteAnimator.\n" +
                "Sprite sheet: one horizontal strip + automatic slice rects for SDSpriteSheetLoader / animator.",
                MessageType.Info);
#endif
        }

#if UNITY_EDITOR_WIN
        void Convert()
        {
            if (!EnsureOutputInsideProject())
                return;

            try
            {
                using var bmp = new System.Drawing.Bitmap(_gifPath);
                var dim = new System.Drawing.Imaging.FrameDimension(bmp.FrameDimensionsList[0]);
                int n = bmp.GetFrameCount(dim);
                if (n <= 0)
                {
                    EditorUtility.DisplayDialog("GIF → Sprites", "No frames found in GIF.", "OK");
                    return;
                }

                EditorUtility.DisplayProgressBar("GIF → Sprites", "Reading frames…", 0f);
                var frames = new System.Drawing.Bitmap[n];
                int w = bmp.Width, h = bmp.Height;
                for (int i = 0; i < n; i++)
                {
                    bmp.SelectActiveFrame(dim, i);
                    var frame = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (var g = System.Drawing.Graphics.FromImage(frame))
                    {
                        g.Clear(System.Drawing.Color.Transparent);
                        g.DrawImage(bmp, 0, 0, w, h);
                    }
                    frames[i] = frame;
                    EditorUtility.DisplayProgressBar("GIF → Sprites", $"Frame {i + 1}/{n}", (float)(i + 1) / n);
                }

                try
                {
                    if (_mode == OutputMode.SeparatePngs)
                        WriteSeparatePngs(frames);
                    else
                        WriteSpriteSheet(frames, w, h, n);
                }
                finally
                {
                    for (int i = 0; i < frames.Length; i++)
                        frames[i]?.Dispose();
                }

                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("GIF → Sprites", $"Done. {n} frame(s) written under {_outputFolder}.", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(e);
                EditorUtility.DisplayDialog("GIF → Sprites", "Failed: " + e.Message, "OK");
            }
        }

        string ResolveOutputFolderAbsolute()
        {
            var s = _outputFolder.Trim().Replace('\\', '/');
            if (s.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, "Assets", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(Path.Combine(Application.dataPath, "..", s)).Replace('\\', '/');
            }

            return Path.GetFullPath(s).Replace('\\', '/');
        }

        bool EnsureOutputInsideProject()
        {
            var data = Application.dataPath.Replace('\\', '/');
            _outputFolder = ResolveOutputFolderAbsolute();
            var folder = _outputFolder;
            if (!folder.StartsWith(data, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog(
                    "GIF → Sprites",
                    "Output folder must be inside the project's Assets directory (e.g. Assets/_Project/Sprites).",
                    "OK");
                return false;
            }
            return true;
        }

        string AssetsRelativePath(string absolutePath)
        {
            var data = Application.dataPath.Replace('\\', '/');
            var abs = Path.GetFullPath(absolutePath).Replace('\\', '/');
            if (!abs.StartsWith(data, StringComparison.OrdinalIgnoreCase))
                return null;
            var rel = abs.Substring(data.Length).TrimStart('/');
            return "Assets/" + rel;
        }

        void WriteSeparatePngs(System.Drawing.Bitmap[] frames)
        {
            Directory.CreateDirectory(_outputFolder);
            var safeBase = SanitizeFileName(_baseName);
            var paths = new string[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                var name = $"{safeBase}_frame_{i:000}.png";
                var abs = Path.Combine(_outputFolder, name);
                frames[i].Save(abs, System.Drawing.Imaging.ImageFormat.Png);
                paths[i] = AssetsRelativePath(abs);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            for (var i = 0; i < paths.Length; i++)
            {
                if (!string.IsNullOrEmpty(paths[i]))
                    ConfigureImporter(paths[i], SpriteImportMode.Single);
            }
        }

        void WriteSpriteSheet(System.Drawing.Bitmap[] frames, int w, int h, int n)
        {
            Directory.CreateDirectory(_outputFolder);
            var safeBase = SanitizeFileName(_baseName);
            var sheet = new System.Drawing.Bitmap(w * n, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = System.Drawing.Graphics.FromImage(sheet))
            {
                g.Clear(System.Drawing.Color.Transparent);
                for (int i = 0; i < n; i++)
                    g.DrawImage(frames[i], i * w, 0, w, h);
            }

            var fileName = $"{safeBase}_sheet.png";
            var abs = Path.Combine(_outputFolder, fileName);
            sheet.Save(abs, System.Drawing.Imaging.ImageFormat.Png);
            sheet.Dispose();

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var rel = AssetsRelativePath(abs);
            if (rel == null)
                return;

            var importer = AssetImporter.GetAtPath(rel) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = _pixelsPerUnit;
            importer.filterMode = _filterMode;
            importer.textureCompression = _compression;

            var meta = new SpriteMetaData[n];
            for (int i = 0; i < n; i++)
            {
                meta[i].name = $"frame_{i}";
                meta[i].rect = new Rect(i * w, 0, w, h);
                meta[i].pivot = new Vector2(0.5f, 0.5f);
                meta[i].border = Vector4.zero;
            }
            importer.spritesheet = meta;
            importer.SaveAndReimport();
        }

        void ConfigureImporter(string assetPath, SpriteImportMode mode)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = mode;
            importer.spritePixelsPerUnit = _pixelsPerUnit;
            importer.filterMode = _filterMode;
            importer.textureCompression = _compression;
            importer.SaveAndReimport();
        }

        static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "frames";
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
#endif
    }
}
#endif
