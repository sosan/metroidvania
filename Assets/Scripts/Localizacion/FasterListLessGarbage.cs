using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;


public class FasterListLessGarbage<T>
{


	public T[] buffer;
	public int size = 0;

	[DebuggerHidden]
	[DebuggerStepThrough]
	public IEnumerator<T> GetEnumerator ()
	{
		if (buffer != null)
		{
			for (int i = 0; i < size; ++i)
			{
				yield return buffer[i];
			}
		}
	}
	
	[DebuggerHidden]
	public T this[int i]
	{
		get { return buffer[i]; }
		set { buffer[i] = value; }
	}

	void AllocateMore ()
	{
		T[] newList = (buffer != null) ? new T[Mathf.Max(buffer.Length << 1, 32)] : new T[32];
		if (buffer != null && size > 0) buffer.CopyTo(newList, 0);
		buffer = newList;
	}

	void Trim ()
	{
		if (size > 0)
		{
			if (size < buffer.Length)
			{
				T[] newList = new T[size];
				for (int i = 0; i < size; ++i) newList[i] = buffer[i];
				buffer = newList;
			}
		}
		else buffer = null;
	}


	public void Clear () { size = 0; }


	public void Release () { size = 0; buffer = null; }


	public void Add (T item)
	{
		if (buffer == null || size == buffer.Length) AllocateMore();
		buffer[size++] = item;
	}


	public void Insert (int index, T item)
	{
		if (buffer == null || size == buffer.Length) AllocateMore();

		if (index > -1 && index < size)
		{
			for (int i = size; i > index; --i) buffer[i] = buffer[i - 1];
			buffer[index] = item;
			++size;
		}
		else Add(item);
	}


	public bool Contains (T item)
	{
		if (buffer == null) return false;
		for (int i = 0; i < size; ++i) if (buffer[i].Equals(item)) return true;
		return false;
	}


	public int IndexOf (T item)
	{
		if (buffer == null) return -1;
		for (int i = 0; i < size; ++i) if (buffer[i].Equals(item)) return i;
		return -1;
	}


	public bool Remove (T item)
	{
		if (buffer != null)
		{
			EqualityComparer<T> comp = EqualityComparer<T>.Default;

			for (int i = 0; i < size; ++i)
			{
				if (comp.Equals(buffer[i], item))
				{
					--size;
					buffer[i] = default(T);
					for (int b = i; b < size; ++b) buffer[b] = buffer[b + 1];
					buffer[size] = default(T);
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveAt (int index)
	{
		if (buffer != null && index > -1 && index < size)
		{
			--size;
			buffer[index] = default(T);
			for (int b = index; b < size; ++b) buffer[b] = buffer[b + 1];
			buffer[size] = default(T);
		}
	}


	public T Pop ()
	{
		if (buffer != null && size != 0)
		{
			T val = buffer[--size];
			buffer[size] = default(T);
			return val;
		}
		return default(T);
	}


	public T[] ToArray () { Trim(); return buffer; }



	// mejor usar el list original
	[DebuggerHidden]
	[DebuggerStepThrough]
	public void Sort (CompareFunc comparer)
	{
		int start = 0;
		int max = size - 1;
		bool changed = true;

		while (changed)
		{
			changed = false;

			for (int i = start; i < max; ++i)
			{
				if (comparer(buffer[i], buffer[i + 1]) > 0)
				{
					// Swap the values
					T temp = buffer[i];
					buffer[i] = buffer[i + 1];
					buffer[i + 1] = temp;
					changed = true;
				}
				else if (!changed)
				{
					start = (i == 0) ? 0 : i - 1;
				}
			}
		}
	}


	public delegate int CompareFunc (T left, T right);


}
