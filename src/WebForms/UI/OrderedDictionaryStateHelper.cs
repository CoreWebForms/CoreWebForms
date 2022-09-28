// MIT License.

#nullable disable

using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI;

internal static class OrderedDictionaryStateHelper
{
    public static void LoadViewState(IOrderedDictionary dictionary, ArrayList state)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (state != null)
        {
            for (int i = 0; i < state.Count; i++)
            {
                Pair pairEntry = (Pair)state[i];
                dictionary.Add(pairEntry.First, pairEntry.Second);
            }
        }
    }

    public static ArrayList SaveViewState(IOrderedDictionary dictionary)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        ArrayList list = new ArrayList(dictionary.Count);
        foreach (DictionaryEntry entry in dictionary)
        {
            list.Add(new Pair(entry.Key, entry.Value));
        }
        return list;
    }
}

