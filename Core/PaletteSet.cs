using System;
using System.Drawing;
using System.IO;
using System.Text;
using MinishMaker.Utilities;

namespace MinishMaker.Core
{
    public class PaletteSet
    {
        public Color[] Palettes;
        private byte pstart = 0;
        private byte pamount = 0;
        public int pnum = 0;
        public PaletteSet(int pnum, string areaPath)
        {
            this.pnum = pnum;
            this.Palettes = LoadPalettes(areaPath);
        }

        public Color[] LoadPalettes(string areaPath)
        {
            Color[] palettes = new Color[16 * 16];

            var r = ROM.Instance.reader;

            var header = ROM.Instance.headers;
            int paletteSetTableLoc = header.paletteSetTableLoc;
            int tileOffset = header.tileOffset;
            int addr = r.ReadAddr(paletteSetTableLoc + (pnum * 4)); //4 byte entries
            int palAddr = tileOffset + (r.ReadUInt16(addr) << 5);
            pstart = r.ReadByte();
            pamount = r.ReadByte();

            byte[] pdata;
            if (File.Exists(areaPath + "/" + DataType.palette + "Dat.bin"))
            {
                pdata = File.ReadAllBytes(areaPath + "/" + DataType.palette + "Dat.bin");
            }
            else
            {
                pdata = r.ReadBytes(pamount * 0x20, palAddr);
            }

            int pos = 0; //position in pdata array
                         //manual 0th entry as I dont know where it gets it from yet

            palettes[0] = Color.Transparent;
            palettes[1] = Color.FromArgb(14 * 8, 3 * 8, 2 * 8);
            palettes[2] = Color.FromArgb(20 * 8, 5 * 8, 3 * 8);
            palettes[3] = Color.FromArgb(26 * 8, 8 * 8, 4 * 8);
            palettes[4] = Color.FromArgb(31 * 8, 10 * 8, 2 * 8);
            palettes[5] = Color.FromArgb(31 * 8, 23 * 8, 9 * 8);
            palettes[6] = Color.FromArgb(31 * 8, 17 * 8, 2 * 8);
            palettes[7] = Color.FromArgb(31 * 8, 23 * 8, 5 * 8);
            palettes[8] = Color.FromArgb(31 * 8, 28 * 8, 7 * 8);
            palettes[9] = Color.FromArgb(31 * 8, 31 * 8, 12 * 8);
            palettes[10] = Color.FromArgb(14 * 8, 12 * 8, 12 * 8);
            palettes[11] = Color.FromArgb(19 * 8, 17 * 8, 17 * 8);
            palettes[12] = Color.FromArgb(24 * 8, 22 * 8, 22 * 8);
            palettes[13] = Color.FromArgb(29 * 8, 27 * 8, 27 * 8);
            palettes[14] = Color.FromArgb(31 * 8, 31 * 8, 31 * 8);
            palettes[15] = Color.FromArgb(0, 0, 0);

            if (pstart >= 2)
            {
                palettes[16] = Color.Transparent;
                palettes[17] = Color.FromArgb(3 * 8, 6 * 8, 18 * 8);
                palettes[18] = Color.FromArgb(4 * 8, 13 * 8, 25 * 8);
                palettes[19] = Color.FromArgb(5 * 8, 20 * 8, 30 * 8);
                palettes[20] = Color.FromArgb(9 * 8, 26 * 8, 31 * 8);
                palettes[21] = Color.FromArgb(21 * 8, 29 * 8, 31 * 8);
                palettes[22] = Color.FromArgb(13 * 8, 9 * 8, 5 * 8);
                palettes[23] = Color.FromArgb(21 * 8, 12 * 8, 4 * 8);
                palettes[24] = Color.FromArgb(28 * 8, 18 * 8, 5 * 8);
                palettes[25] = Color.FromArgb(31 * 8, 26 * 8, 4 * 8);
                palettes[26] = Color.FromArgb(10 * 8, 12 * 8, 13 * 8);
                palettes[27] = Color.FromArgb(14 * 8, 17 * 8, 18 * 8);
                palettes[28] = Color.FromArgb(20 * 8, 22 * 8, 24 * 8);
                palettes[29] = Color.FromArgb(26 * 8, 27 * 8, 29 * 8);
                palettes[30] = Color.FromArgb(31 * 8, 31 * 8, 31 * 8);
                palettes[31] = Color.FromArgb(0, 0, 0);
            }

            for (int p = pstart; p < (pstart + pamount); p++)
            {
                if (p >= 16)
                {
                    throw new PaletteException("The room attempted to load more than 16 palettes, the room is highly likely invalid.");
                }
                for (int i = 0; i < 0x10; i++)
                {
                    ushort pd = (ushort)(pdata[pos] | (pdata[pos + 1] << 8));
                    pos += 2;
                    int red = (pd & 0x1F) << 3;
                    int green = ((pd >> 5) & 0x1F) << 3;
                    int blue = ((pd >> 10) & 0x1F) << 3;
                    palettes[p * 16 + i] = Color.FromArgb(red, green, blue);
                }
            }
            return palettes;
        }

        public string ToPaletteString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("CLRX 8 256");
            var i = 0;
            foreach (var color in this.Palettes)
            {
                sb.Append("0x00" + color.B.Hex().PadLeft(2, '0') + color.G.Hex().PadLeft(2, '0') + color.R.Hex().PadLeft(2, '0') + " ");
                i++;
                if (i == 4)
                {
                    sb.AppendLine();
                    i = 0;
                }
            }
            return sb.ToString();
        }

        public long GetSaveData(ref byte[] data)
        {
            data = new byte[pamount * 0x20];
            for (int i = 0; i < pamount * 0x10; i++)
            {
                var curColor = Palettes[i + (pstart * 16)];
                int rVal = (curColor.R >> 3);
                int gVal = (curColor.G >> 3) << 5;
                int bVal = (curColor.B >> 3) << 10;
                int value = rVal + gVal + bVal;
                data[i * 2] = (byte)(value & 0xFF);
                data[i * 2 + 1] = (byte)(value >> 8);
            }
            return pamount * 0x20;
        }
    }


    public class PaletteException : Exception
    {
        public PaletteException() { }
        public PaletteException(string message) : base(message) { }
        public PaletteException(string message, Exception inner) : base(message, inner) { }
    }
}
