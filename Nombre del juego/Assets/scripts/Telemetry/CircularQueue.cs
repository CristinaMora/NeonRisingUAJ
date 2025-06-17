using System;
using UnityEngine;
public class CircularQueue<T>
{
	private readonly T[] data;
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
		if(inUse == capacity)
		{
			Pop();
		}

		data[writePos++] = item;
		writePos %= capacity;
		inUse++;
	}

	public void Pop() 
	{
		if(inUse == 0)
		{
			throw new InvalidOperationException("Buffer is empty");
		}
		data[readPos] = default;
		readPos = (readPos + 1) % capacity;
		inUse--;
	}

	public T Front()
	{
		if (inUse == 0)
			throw new InvalidOperationException("Buffer is empty");

		return data[readPos];
	}

	public int Count => inUse;

	public bool IsEmpty => inUse == 0;

	public bool IsFull => inUse == capacity;
}
