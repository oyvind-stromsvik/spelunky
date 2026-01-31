using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spelunky {

    [CustomEditor(typeof(Texture2D))]
    [CanEditMultipleObjects]
    public class Texture2DFullWidthInspector : Editor {

        private Editor _builtInEditor;
        private Type _builtInInspectorType;

        private const float CheckerTileSize = 64f;
        private const float PreviewPadding = 1f;
        private static Texture2D s_CheckerTex;
        private static Material s_TransparentPreviewMaterial;

        // Cache multi-selection info; Unity warns about using `targets` inside OnPreviewGUI.
        private int _selectionCount = 1;

        private void OnEnable() {
            _builtInInspectorType = Type.GetType("UnityEditor.TextureInspector, UnityEditor", false);
            if (_builtInInspectorType != null) {
                // Passing `targets` here is OK (not in OnPreviewGUI) and enables multi-object importer UI.
                _builtInEditor = CreateEditor(targets, _builtInInspectorType);
            }

            _selectionCount = targets?.Length ?? 1;
        }

        private void OnDisable() {
            if (_builtInEditor != null) {
                DestroyImmediate(_builtInEditor);
                _builtInEditor = null;
            }
        }

        public override void OnInspectorGUI() {
            // Keep Unity's regular texture importer/inspector UI.
            if (_builtInEditor != null) {
                _builtInEditor.OnInspectorGUI();
            }
            else {
                DrawDefaultInspector();
            }

            // Update cached selection count (safe here).
            _selectionCount = targets?.Length ?? 1;
        }

        public override bool HasPreviewGUI() {
            return target is Texture2D;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            // IMPORTANT: Unity warns if we access `targets` here. Only use `target`.
            Texture2D tex = target as Texture2D;
            if (tex == null) {
                return;
            }

            EnsureCheckerTexture();
            EnsureTransparentPreviewMaterial();

            // Draw a checkerboard background to represent transparency.
            Rect uv = new Rect(0, 0, r.width / CheckerTileSize, r.height / CheckerTileSize);
            GUI.DrawTextureWithTexCoords(r, s_CheckerTex, uv, true);

            // Slightly inset to avoid sampling outside edges (helps reduce bleed when scaling).
            Rect inner = new Rect(r.x + PreviewPadding, r.y + PreviewPadding,
                r.width - PreviewPadding * 2f, r.height - PreviewPadding * 2f);

            // Always scale-to-fit; no zoom buttons.
            EditorGUI.DrawPreviewTexture(inner, tex, s_TransparentPreviewMaterial, ScaleMode.ScaleToFit);

            // Overlay selection count for multi-select (cached outside preview GUI).
            if (_selectionCount > 1) {
                Rect labelRect = new Rect(r.x + 6f, r.y + 6f, r.width - 12f, 18f);
                EditorGUI.DropShadowLabel(labelRect, _selectionCount + " selected");
            }
        }

        private static void EnsureCheckerTexture() {
            if (s_CheckerTex != null) {
                return;
            }

            // Small checkerboard (two light/dark grays), repeated.
            const int size = 16;
            s_CheckerTex = new Texture2D(size, size, TextureFormat.RGBA32, false) {
                name = "InspectorPreview_Checker",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32 c0 = new Color32(255, 255, 255, 255);
            Color32 c1 = new Color32(205, 205, 205, 255);
            int block = 4;

            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    bool odd = (x / block + y / block) % 2 == 1;
                    s_CheckerTex.SetPixel(x, y, odd ? c1 : c0);
                }
            }

            s_CheckerTex.Apply(false, true);
        }

        private static void EnsureTransparentPreviewMaterial() {
            if (s_TransparentPreviewMaterial != null) {
                return;
            }

            // Best option: Unity's own preview material. It handles alpha correctly across pipelines.
            // It's internal, so we access it via reflection with safe fallbacks.
            try {
                Type matUtil = Type.GetType("UnityEditor.MaterialUtility, UnityEditor", false);
                if (matUtil != null) {
                    MethodInfo mi = matUtil.GetMethod("GetDefaultMaterial", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (mi != null) {
                        Material m = mi.Invoke(null, null) as Material;
                        if (m != null) {
                            s_TransparentPreviewMaterial = m;
                            return;
                        }
                    }
                }

                // Another common internal: UnityEditor.EditorGUIUtility.GetBuiltinExtraResource<Material>("Default-Material.mat")
                MethodInfo extraRes = typeof(EditorGUIUtility).GetMethod("GetBuiltinExtraResource", BindingFlags.Static | BindingFlags.Public);
                if (extraRes != null && extraRes.IsGenericMethodDefinition) {
                    Material m = extraRes.MakeGenericMethod(typeof(Material)).Invoke(null, new object[] { "Default-Material.mat" }) as Material;
                    if (m != null) {
                        s_TransparentPreviewMaterial = m;
                        return;
                    }
                }
            }
            catch {
                // ignore and fall back below
            }

            // Fallback. Prefer a shader that blends alpha.
            Shader shader = Shader.Find("Unlit/Transparent");
            if (shader == null) {
                shader = Shader.Find("UI/Unlit/Transparent");
            }

            if (shader == null) {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null) {
                s_TransparentPreviewMaterial = new Material(shader) {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
        }

    }

}
