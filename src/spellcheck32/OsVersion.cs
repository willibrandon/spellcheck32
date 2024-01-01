using Windows.Wdk;
using Windows.Win32.System.SystemInformation;

namespace spellcheck32
{
    internal class OsVersion
    {
        /// <summary>
        ///  Determines whether the OS version is Windows 8 or later.
        /// </summary>
        /// <returns>
        ///  <see langword="true"/> if the OS version is Windows 8 or later, otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsWindows8OrGreater()
        {
            OSVERSIONINFOW osvi;
            unsafe
            {
                osvi = new()
                {
                    dwOSVersionInfoSize = (uint)sizeof(OSVERSIONINFOW)
                };
            }

            if (0 == PInvoke.RtlGetVersion(ref osvi))
            {
                return osvi.dwMajorVersion > 6 || (osvi.dwMajorVersion == 6 && osvi.dwMinorVersion >= 2);
            }

            return false;
        }
    }
}
