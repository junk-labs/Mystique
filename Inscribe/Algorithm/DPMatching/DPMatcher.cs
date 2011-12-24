using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
        /// <param name="test">test sample</param>
        /// <param name="reference">reference sample</param>
        /// <returns></returns>
        public static MatchingResult Matching(String test, String reference)
        {
            int[,] traceTable;
            double dpv = DPMatchingCore(test, reference, out traceTable);
            return Trace(traceTable, dpv);
        }


        /// <summary>
        /// Run DPMatching, get distance.
        /// </summary>
        /// <param name="test">test sample</param>
        /// <param name="reference">reference sample</param>
        /// <param name="traceTable">result trace table</param>
        /// <returns>distance between two samples.</returns>
        public static double DPMatchingCore(String test, String reference)
        {
            int[,] _;
            return DPMatchingCore(test, reference, out _);
        }

        /// <summary>
        /// Run DPMatching, get distance and trace table.
        /// </summary>
        /// <param name="test">test sample</param>
        /// <param name="reference">reference sample</param>
        /// <param name="traceTable">result trace table</param>
        /// <returns>distance between two samples.</returns>
        public static double DPMatchingCore(String test, String reference, out int[,] traceTable)
        {
            int rl = reference.Length;
            int tl = test.Length;
            traceTable = InitTraceTable(rl, tl);
            for (int ri = 0; ri < rl; ri++)
            {
                for (int ti = 0; ti < tl; ti++)
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
            return traceTable[rl, tl] / (double)(rl + tl);
        }

        /// <summary>
        /// Analyze trace table.
        /// </summary>
        /// <param name="traceTable">tracing target</param>
        /// <param name="distance">distance of samples</param>
        /// <returns>MatchingResult data.</returns>
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

        /// <summary>
        /// Initializing trace table.
        /// </summary>
        /// <param name="rlen">reference sample length</param>
        /// <param name="tlen">trace sample length</param>
        /// <returns>trace table</returns>
        private static int[,] InitTraceTable(int rlen, int tlen)
        {
            var traceTable = new int[rlen + 1, tlen + 1];
            Enumerable.Range(1, rlen).ForEach(i => traceTable[i, 0] = i);
            Enumerable.Range(1, tlen).ForEach(i => traceTable[0, i] = i);
            traceTable[0, 0] = 0;
            return traceTable;
        }
    }

    /// <summary>
    /// Result data of DPMatching
    /// </summary>
    public class MatchingResult
    {
        /// <summary>
        /// Distance two samples
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Insertion error
        /// </summary>
        public int Insertions { get; set; }

        /// <summary>
        /// Deletion error
        /// </summary>
        public int Deletions { get; set; }

        public override string ToString()
        {
            return "Distance: " + Distance + ", Insertions: " + Insertions + ", Deletions: " + Deletions;
        }
    }
}
