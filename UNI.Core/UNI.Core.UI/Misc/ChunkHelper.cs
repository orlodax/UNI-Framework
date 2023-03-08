using System.Collections.Generic;
using System.Linq;
using UNI.Core.UI.CustomControls.GridBox;

namespace UNI.Core.UI.Misc
{
    internal static class ChunkHelper
    {
        internal static IEnumerable<IEnumerable<T>> Chunkify<T>(IEnumerable<T> fullList, int batchSize)
        {
            int total = 0;
            while (total < fullList.Count())
            {
                yield return fullList.Skip(total).Take(batchSize);
                total += batchSize;
            }
        }

        internal static IEnumerable<IEnumerable<T>> ChunkifyAll<T>(IEnumerable<T> fullList, int batchSize)
        {
            int pagesId = 0;
            int batchCounter = batchSize;

            var chunks = new List<List<T>>() { new List<T>() };

            foreach (T item in fullList)
            {
                if (batchCounter <= 0)
                {
                    chunks.Add(new List<T>());
                    pagesId++;
                    batchCounter = batchSize;
                }
                if (item.GetType() != typeof(GridBox))
                {
                    chunks[pagesId].Add(item);
                    batchCounter--;
                }
                else
                {
                    var gridBoxVM = (item as GridBox).DataContext as dynamic;
                    if (gridBoxVM.itemsSource.Count != 0)
                    {
                        if (gridBoxVM.itemsSource.Count * 3.2 > batchCounter)
                        {
                            chunks.Add(new List<T>());
                            pagesId++;
                            batchCounter = batchSize;
                        }
                        chunks[pagesId].Add(item);
                        batchCounter -= gridBoxVM.itemsSource.Count * 3.2;
                    }
                }
            }
            return chunks;
        }
    }
}
