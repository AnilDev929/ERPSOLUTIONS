using System.Text.RegularExpressions;

namespace SchoolERP.Common
{
    public static class AadhaarValidator
    {
        private static readonly int[,] d =
        {
        {0,1,2,3,4,5,6,7,8,9},
        {1,2,3,4,0,6,7,8,9,5},
        {2,3,4,0,1,7,8,9,5,6},
        {3,4,0,1,2,8,9,5,6,7},
        {4,0,1,2,3,9,5,6,7,8},
        {5,9,8,7,6,0,4,3,2,1},
        {6,5,9,8,7,1,0,4,3,2},
        {7,6,5,9,8,2,1,0,4,3},
        {8,7,6,5,9,3,2,1,0,4},
        {9,8,7,6,5,4,3,2,1,0}
    };

        private static readonly int[,] p =
        {
        {0,1,2,3,4,5,6,7,8,9},
        {1,5,7,6,2,8,3,0,9,4},
        {5,8,0,3,7,9,6,1,4,2},
        {8,9,1,6,0,4,3,5,2,7},
        {9,4,5,3,1,2,6,8,7,0},
        {4,2,8,6,5,7,3,9,0,1},
        {2,7,9,3,8,0,6,4,1,5},
        {7,0,4,6,9,1,3,2,5,8}
    };

        public static bool IsValid(string aadhaar)
        {
            if (string.IsNullOrWhiteSpace(aadhaar))
                return false;

            aadhaar = aadhaar.Trim();

            if (!Regex.IsMatch(aadhaar, @"^\d{12}$"))
                return false;

            int c = 0;

            var reversed =
                aadhaar.Reverse()
                       .Select(x => int.Parse(x.ToString()))
                       .ToArray();

            for (int i = 0; i < reversed.Length; i++)
            {
                c = d[c, p[i % 8, reversed[i]]];
            }

            return c == 0;
        }
    }
}
