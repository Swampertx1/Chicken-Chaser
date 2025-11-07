using System;

namespace GabesCommonUtility.Extensions
{
    public static class MathExtensions
    {
        /// <summary>
        /// Given a float, returns if it's positive 1, 0 or negative 1 as an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int NormalizeToInt(this float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value); // get raw binary representation
            if ((bits >> 31) != 0) return -1;
            if ((bits & 0x7FFFFFFF) == 0) return 0; // mask out sign bit
            return 1;
        }
        
        /// <summary>
        /// Given a float, returns if it's positive 1, 0 or negative 1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Normalize(this float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value); // get raw binary representation
            if ((bits >> 31) != 0) return -1f;
            if ((bits & 0x7FFFFFFF) == 0) return 0f; // mask out sign bit
            return 1f;
        }
    }
}