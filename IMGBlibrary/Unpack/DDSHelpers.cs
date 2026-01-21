using IMGBlibrary.Support;
using System;
using System.Text;

namespace IMGBlibrary.Unpack
{
    internal class DDSHelpers
    {
        public static byte[] GetDDSHeader(GTEX gtex)
        {
            var ddsHeader = new byte[128];

            var ddsMagic = Encoding.ASCII.GetBytes("DDS ");
            Array.ConstrainedCopy(ddsMagic, 0, ddsHeader, 0, 4);

            var dwSize = BitConverter.GetBytes((uint)124);
            Array.ConstrainedCopy(dwSize, 0, ddsHeader, 4, 4);

            var dwFlags = GetdwFlags(gtex);
            Array.ConstrainedCopy(dwFlags, 0, ddsHeader, 8, 4);

            var dwHeight = BitConverter.GetBytes((uint)gtex.Height);
            Array.ConstrainedCopy(dwHeight, 0, ddsHeader, 12, 4);

            var dwWidth = BitConverter.GetBytes((uint)gtex.Width);
            Array.ConstrainedCopy(dwWidth, 0, ddsHeader, 16, 4);

            var dwPitchOrLinearSize = GetdwPitchOrLinearSize(gtex);
            Array.ConstrainedCopy(dwPitchOrLinearSize, 0, ddsHeader, 20, 4);

            uint dwDepthVal = (uint)(gtex.Type == 2 ? gtex.Depth : 0);
            var dwDepth = BitConverter.GetBytes(dwDepthVal);
            Array.ConstrainedCopy(dwDepth, 0, ddsHeader, 24, 4);

            var dwMipMapCount = BitConverter.GetBytes((uint)gtex.MipCount);
            Array.ConstrainedCopy(dwMipMapCount, 0, ddsHeader, 28, 4);

            var dwReserved1 = new byte[44];
            Array.ConstrainedCopy(dwReserved1, 0, ddsHeader, 32, 44);

            var ddspf = Getddspf(gtex);
            Array.ConstrainedCopy(ddspf, 0, ddsHeader, 76, 32);

            var dwCaps = GetdwCaps(gtex);
            Array.ConstrainedCopy(dwCaps, 0, ddsHeader, 108, 4);

            var dwCaps2 = GetdwCaps2(gtex);
            Array.ConstrainedCopy(dwCaps2, 0, ddsHeader, 112, 4);

            var dwCaps3And4 = new byte[8];
            Array.ConstrainedCopy(dwCaps3And4, 0, ddsHeader, 116, 8);

            var dwReserved2 = new byte[4];
            Array.ConstrainedCopy(dwReserved2, 0, ddsHeader, 124, 4);

            return ddsHeader;
        }

        private static byte[] GetdwFlags(GTEX gtex)
        {
            uint ddsCapsVal1 = 0x1;  // DDSD_CAPS
            uint ddsCapsVal2 = 0x2;  // DDSD_HEIGHT
            uint ddsCapsVal3 = 0x4;  // DDSD_WIDTH
            uint ddsCapsVal4 = (uint)(gtex.Format == 3 || gtex.Format == 4 ? 0x8 : 0);  // DDSD_PITCH
            uint ddsCapsVal5 = 0x1000;  // DDSD_PIXELFORMAT
            uint ddsCapsVal6 = (uint)(gtex.MipCount > 1 ? 0x20000 : 0); // DDSD_MIPMAPCOUNT
            uint ddsCapsVal7 = (uint)(gtex.Format == 24 || gtex.Format == 25 || gtex.Format == 26 ? 0x80000 : 0);    // DDSD_LINEARSIZE
            uint ddsCapsVal8 = (uint)(gtex.Type == 2 ? 0x800000 : 0);   // DDSD_DEPTH
            uint dwFlagsVal = ddsCapsVal1 + ddsCapsVal2 + ddsCapsVal3 + ddsCapsVal4 + ddsCapsVal5 + ddsCapsVal6 + ddsCapsVal7 + ddsCapsVal8;

            return BitConverter.GetBytes(dwFlagsVal);
        }

        private static byte[] GetdwPitchOrLinearSize(GTEX gtex)
        {
            uint wPitchOrLinearSizeVal = 0;

            switch (gtex.Format)
            {
                case 3:     // R8G8B8A8 (with mips)
                case 4:     // R8G8B8A8
                    wPitchOrLinearSizeVal = ((uint)gtex.Width * 32 + 7) / 8;
                    break;

                case 24:     // DXT1
                    wPitchOrLinearSizeVal = Math.Max(1, (((uint)gtex.Width + 3) / 4)) * Math.Max(1, (((uint)gtex.Height + 3) / 4)) * 8;
                    break;

                case 25:     // DXT3
                    wPitchOrLinearSizeVal = Math.Max(1, (((uint)gtex.Width + 3) / 4)) * Math.Max(1, (((uint)gtex.Height + 3) / 4)) * 16;
                    break;

                case 26:     // DXT5             
                    wPitchOrLinearSizeVal = Math.Max(1, (((uint)gtex.Width + 3) / 4)) * Math.Max(1, (((uint)gtex.Height + 3) / 4)) * 16;
                    break;
            }

            return BitConverter.GetBytes(wPitchOrLinearSizeVal);
        }

        private static byte[] Getddspf(GTEX gtex)
        {
            var ddspf = new byte[32];

            var dwSize = BitConverter.GetBytes((uint)32);    // DDS_PIXELFORMAT -> dwSize
            Array.ConstrainedCopy(dwSize, 0, ddspf, 0, 4);

            uint flagsVal1 = (uint)(gtex.Format == 3 || gtex.Format == 4 ? 0x1 : 0);    // DDS_PIXELFORMAT -> DDPF_ALPHAPIXELS
            uint flagsVal2 = 0; // DDS_PIXELFORMAT -> DDPF_ALPHA
            uint flagsVal3 = (uint)(gtex.Format == 24 || gtex.Format == 25 || gtex.Format == 26 ? 0x4 : 0); // DDS_PIXELFORMAT -> DDPF_FOURCC
            uint flagsVal4 = (uint)(gtex.Format == 3 || gtex.Format == 4 ? 0x40 : 0);   // DDS_PIXELFORMAT -> DDPF_RGB
            uint flagsVal5 = 0; // DDS_PIXELFORMAT -> DDPF_YUV
            uint flagsVal6 = 0; // DDS_PIXELFORMAT -> DDPF_LUMINANCE
            uint flagsVal = flagsVal1 + flagsVal2 + flagsVal3 + flagsVal4 + flagsVal5 + flagsVal6;
            var dwFlags = BitConverter.GetBytes(flagsVal);  // DDS_PIXELFORMAT -> dwFlags
            Array.ConstrainedCopy(dwFlags, 0, ddspf, 4, 4);

            var dwFourCC = new byte[4]; // DDS_PIXELFORMAT -> dwFourCC

            switch (gtex.Format)
            {
                case 24:    // DXT1
                    dwFourCC = Encoding.ASCII.GetBytes("DXT1");
                    break;

                case 25:    // DXT3
                    dwFourCC = Encoding.ASCII.GetBytes("DXT3");
                    break;

                case 26:    // DXT5
                    dwFourCC = Encoding.ASCII.GetBytes("DXT5");
                    break;
            }

            Array.ConstrainedCopy(dwFourCC, 0, ddspf, 8, 4);

            uint dwRGBBitCount = (uint)(gtex.Format == 3 || gtex.Format == 4 ? 0x20 : 0); // DDS_PIXELFORMAT -> dwRGBBitCount
            Array.ConstrainedCopy(BitConverter.GetBytes(dwRGBBitCount), 0, ddspf, 12, 4);

            uint dwRBitMask = (uint)(gtex.Format == 3 || gtex.Format == 4 ? 0x00FF0000 : 0); // DDS_PIXELFORMAT -> dwRBitMask
            Array.ConstrainedCopy(BitConverter.GetBytes(dwRBitMask), 0, ddspf, 16, 4);

            uint dwGBitMask = (uint)(gtex.Format == 3 || gtex.Format == 4 ? 0x0000FF00 : 0); // DDS_PIXELFORMAT -> dwGBitMask
            Array.ConstrainedCopy(BitConverter.GetBytes(dwGBitMask), 0, ddspf, 20, 4);

            uint dwBBitMask = (uint)(gtex.Format == 3 || gtex.Format == 4 ? 0x000000FF : 0); // DDS_PIXELFORMAT -> dwBBitMask
            Array.ConstrainedCopy(BitConverter.GetBytes(dwBBitMask), 0, ddspf, 24, 4);

            uint dwABitMask = (gtex.Format == 3 || gtex.Format == 4 ? 0xFF000000 : 0); // DDS_PIXELFORMAT -> dwABitMask
            Array.ConstrainedCopy(BitConverter.GetBytes(dwABitMask), 0, ddspf, 28, 4);

            return ddspf;
        }

        private static byte[] GetdwCaps(GTEX gtex)
        {
            uint dwCapsVal1 = (uint)(gtex.MipCount > 1 || gtex.Type == 1 || gtex.Type == 2 || gtex.Type == 5 ? 0x8 : 0);    // DDSCAPS_COMPLEX
            uint dwCapsVal2 = (uint)(gtex.MipCount > 1 ? 0x400000 : 0); // DDSCAPS_MIPMAP
            uint dwCapsVal3 = 0x1000;   // DDSCAPS_TEXTURE
            uint dwCapsVal = dwCapsVal1 + dwCapsVal2 + dwCapsVal3;

            return BitConverter.GetBytes(dwCapsVal);
        }

        private static byte[] GetdwCaps2(GTEX gtex)
        {
            uint dwCaps2Val1 = (uint)(gtex.Type == 1 || gtex.Type == 5 ? 0x200 : 0);    // DDSCAPS2_CUBEMAP
            uint dwCaps2Val2 = (uint)(gtex.Type == 1 || gtex.Type == 5 ? 0x400 : 0);    // DDSCAPS2_CUBEMAP_POSITIVEX
            uint dwCaps2Val3 = (uint)(gtex.Type == 1 || gtex.Type == 5 ? 0x800 : 0);    // DDSCAPS2_CUBEMAP_NEGATIVEX
            uint dwCaps2Val4 = (uint)(gtex.Type == 1 || gtex.Type == 5 ? 0x1000 : 0);    // DDSCAPS2_CUBEMAP_POSITIVEY
            uint dwCaps2Val5 = (uint)(gtex.Type == 1 || gtex.Type == 5 ? 0x2000 : 0);    // DDSCAPS2_CUBEMAP_NEGATIVEY
            uint dwCaps2Val6 = (uint)(gtex.Type == 1 || gtex.Type == 5 ? 0x4000 : 0);    // DDSCAPS2_CUBEMAP_POSITIVEZ
            uint dwCaps2Val7 = (uint)(gtex.Type == 1 || gtex.Type == 5 ? 0x8000 : 0);    // DDSCAPS2_CUBEMAP_NEGATIVEZ
            uint dwCaps2Val8 = (uint)(gtex.Type == 2 ? 0x200000 : 0);   // DDSCAPS2_VOLUME
            uint dwCaps2Val = dwCaps2Val1 + dwCaps2Val2 + dwCaps2Val3 + dwCaps2Val4 + dwCaps2Val5 + dwCaps2Val6 + dwCaps2Val7 + dwCaps2Val8;

            return BitConverter.GetBytes(dwCaps2Val);
        }
    }
}