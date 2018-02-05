using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace OCR
{
    public class AspriseOCR
    {
        [DllImport("OCR\\AspriseOCR.dll", EntryPoint = "OCR")]
        public static extern IntPtr OCR(string file, int type);
        [DllImport("OCR\\AspriseOCR.dll", EntryPoint = "OCRpart")]
        public static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);
        [DllImport("OCR\\AspriseOCR.dll", EntryPoint = "OCRBarCodes")]
        public static extern IntPtr OCRBarCodes(string file, int type);
        [DllImport("OCR\\AspriseOCR.dll", EntryPoint = "OCRpartBarCodes")]
        public static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);
    }
}
