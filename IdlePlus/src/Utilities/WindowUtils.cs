using System.Runtime.InteropServices;

namespace IdlePlus.Utilities {
	public static class WindowUtils {
		[DllImport("user32.dll", EntryPoint = "SetWindowText")]
		public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
	}
}