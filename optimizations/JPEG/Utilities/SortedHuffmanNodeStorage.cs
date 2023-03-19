using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JPEG.Utilities;

public class SortedHuffmanNodeStorage
{
    Element Head = null;
    public int Count { get; private set; }

    public SortedHuffmanNodeStorage(int[] friquencies)
    {
        for (int i = 0; i < friquencies.Length; i++)
        {
            Add(new HuffmanNode() {Frequency = friquencies[i], LeafLabel = (byte) i});
        }
    }
    public SortedHuffmanNodeStorage(IEnumerable<HuffmanNode> nods)
    {
        foreach (var var in nods)
        {
            Add(var);
        }
    }

    public void Add(HuffmanNode value)
    {
        Element el = new Element(value);
        
        if (Head == null)
        {
            Head = el;
            Count++;
            return;
        }

        Element pointer1 = Head;
        Element pointer2 = Head;
        while (pointer1 != null && pointer1.value.Frequency < value.Frequency)
        {
            pointer2 = pointer1;
            pointer1 = pointer1.Next;
        }

        if (pointer1 == null)
        {
            pointer2.Next = el;
            el.Prev = pointer2;
        }
        else if(pointer1.Prev==null)
        {
            Head = el;
            el.Next = pointer1;
            pointer1.Prev = el;
        }
        else
        {
            el.Prev = pointer1.Prev;
            el.Next = pointer1;
            el.Prev.Next = el;
            el.Next.Prev = el;
        }
        Count++;
    }

    public HuffmanNode GetHead()
    {
        var el = Head;
        Head = Head.Next;
        if (Head != null) Head.Prev = null;
        Count--;
        return el.value;
    }

    class Element
    {
        public HuffmanNode value;
        public Element Next = null;
        public Element Prev = null;

        public Element(HuffmanNode value)
        {
            this.value = value;
        }
    }
}