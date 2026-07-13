using System;
using System.Collections.Generic;

namespace RCWorld.DebugMenu
{
    // A named container for debug items and child categories.
    public sealed class DebugMenuCategory
    {
        private readonly List<DebugMenuCategory> childCategories = new List<DebugMenuCategory>();
        private readonly List<DebugMenuItem> items = new List<DebugMenuItem>();

        internal DebugMenuCategory(string name, DebugMenuCategory parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; }
        public DebugMenuCategory Parent { get; }
        public IReadOnlyList<DebugMenuCategory> ChildCategories => childCategories;
        public IReadOnlyList<DebugMenuItem> Items => items;

        internal DebugMenuCategory GetOrAddChild(string name)
        {
            foreach (DebugMenuCategory child in childCategories)
            {
                if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                    return child;
            }

            DebugMenuCategory category = new DebugMenuCategory(name, this);
            childCategories.Add(category);
            return category;
        }

        internal void AddItem(DebugMenuItem item)
        {
            items.Add(item);
        }
    }
}
