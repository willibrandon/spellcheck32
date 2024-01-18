using System.Text;
using System.Threading.Algorithms;

namespace spellcheck32.tests;

public static class CommonTestHelper
{
    public static string GenerateRandomString(Random rand)
    {
        const int LEN = 10_000;
        StringBuilder sb = new(LEN);
        for (int i = 0; i < LEN; i++) sb.Append((char)('a' + rand.Next(0, 26)));
        return sb.ToString();
    }

    public static int SerialEditDistance(string s1, string s2)
    {
        int[,] dist = new int[s1.Length + 1, s2.Length + 1];
        for (int i = 0; i <= s1.Length; i++) dist[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++) dist[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                dist[i, j] = (s1[i - 1] == s2[j - 1]) ?
                    dist[i - 1, j - 1] :
                    1 + Math.Min(dist[i - 1, j],
                        Math.Min(dist[i, j - 1],
                                 dist[i - 1, j - 1]));
            }
        }

        return dist[s1.Length, s2.Length];
    }

    public static int ParallelEditDistance(string s1, string s2)
    {
        int[,] dist = new int[s1.Length + 1, s2.Length + 1];
        for (int i = 0; i <= s1.Length; i++) dist[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++) dist[0, j] = j;
        int numBlocks = Environment.ProcessorCount * 4;

        ParallelAlgorithms.Wavefront(
            s1.Length, s2.Length,
            numBlocks, numBlocks,
            (start_i, end_i, start_j, end_j) =>
            {
                for (int i = start_i + 1; i <= end_i; i++)
                {
                    for (int j = start_j + 1; j <= end_j; j++)
                    {
                        dist[i, j] = (s1[i - 1] == s2[j - 1]) ?
                            dist[i - 1, j - 1] :
                            1 + Math.Min(dist[i - 1, j],
                                Math.Min(dist[i, j - 1],
                                         dist[i - 1, j - 1]));
                    }
                }
            });

        return dist[s1.Length, s2.Length];
    }
}
