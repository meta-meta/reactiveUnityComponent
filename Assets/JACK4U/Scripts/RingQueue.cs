//http://entitycrisis.blogspot.de/2010/07/generic-ring-buffer-in-c.html
using System;
using System.Collections.Generic;

public class RingQueue<T>
{
	private int size;
	private int read = 0;
	private int write = 0;
	private int count = 0;
	private T[] objects;
	
	public RingQueue (int size)
	{
		this.size = size;
		objects = new T[size + 1];
	}
	
	public bool Empty {
		get { return (read == write) && (count == 0); }
	}
	
	public bool Full {
		get { return (read == write) && (count > 0); }
	}
	
	public void Write (T item)
	{
		if (Full){
			throw new IndexOutOfRangeException ("Queue Full!");
		}
		objects[write] = item;
		count++;
		write = (write + 1) % size;
	}
	
	public T Read ()
	{
		if (Empty)
			throw new IndexOutOfRangeException ("Queue Empty!");
		T item = objects[read];
		count--;
		read = (read + 1) % size;
		return item;
	}
	public void Clear(){
		count= 0;
		read = 0;
		write = 0;
		objects = new T[size + 1];
	}
}
