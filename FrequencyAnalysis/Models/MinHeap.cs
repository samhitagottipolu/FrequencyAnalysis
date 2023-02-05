using System;
namespace FrequencyAnalysis.Models
{
    public class MinHeap
    {
        public MinHeapNode[] minHeapNodes;
        public int size;
        public int capacity;
        public Dictionary<string, TokenFrequencyModel> tokenMap;

        public bool IsFullSize()
        {
            return size == capacity;
        }

        public void Insert(MinHeapNode n)
        {
            if (!IsFullSize())
            {
                minHeapNodes[size] = n;

                FixHeapAbove(size);

                size++;
            }
        }

        /// <summary>
        /// If the newly added child is smaller than the parent, swap. Rinse and repeat till heapified
        /// </summary>
        public void FixHeapAbove(int index)
        {
            MinHeapNode newNode = minHeapNodes[index];
            while (index > 0 && newNode.frequency < minHeapNodes[GetParent(index)].frequency)
            {
                MinHeapNode parentNode = minHeapNodes[GetParent(index)];
                tokenMap[parentNode.word].minHeapIndex = tokenMap[minHeapNodes[index].word].minHeapIndex;
                
                minHeapNodes[index] = parentNode;
                index = GetParent(index); //since we're fixing above tree, iterate up
            }

            tokenMap[newNode.word].minHeapIndex = index;
            minHeapNodes[index] = newNode;
        }

        public void FixHeapBelow(int index, int lastHeapIndex)
        {
            int childToSwap = index;

            while (index <= lastHeapIndex)
            {
                int leftChildIndex = GetLeftChild(index);
                int rightChildIndex = GetRightChild(index);

                if (leftChildIndex <= lastHeapIndex && minHeapNodes[index].frequency > minHeapNodes[leftChildIndex].frequency) //only has left child
                {
                    childToSwap = leftChildIndex;
                }
                else if (rightChildIndex <= lastHeapIndex && minHeapNodes[index].frequency > minHeapNodes[rightChildIndex].frequency) //both children exist
                {
                    childToSwap = rightChildIndex;
                }
                else if (leftChildIndex > lastHeapIndex && rightChildIndex > lastHeapIndex) //no children to swap with
                    break;

                if (index != childToSwap)
                {
                    //update the tokenMap's minHeapIndexes before swapping
                    tokenMap[minHeapNodes[childToSwap].word].minHeapIndex = index;
                    tokenMap[minHeapNodes[index].word].minHeapIndex = childToSwap;

                    MinHeapNode temp = minHeapNodes[index];
                    minHeapNodes[index] = minHeapNodes[childToSwap];
                    minHeapNodes[childToSwap] = temp;
                }
                else
                    break;

                index = childToSwap;
            }
        }

        //TODO
        public void Sort()
        {
           
        }

        public int Peek()
        {
            return minHeapNodes[0].frequency;
        }

        public int GetParent(int index)
        {
            return (index - 1) / 2;
        }

        public int GetLeftChild(int index)
        {
            return 2 * index + 1;
        }

        public int GetRightChild(int index)
        {
            return 2 * index + 2;
        }

        public MinHeap(int capacity, Dictionary<string, TokenFrequencyModel> tokenMap)
        {
            this.minHeapNodes = new MinHeapNode[capacity];
            this.capacity = capacity;
            this.tokenMap = tokenMap;
        }
    }
}

