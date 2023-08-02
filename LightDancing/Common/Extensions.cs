using System.Collections.Generic;
using System.Linq;

namespace LightDancing.Common
{
    public static class Extensions
    {
        public static void PadListWithEmptyStringsToMultipleOfNumber(this List<byte> inputList, int number)
        {
            int remainder = inputList.Count % number;

            if (remainder != 0)
            {
                int numberOfEmptyStringsToAdd = number - remainder;
                inputList.PadListWithZeros(inputList.Count + numberOfEmptyStringsToAdd);
            }
        }

        public static void PadListWithZeros(this List<byte> inputList, int targetSize)
        {
            int numberOfZeros = targetSize - inputList.Count;

            if (numberOfZeros > 0)
            {
                inputList.AddRange(Enumerable.Repeat((byte)0, numberOfZeros));
            }
        }
    }
}
