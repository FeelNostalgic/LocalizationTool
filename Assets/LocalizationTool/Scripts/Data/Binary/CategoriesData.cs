using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationTool.Data.Binary
{
    [Serializable]
    public class CategoriesData
    {
        public string defaultCategory;
        
        //This is always ordered
        public List<CategoryTuple> orderedCategories;
        public List<string> Categories => orderedCategories.Select(c => c.Category).ToList();

        public CategoriesData()
        {
            orderedCategories = new List<CategoryTuple>();
        }
        
        public void Add(string category)
        {
            var newIndex = orderedCategories.Count == 0 ? 1 : orderedCategories.Max(item => item.Index) + 1;
            if (defaultCategory.Equals(category)) newIndex = 0;
            orderedCategories.Add(new CategoryTuple
            {
                Index = newIndex,
                Category = category
            });

            orderedCategories = orderedCategories.OrderBy(tuple => tuple.Index).ToList();
        }

        public bool Contains(string category)
        {
            return orderedCategories.FirstOrDefault(tuple => tuple.Category.Equals(category)) != default;
        }
        
        public void Clear()
        {
           orderedCategories.Clear();
        }

        public void Remove(string category)
        {
            var tuple = orderedCategories.First(tuple => tuple.Category.Equals(category));
            orderedCategories.Remove(tuple);
            UpdateIndexes();
        }

        public void ChangeName(string oldName, string newName)
        {
            var tuple = orderedCategories.First(tuple => tuple.Category.Equals(oldName));
            if (defaultCategory.Equals(oldName)) defaultCategory = newName;
            tuple.Category = newName;
        }

        public CategoryTuple ChangeIndex(int oldIndex, int newIndex)
        {
            var itemToChange = orderedCategories.First(tuple => tuple.Index == oldIndex);

            if (oldIndex < newIndex)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var item in orderedCategories.Where(tuple => tuple.Index > oldIndex && tuple.Index <= newIndex))
                {
                    item.Index--;
                }
            }
            else if (oldIndex > newIndex)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var item in orderedCategories.Where(tuple => tuple.Index >= newIndex && tuple.Index < oldIndex))
                {
                    item.Index++;
                }
            }

            // Update index
            itemToChange.Index = newIndex;

            // Reorder 
            orderedCategories = orderedCategories.OrderBy(tuple => tuple.Index).ToList();

            UpdateIndexes();
            return itemToChange;
        }

        public void ChangeIndex(string category, int newIndex)
        {
            var itemToChange = orderedCategories.First(tuple => tuple.Category.Equals(category));
            itemToChange.Index = newIndex;
            
            orderedCategories = orderedCategories.OrderBy(tuple => tuple.Index).ToList();
            
            UpdateIndexes();
        }

        private void UpdateIndexes()
        {
            for (var i = 0; i < orderedCategories.Count; i++)
            {
                orderedCategories[i].Index = i + 1;
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