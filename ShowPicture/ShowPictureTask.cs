using System;
using System.Collections.Generic;

namespace ShowPicture
{
    public static class ShowPictureTask
    {
        private const int ScreenWidth = 512;
        private const int ScreenHeight = 256;
        private const int ScreenBaseAddress = 0x4000;
        private const int WordsPerScreenRow = 32;
        private const int PixelsPerWord = 16;
        public static string[] GenerateShowPictureCode(bool[,] pixels)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));

            var height = pixels.GetLength(0);
            var width = pixels.GetLength(1);
            if (height > ScreenHeight || width > ScreenWidth)
                throw new ArgumentException("Image must fit into the Hack screen (512x256).", nameof(pixels));

            var code = new List<string>();
            short? currentD = null;
            int? currentAddress = null;
            var wordsPerImageRow = (width + PixelsPerWord - 1) / PixelsPerWord;

            for (var y = 0; y < height; y++)
            {
                var rowBaseAddress = ScreenBaseAddress + y * WordsPerScreenRow;
                for (var wordIndex = 0; wordIndex < wordsPerImageRow; wordIndex++)
                {
                    var wordValue = BuildScreenWord(pixels, y, wordIndex, width);
                    if (wordValue == 0)
                        continue;

                    EmitWordWrite(code, rowBaseAddress + wordIndex, wordValue, ref currentD, ref currentAddress);
                }
            }

            code.Add("(END)");
            code.Add("@END");
            code.Add("0;JMP");
            return code.ToArray();
        }

        private static short BuildScreenWord(bool[,] pixels, int y, int wordIndex, int width)
        {
            var startX = wordIndex * PixelsPerWord;
            var endX = Math.Min(startX + PixelsPerWord, width);
            var word = 0;

            for (var x = startX; x < endX; x++)
            {
                if (pixels[y, x])
                    word |= 1 << (x - startX);
            }

            return unchecked((short)word);
        }

        private static void EmitWordWrite(
            List<string> code,
            int address,
            short value,
            ref short? currentD,
            ref int? currentAddress)
        {
            if (currentD == value)
            {
                EmitAddress(code, address, ref currentAddress);
                code.Add("M=D");
                return;
            }

            if (value == -1)
            {
                EmitAddress(code, address, ref currentAddress);
                code.Add("M=-1");
                return;
            }

            if (value == 1)
            {
                EmitAddress(code, address, ref currentAddress);
                code.Add("M=1");
                return;
            }

            LoadValueIntoD(code, value);
            currentD = value;
            currentAddress = null;

            EmitAddress(code, address, ref currentAddress);
            code.Add("M=D");
        }

        private static void EmitAddress(List<string> code, int address, ref int? currentAddress)
        {
            if (currentAddress.HasValue && currentAddress.Value + 1 == address)
                code.Add("A=A+1");
            else
                code.Add($"@{address}");

            currentAddress = address;
        }

        private static void LoadValueIntoD(List<string> code, short value)
        {
            var unsignedValue = unchecked((ushort)value);
            if (unsignedValue <= short.MaxValue)
            {
                code.Add($"@{unsignedValue}");
                code.Add("D=A");
                return;
            }

            var invertedValue = (~unsignedValue) & short.MaxValue;
            code.Add($"@{invertedValue}");
            code.Add("D=!A");
        }
    }
}
