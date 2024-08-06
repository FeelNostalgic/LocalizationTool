using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationTool.Data.Binary
{
    [Serializable]
    public class CategoriesData
    {
        public string DefaultCategory;
        
        //This is always ordered
        public List<CategoryTuple> OrderedCategories = new();
        public List<string> Categories => OrderedCategories.Select(c => c.Category).ToList();

        public void Add(string category)
        {
            var newIndex = OrderedCategories.Count == 0 ? 1 : OrderedCategories.Max(item => item.Index) + 1;
            OrderedCategories.Add(new CategoryTuple
            {
                Index = newIndex,
                Category = category
            });

            OrderedCategories = OrderedCategories.OrderBy(tuple => tuple.Index).ToList();
        }

        public bool Contains(string category)
        {
            return OrderedCategories.FirstOrDefault(tuple => tuple.Category.Equals(category)) != default;
        }

        public void Remove(string category)
        {
            var tuple = OrderedCategories.First(tuple => tuple.Category.Equals(category));
            OrderedCategories.Remove(tuple);
            UpdateIndexes();
        }

        public void ChangeName(string oldName, string newName)
        {
            var tuple = OrderedCategories.First(tuple => tuple.Category.Equals(oldName));
            if (DefaultCategory.Equals(oldName)) DefaultCategory = newName;
            tuple.Category = newName;
        }

        public CategoryTuple ChangeIndex(int oldIndex, int newIndex)
        {
            var itemToChange = OrderedCategories.First(tuple => tuple.Index == oldIndex);

            if (oldIndex < newIndex)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var item in OrderedCategories.Where(tuple => tuple.Index > oldIndex && tuple.Index <= newIndex))
                {
                    item.Index--;
                }
            }
            else if (oldIndex > newIndex)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var item in OrderedCategories.Where(tuple => tuple.Index >= newIndex && tuple.Index < oldIndex))
                {
                    item.Index++;
                }
            }

            // Update index
            itemToChange.Index = newIndex;

            // Reorder 
            OrderedCategories = OrderedCategories.OrderBy(tuple => tuple.Index).ToList();

            UpdateIndexes();
            return itemToChange;
        }

        public void ChangeIndex(string category, int newIndex)
        {
            var itemToChange = OrderedCategories.First(tuple => tuple.Category.Equals(category));
            itemToChange.Index = newIndex;
            
            OrderedCategories = OrderedCategories.OrderBy(tuple => tuple.Index).ToList();
            
            UpdateIndexes();
        }

        private void UpdateIndexes()
        {
            for (var i = 0; i < OrderedCategories.Count; i++)
            {
                OrderedCategories[i].Index = i + 1;
            }
        }

        [Serializable]
        public class CategoryTuple
        {
            public int Index;
            public string Category;

            public void Deconstruct(out int index, out string category)
            {
                index = Index;
                category = Category;
            }
        }
    }
}