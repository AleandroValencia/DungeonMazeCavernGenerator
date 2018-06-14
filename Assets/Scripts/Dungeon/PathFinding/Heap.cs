using System;

// quick heap class writen for using in pathfinding algorithm for dungeon generator
public class Heap<T> where T : IHeapItem<T>
{
	
	T[] items;
	int CurrItemCount;
    public int Count { get { return CurrItemCount; } }
    public Heap(int _MaxHeapSize)
    {
		items = new T[_MaxHeapSize];
	}
	
	public void Add(T _Item)
    {
        _Item.HeapIndex = CurrItemCount;
		items[CurrItemCount] = _Item;
		SortUp(_Item);
        CurrItemCount++;
	}

	public T RemoveFirst()
    {
		T firstItem = items[0];
        CurrItemCount--;
		items[0] = items[CurrItemCount];
		items[0].HeapIndex = 0;
		SortDown(items[0]);
		return firstItem;
	}

	public void UpdateItem(T _Item)
    {
		SortUp(_Item);
	}

	public bool Contains(T _Item)
    {
		return Equals(items[_Item.HeapIndex], _Item);
	}

	void SortDown(T _Item)
    {
		while (true)
        {
			int childIndexLeft = _Item.HeapIndex * 2 + 1;
			int childIndexRight = _Item.HeapIndex * 2 + 2;
			int swapIndex = 0;

			if (childIndexLeft < CurrItemCount)
            {
				swapIndex = childIndexLeft;

				if (childIndexRight < CurrItemCount)
                {
					if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
						swapIndex = childIndexRight;
					}
				}

				if (_Item.CompareTo(items[swapIndex]) < 0)
                {
					Swap (_Item, items[swapIndex]);
				}
				else
                {
					return;
				}

			}
			else
            {
				return;
			}

		}
	}
	
	void SortUp(T _Item)
    {
		int parentIndex = (_Item.HeapIndex-1)/2;
		
		while (true)
        {
			T parentItem = items[parentIndex];
			if (_Item.CompareTo(parentItem) > 0)
            {
				Swap (_Item, parentItem);
			}
			else
            {
				break;
			}

			parentIndex = (_Item.HeapIndex-1)/2;
		}
	}
	
	void Swap(T _ItemA, T _ItemB)
    {
		items[_ItemA.HeapIndex] = _ItemB;
		items[_ItemB.HeapIndex] = _ItemA;
		int itemAIndex = _ItemA.HeapIndex;
        _ItemA.HeapIndex = _ItemB.HeapIndex;
        _ItemB.HeapIndex = itemAIndex;
	}
	
	
	
}

public interface IHeapItem<T> : IComparable<T>
{
	int HeapIndex{get;set;}
}
