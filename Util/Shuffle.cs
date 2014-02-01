using System;

public class Shuffle
{
    public static T[] RandomPermutation<T>(T[] array)
    {
        T[] retArray = new T[array.Length];
        array.CopyTo(retArray, 0);

        Random random = new Random();
        for (int i = 0; i < array.Length; i += 1)
        {
            int swapIndex = random.Next(i, array.Length);
            if (swapIndex != i)
            {
                T temp = retArray[i];
                retArray[i] = retArray[swapIndex];
                retArray[swapIndex] = temp;
            }
        }

        return retArray;
    }
}
