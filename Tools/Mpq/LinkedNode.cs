// <copyright file="LinkedNode.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq;

internal class LinkedNode
{
    public LinkedNode(int decompVal, int weight)
    {
        this.DecompressedValue = decompVal;
        this.Weight = weight;
    }

    public int DecompressedValue { get; }

    public int Weight { get; set; }

    public LinkedNode? Child0 { get; set; }

    public LinkedNode? Parent { get; set; }

    public LinkedNode? Next { get; set; }

    public LinkedNode? Prev { get; set; }

    public LinkedNode? Child1 => this.Child0?.Prev;

    // TODO: This would be more efficient as a member of the other class
    // ie avoid the recursion
    public LinkedNode Insert(LinkedNode other)
    {
        // 'Next' should have a lower weight
        // we should return the lower weight
        if (other.Weight <= this.Weight)
        {
            // insert before
            if (this.Next is not null)
            {
                this.Next.Prev = other;
                other.Next = this.Next;
            }

            this.Next = other;
            other.Prev = this;
            return other;
        }
        else
        {
            if (this.Prev is null)
            {
                // Insert after
                other.Prev = null;
                this.Prev = other;
                other.Next = this;
            }
            else
            {
                this.Prev.Insert(other);
            }
        }

        return this;
    }
}
