using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationTool.Data.Binary
{
    [Serializable]
    public class LanguagesData
    {
        public string FavouriteLanguage;

        //This is always ordered
        public List<LanguageTuple> orderedLanguages;
        public List<string> Languages => orderedLanguages.Select(tuple => tuple.Language).ToList();

        public LanguagesData()
        {
            orderedLanguages = new List<LanguageTuple>();
        }
        
        public void Add(string language)
        {
            var newIndex = orderedLanguages.Count == 0 ? 1 : orderedLanguages.Max(item => item.Index) + 1;
            orderedLanguages.Add(new LanguageTuple
            {
                Index = newIndex,
                Language = language
            });

            orderedLanguages = orderedLanguages.OrderBy(tuple => tuple.Index).ToList();
        }

        public bool Contains(string language)
        {
            return orderedLanguages.FirstOrDefault(tuple => tuple.Language.Equals(language)) != default;
        }

        public int Count()
        {
            return orderedLanguages.Count;
        }

        public void Clear()
        {
            orderedLanguages.Clear();
        }

        public void Remove(string language)
        {
            var tuple = orderedLanguages.First(tuple => tuple.Language.Equals(language));
            orderedLanguages.Remove(tuple);
            UpdateIndexes();
        }

        public void ChangeName(string oldName, string newName)
        {
            var tuple = orderedLanguages.First(tuple => tuple.Language.Equals(oldName));
            tuple.Language = newName;
            if (FavouriteLanguage.Equals(oldName)) FavouriteLanguage = newName;
        }

        public LanguageTuple ChangeIndex(int oldIndex, int newIndex)
        {
            var itemToChange = orderedLanguages.First(tuple => tuple.Index == oldIndex);

            if (oldIndex < newIndex)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var item in orderedLanguages.Where(tuple => tuple.Index > oldIndex && tuple.Index <= newIndex))
                {
                    item.Index--;
                }
            }
            else if (oldIndex > newIndex)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var item in orderedLanguages.Where(tuple => tuple.Index >= newIndex && tuple.Index < oldIndex))
                {
                    item.Index++;
                }
            }

            // Update index
            itemToChange.Index = newIndex;

            // Reorder 
            orderedLanguages = orderedLanguages.OrderBy(tuple => tuple.Index).ToList();

            UpdateIndexes();
            return itemToChange;
        }
        
        public void ChangeIndex(string category, int newIndex)
        {
            var itemToChange = orderedLanguages.First(tuple => tuple.Language.Equals(category));
            itemToChange.Index = newIndex;
            
            orderedLanguages = orderedLanguages.OrderBy(tuple => tuple.Index).ToList();
            
            UpdateIndexes();
        }

        private void UpdateIndexes()
        {
            for (var i = 0; i < orderedLanguages.Count; i++)
            {
                orderedLanguages[i].Index = i + 1;
            }
        }

        [Serializable]
        public class LanguageTuple
        {
            public int Index;
            public string Language;

            public void Deconstruct(out int index, out string language)
            {
                index = Index;
                language = Language;
            }
        }
    }
}