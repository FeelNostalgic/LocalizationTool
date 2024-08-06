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
        public List<LanguageTuple> OrderedLanguages = new();
        public List<string> Languages => OrderedLanguages.Select(tuple => tuple.Language).ToList();

        public void Add(string language)
        {
            var newIndex = OrderedLanguages.Count == 0 ? 1 : OrderedLanguages.Max(item => item.Index) + 1;
            OrderedLanguages.Add(new LanguageTuple
            {
                Index = newIndex,
                Language = language
            });

            OrderedLanguages = OrderedLanguages.OrderBy(tuple => tuple.Index).ToList();
        }

        public bool Contains(string language)
        {
            return OrderedLanguages.FirstOrDefault(tuple => tuple.Language.Equals(language)) != default;
        }

        public int Count()
        {
            return OrderedLanguages.Count;
        }

        public void Remove(string language)
        {
            var tuple = OrderedLanguages.First(tuple => tuple.Language.Equals(language));
            OrderedLanguages.Remove(tuple);
            UpdateIndexes();
        }

        public void ChangeName(string oldName, string newName)
        {
            var tuple = OrderedLanguages.First(tuple => tuple.Language.Equals(oldName));
            tuple.Language = newName;
            if (FavouriteLanguage.Equals(oldName)) FavouriteLanguage = newName;
        }

        public LanguageTuple ChangeIndex(int oldIndex, int newIndex)
        {
            var itemToChange = OrderedLanguages.First(tuple => tuple.Index == oldIndex);

            if (oldIndex < newIndex)
            {
                // Scroll down elements between oldIndex and newIndex
                foreach (var item in OrderedLanguages.Where(tuple => tuple.Index > oldIndex && tuple.Index <= newIndex))
                {
                    item.Index--;
                }
            }
            else if (oldIndex > newIndex)
            {
                // Scroll up elements between oldIndex and newIndex
                foreach (var item in OrderedLanguages.Where(tuple => tuple.Index >= newIndex && tuple.Index < oldIndex))
                {
                    item.Index++;
                }
            }

            // Update index
            itemToChange.Index = newIndex;

            // Reorder 
            OrderedLanguages = OrderedLanguages.OrderBy(tuple => tuple.Index).ToList();

            UpdateIndexes();
            return itemToChange;
        }

        private void UpdateIndexes()
        {
            for (var i = 0; i < OrderedLanguages.Count; i++)
            {
                OrderedLanguages[i].Index = i + 1;
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