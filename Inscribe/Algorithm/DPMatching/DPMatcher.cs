using System;
using System.Linq;

namespace Inscribe.Algorithm.DPMatching
{
    /// <summary>
    /// Matching with Dynamic Programming method for String.
    /// </summary>
    public static class DPMatcher
    {
        /// <summary>
        /// Run DPMatching and get errors between test and reference.
        /// </summary>
        /// <param name="test">test string</param>
        /// <param name="reference">reference string</param>
        /// <returns></returns>
        public static MatchingResult Matching(String test, String reference)
        {
            int[,] traceTable;
            double dpv = DPMatchingCore(test, reference, out traceTable);
            return Trace(traceTable, dpv);
        }

        public static double DPMatchingCore(String test, String reference, out int[,] traceTable)
        {
            traceTable = InitTraceTable(reference.Length, test.Length);
            for (int ri = 0; ri < reference.Length; ri++)
            {
                for (int ti = 0; ti < test.Length; ti++)
                {
                    int err = reference[ri] != test[ti] ? 1 : 0;
                    int min = traceTable[ri, ti] + err;
                    if (traceTable[ri + 1, ti] < min)
                        min = traceTable[ri + 1, ti];
                    if (traceTable[ri, ti + 1] < min)
                        min = traceTable[ri, ti + 1];
                    traceTable[ri + 1, ti + 1] = err + min;
                }
            }
            return traceTable[reference.Length, test.Length] / (double)(reference.Length + test.Length);
        }

        public static MatchingResult Trace(int[,] traceTable, double distance)
        {
            MatchingResult r = new MatchingResult();
            r.Distance = distance;
            int tf = traceTable.GetLength(1);
            int rf = traceTable.GetLength(0);
            int ri = rf - 1;
            int ti = tf - 1;
            while (ri > 0 && ti > 0)
            {
                int min = new[] { traceTable[ri - 1, ti], traceTable[ri, ti - 1], traceTable[ri - 1, ti - 1] }
                    .Min();
                if (traceTable[ri - 1, ti - 1] == min)
                {
                    ri--; ti--;
                }
                else if (traceTable[ri, ti - 1] == min)
                {
                    ti--;
                    r.Insertions++;
                }
                else
                {
                    ri--;
                    r.Deletions++;
                }
            }
            return r;
        }

        private static int[,] InitTraceTable(int rlen, int tlen)
        {
            var traceTable = new int[rlen + 1, tlen + 1];
            Enumerable.Range(1, rlen).ForEach(i => traceTable[i, 0] = i);
            Enumerable.Range(1, tlen).ForEach(i => traceTable[0, i] = i);
            traceTable[0, 0] = 0;
            return traceTable;
        }
    }

    public class MatchingResult
    {
        public double Distance { get; set; }

        public int Insertions { get; set; }

        public int Deletions { get; set; }

        public override string ToString()
        {
            return "Distance: " + Distance + ", Insertions: " + Insertions + ", Deletions: " + Deletions;
        }
    }
}
