using System;

namespace RCWorld.DebugMenu
{
    /// <summary>
    /// Stores the menu hierarchy independently from the menu presentation.
    /// </summary>
    public static class DebugMenuRegistry
    {
        private static readonly DebugMenuCategory rootCategory = new DebugMenuCategory("Debug Menu", null);

        public static DebugMenuCategory RootCategory => rootCategory;
        public static event Action ContentChanged;

        public static T Register<T>(T item, string categoryPath) where T : DebugMenuItem
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            DebugMenuCategory category = GetOrCreateCategory(categoryPath);
            category.AddItem(item);
            ContentChanged?.Invoke();
            return item;
        }

        public static DebugMenuCategory GetOrCreateCategory(string categoryPath)
        {
            DebugMenuCategory category = rootCategory;
            if (string.IsNullOrWhiteSpace(categoryPath))
                return category;

            string[] pathParts = categoryPath.Split('/');
            foreach (string part in pathParts)
            {
                string categoryName = part.Trim();
                if (!string.IsNullOrEmpty(categoryName))
                    category = category.GetOrAddChild(categoryName);
            }

            return category;
        }
    }
}
