using System.Collections.Generic;

namespace RCWorld.DebugMenu
{
    // Holds navigation state without depending on Unity UI components.
    internal sealed class DebugMenuNavigation
    {
        public DebugMenuCategory CurrentCategory { get; private set; }
        public int SelectedIndex { get; private set; }

        public void Reset(DebugMenuCategory rootCategory)
        {
            CurrentCategory = rootCategory;
            SelectedIndex = 0;
        }

        public void Move(int direction)
        {
            int count = GetEntryCount();
            if (count == 0) return;
            SelectedIndex = (SelectedIndex + direction) % count;
            if (SelectedIndex < 0) SelectedIndex += count;
        }

        public void OpenCategory(DebugMenuCategory category)
        {
            CurrentCategory = category;
            SelectedIndex = 0;
        }

        public void GoBack()
        {
            if (CurrentCategory.Parent == null) return;
            CurrentCategory = CurrentCategory.Parent;
            SelectedIndex = 0;
        }

        public bool TryGetSelectedItem(out DebugMenuItem item)
        {
            int itemStartIndex = CurrentCategory.ChildCategories.Count;
            int itemIndex = SelectedIndex - itemStartIndex;
            if (itemIndex >= 0 && itemIndex < CurrentCategory.Items.Count)
            {
                item = CurrentCategory.Items[itemIndex];
                return true;
            }
            item = null;
            return false;
        }

        public bool TryGetSelectedCategory(out DebugMenuCategory category)
        {
            if (SelectedIndex >= 0 && SelectedIndex < CurrentCategory.ChildCategories.Count)
            {
                category = CurrentCategory.ChildCategories[SelectedIndex];
                return true;
            }
            category = null;
            return false;
        }

        private int GetEntryCount()
        {
            return CurrentCategory.ChildCategories.Count + CurrentCategory.Items.Count;
        }
    }
}
