using System.Collections.Generic;

public static class LinkedListExtension   
{
    public static void AppendRange<T>(this LinkedList<T> source, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            source.AddLast(item);
        }
    }

    public static void PrependRange<T>(this LinkedList<T> source, IEnumerable<T> items)
    {
        LinkedListNode<T> first = source.First;
        if (first is null)
        {
            AppendRange(source, items);
            return;
        }

        foreach (T item in items)
        {
            source.AddBefore(first, item);
        }
    }
}