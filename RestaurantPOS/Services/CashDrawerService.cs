using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public class CashDrawerService
    {
        private readonly SettingsService _settingsService;

        // ─── Win32 raw printing API ───────────────────────────────────────────
        [DllImport("winspool.drv", EntryPoint = "OpenPrinterA",
            SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter(
            string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.drv", EntryPoint = "ClosePrinter",
            SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "StartDocPrinterA",
            SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(
            IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.drv", EntryPoint = "EndDocPrinter",
            SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "StartPagePrinter",
            SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "EndPagePrinter",
            SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "WritePrinter",
            SetLastError = true, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(
            IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName = "Cash Drawer";
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile = null!;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType = "RAW";
        }

        // ─── ESC/POS drawer kick command ──────────────────────────────────────
        // ESC p pin t1 t2
        // pin = 0x00 (drawer 1) or 0x01 (drawer 2)
        // t1  = pulse on time  (0x19 = 25 × 2ms = 50ms)
        // t2  = pulse off time (0xFA = 250 × 2ms = 500ms)
        private static readonly byte[] DrawerKickCommand =
        {
            0x1B, 0x70, 0x00, 0x19, 0xFA
        };

        public CashDrawerService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// Opens the cash drawer by sending the ESC/POS kick command
        /// directly to the configured receipt printer as raw bytes.
        /// </summary>
        public bool OpenDrawer()
        {
            var printerName = _settingsService.Settings.DefaultPrinter;

            if (string.IsNullOrWhiteSpace(printerName))
            {
                System.Diagnostics.Debug.WriteLine(
                    "CashDrawerService: No default printer configured.");
                return false;
            }

            return SendRawBytesToPrinter(printerName, DrawerKickCommand);
        }

        // ─── Raw print helper ─────────────────────────────────────────────────

        private static bool SendRawBytesToPrinter(string printerName, byte[] bytes)
        {
            IntPtr hPrinter = IntPtr.Zero;
            IntPtr pBytes = IntPtr.Zero;

            try
            {
                if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                    return false;

                var di = new DOCINFOA();

                if (!StartDocPrinter(hPrinter, 1, di))
                    return false;

                if (!StartPagePrinter(hPrinter))
                {
                    EndDocPrinter(hPrinter);
                    return false;
                }

                // Marshal byte array to unmanaged memory
                pBytes = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, pBytes, bytes.Length);

                bool success = WritePrinter(
                    hPrinter, pBytes, bytes.Length, out _);

                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"CashDrawerService error: {ex.Message}");
                return false;
            }
            finally
            {
                if (pBytes != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pBytes);

                if (hPrinter != IntPtr.Zero)
                    ClosePrinter(hPrinter);
            }
        }
    }
}
