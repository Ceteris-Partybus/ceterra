using System.Collections.Generic;

public class CyclicList<T> : List<T> {

    protected int listIterator = 0;
    protected int listLength;

    public int ListLength {
        get; set;
    }

    public void IterateLatestValues(T value) {
        if (this.listIterator < this.ListLength) {
            this.Add(value);
        }
        else {
            this[this.listIterator % this.ListLength] = value;
        }
        this.listIterator++;
    }
}