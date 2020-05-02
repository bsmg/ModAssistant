/*
Copyright (c) 2013 Max Hauser

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Text;

namespace ModAssistant.Libs
{
    internal static class IntExtensions
    {
        /// <summary>
        /// The number of digits in a non-negative number. Returns 1 for all
        /// negative numbers. That is ok because we are using it to calculate
        /// string length for a <see cref="StringBuilder"/> for numbers that
        /// aren't supposed to be negative, but when they are it is just a little
        /// slower.
        /// </summary>
        /// <remarks>
        /// This approach is based on https://stackoverflow.com/a/51099524/268898
        /// where the poster offers performance benchmarks showing this is the
        /// fastest way to get a number of digits.
        /// </remarks>
        public static int Digits(this int n)
        {
            if (n < 10) return 1;
            if (n < 100) return 2;
            if (n < 1_000) return 3;
            if (n < 10_000) return 4;
            if (n < 100_000) return 5;
            if (n < 1_000_000) return 6;
            if (n < 10_000_000) return 7;
            if (n < 100_000_000) return 8;
            if (n < 1_000_000_000) return 9;
            return 10;
        }
    }
}
