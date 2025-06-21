using System;

public class CircularQueue<T>
{
    private T[] data;
    private int readPos;
    private int writePos;
    private int inUse;

    private readonly int capacity;
    public CircularQueue(int size)
    {
        capacity = size;
        data = new T[capacity];
        readPos = 0;
        writePos = 0;
        inUse = 0;
    }

    public void Push(T item)
    {
        if (inUse == capacity)
        {
            Pop();
        }

        data[writePos++] = item;
        writePos %= capacity;
        inUse++;
    }

    public T Pop()
    {
        if (inUse == 0)
        {
            throw new InvalidOperationException("Buffer is empty");
        }
        T item = data[readPos++];
        data[readPos] = default;
        readPos = (readPos + 1) % capacity;
        inUse--;
        return item;
    }

    public T Front()
    {
        if (inUse == 0)
            throw new InvalidOperationException("Buffer is empty");

        return data[readPos];
    }

    public bool TryDequeue(out T item)
    {
        if (inUse == 0)
        {
            item = default;
            return false;
        }

        item = data[readPos];
        data[readPos] = default;
        readPos = (readPos + 1) % capacity;
        inUse--;
        return true;
    }

    public int Count => inUse;

    public bool IsEmpty => inUse == 0;

    public bool IsFull => inUse == capacity;
}
