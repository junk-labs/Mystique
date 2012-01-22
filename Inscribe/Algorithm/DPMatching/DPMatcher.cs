using System;
using System.Collections.Generic;
using System.Linq;

namespace Inscribe.Algorithm.DPMatching
{
    /// <summary>
    /// Matching with Dynamic Programming method for string.
    /// </summary>
    public static class DPMatcher
    {
        /// <summary>
        /// Run DPMatching and get errors between test and reference.
        /// </summary>
        /// <param name="test">test sample</param>
        /// <param name="reference">reference sample</param>
        /// <returns></returns>
        public static MatchingResult Matching(string test, string reference)
        {
            int[,] traceTable;
            double dpv = DPMatchingCore(test, reference, out traceTable);
            System.Diagnostics.Debug.WriteLine(Enumerable.Range(0, traceTable.GetLength(0))
                .Select(i => Enumerable.Range(0, traceTable.GetLength(1)).Select(j => traceTable[i, j].ToString()).JoinString(" "))
                .JoinString(Environment.NewLine));
            return Trace(traceTable, dpv);
        }

        /// <summary>
        /// Run DPMatching, get distance.
        /// </summary>
        /// <param name="test">test sample</param>
        /// <param name="reference">reference sample</param>
        /// <param name="traceTable">result trace table</param>
        /// <returns>distance between two samples.</returns>
        public static double DPMatchingCore(string test, string reference)
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
        public static double DPMatchingCore(string test, string reference, out int[,] traceTable)
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
            List<int> insertions = new List<int>();
            List<int> deletions = new List<int>();
            List<Tuple<int, int>> substitutions = new List<Tuple<int, int>>();
            while (ri > 0 && ti > 0)
            {
                int min = new[] { traceTable[ri - 1, ti], traceTable[ri, ti - 1], traceTable[ri - 1, ti - 1] }
                    .Min();
                if (traceTable[ri - 1, ti - 1] == min)
                {
                    if (traceTable[ri, ti] != traceTable[ri - 1, ti - 1])
                        substitutions.Add(new Tuple<int, int>(ti - 1, ri - 1));
                    ri--; ti--;
                }
                else if (traceTable[ri, ti - 1] == min)
                {
                    insertions.Add(ti - 1);
                    ti--;
                }
                else
                {
                    deletions.Add(ri - 1);
                    ri--;
                }
            }
            if (ri != 0)
            {
                // reference index is remain.
                // -> Deletion error
                Enumerable.Range(0, ri)
                    .ForEach(deletions.Add);
            }
            if (ti != 0)
            {
                // test index is remain.
                // -> Insertion error
                Enumerable.Range(0, ti)
                    .ForEach(insertions.Add);
            }
            r.InsertionIndexes = insertions.OrderBy(i => i).ToArray();
            r.DeletionIndexes = deletions.OrderBy(i => i).ToArray();
            r.SubstitutionIndexes = substitutions.OrderBy(i => i.Item1).ToArray();
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
        /// Indexes of insertion error occured
        /// </summary>
        public int[] InsertionIndexes { get; set; }

        /// <summary>
        /// Indexes of deletion error occured
        /// </summary>
        public int[] DeletionIndexes { get; set; }

        /// <summary>
        /// Indexes of substitution error occured<para />
        /// Item1 is index of test, Item2 is index of reference.
        /// </summary>
        public Tuple<int, int>[] SubstitutionIndexes { get; set; }

        /// <summary>
        /// Insertion error
        /// </summary>
        public int Insertions { get { return InsertionIndexes.Length; } }

        /// <summary>
        /// Deletion error
        /// </summary>
        public int Deletions { get { return DeletionIndexes.Length; } }

        /// <summary>
        /// Substitution error
        /// </summary>
        public int Substitutions { get { return SubstitutionIndexes.Length; } }

        public override string ToString()
        {
            return "Distance: " + Distance + ", Insertions: " + Insertions + ", Deletions: " + Deletions + ", Substitutions: " + Substitutions;
        }
    }
}
