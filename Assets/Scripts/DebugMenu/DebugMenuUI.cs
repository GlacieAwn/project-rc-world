using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RCWorld.DebugMenu
{
    /// <summary>
    /// Minimal uGUI presentation for the menu. It can be replaced without changing the registry or navigation code.
    /// </summary>
    internal sealed class DebugMenuUI
    {
        private readonly GameObject rootObject;
        private readonly Text titleText;
        private readonly Text entriesText;

        public DebugMenuUI(Transform parent)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rootObject = CreatePanel(parent);
            titleText = CreateText("Title", rootObject.transform, font, 24, TextAnchor.UpperLeft);
            entriesText = CreateText("Entries", rootObject.transform, font, 18, TextAnchor.UpperLeft);

            SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -45f), new Vector2(-20f, -20f));
            SetRect(entriesText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 20f), new Vector2(-20f, -55f));
            SetVisible(false);
        }

        public void SetVisible(bool visible) { rootObject.SetActive(visible); }

        public void Render(DebugMenuCategory category, int selectedIndex)
        {
            titleText.text = category.Name;
            List<string> lines = new List<string>();
            int entryIndex = 0;
            foreach (DebugMenuCategory child in category.ChildCategories)
            {
                lines.Add(FormatEntry("[ " + child.Name + " ]", string.Empty, entryIndex == selectedIndex));
                entryIndex++;
            }
            foreach (DebugMenuItem item in category.Items)
            {
                lines.Add(FormatEntry(item.Name, item.ValueText, entryIndex == selectedIndex));
                entryIndex++;
            }
            if (lines.Count == 0)
                lines.Add("(No debug items registered)");
            entriesText.text = string.Join("\n", lines);
        }

        private static string FormatEntry(string name, string value, bool selected)
        {
            string prefix;
            if (selected) prefix = "> ";
            else prefix = "  ";
            if (string.IsNullOrEmpty(value)) return prefix + name;
            return prefix + name + ": " + value;
        }

        private static GameObject CreatePanel(Transform parent)
        {
            GameObject panel = new GameObject("DebugMenuUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(Image));
            panel.transform.SetParent(parent, false);
            Canvas canvas = panel.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = short.MaxValue;
            Image image = panel.GetComponent<Image>(); image.color = new Color(0f, 0f, 0f, 0.82f);
            RectTransform rectTransform = panel.GetComponent<RectTransform>(); rectTransform.anchorMin = new Vector2(0.1f, 0.1f); rectTransform.anchorMax = new Vector2(0.65f, 0.9f); rectTransform.offsetMin = Vector2.zero; rectTransform.offsetMax = Vector2.zero;
            return panel;
        }

        private static Text CreateText(string name, Transform parent, Font font, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text)); textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>(); text.font = font; text.fontSize = fontSize; text.alignment = alignment; text.horizontalOverflow = HorizontalWrapMode.Overflow; text.verticalOverflow = VerticalWrapMode.Overflow; text.color = Color.white;
            return text;
        }

        private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin; rectTransform.anchorMax = anchorMax; rectTransform.offsetMin = offsetMin; rectTransform.offsetMax = offsetMax;
        }
    }
}
