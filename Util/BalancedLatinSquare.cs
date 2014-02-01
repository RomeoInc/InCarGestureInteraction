using System;
using System.Collections;

/// <summary>
/// See: http://rintintin.colorado.edu/~chathach/balancedlatinsquares.html
/// </summary>
public class BalancedLatinSquare
{
    public static int[,] GetLatinSquare(int n)
    {
        // 1. Create table
        int[,] latinSquare = new int[n, n];

        // 2. Init first row
        latinSquare[0, 0] = 1;
        latinSquare[0, 1] = 2;

        for (int i = 2, j = 3, k = 0; i < n; i++)
        {
            if (i % 2 == 1)
                latinSquare[0, i] = j++;
            else
                latinSquare[0, i] = n - (k++);
        }

        // 3. Init first column
        for (int i = 1; i <= n; i++)
        {
            latinSquare[i - 1, 0] = i;
        }

        // 4. Complete table
        for (int row = 1; row < n; row++)
        {
            for (int col = 1; col < n; col++)
            {
                latinSquare[row, col] = (latinSquare[row - 1, col] + 1) % n;

                if (latinSquare[row, col] == 0)
                    latinSquare[row, col] = n;
            }
        }

        return latinSquare;
    }

    public static void PrintLatinSquare(int[,] LatinSquare, int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Console.Write(LatinSquare[i, j].ToString().PadLeft(3));
            }
            Console.WriteLine();
        }
    }
}