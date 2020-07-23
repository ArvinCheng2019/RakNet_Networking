using System.Runtime.InteropServices;
namespace Native
{
    public static class MessageBox
    {
        public enum Type : long
        {
            OK = 0x00000000L,
            OK_CANCEL = 0x00000001L,
            ABORT_RETRY_IGNORE = 0x00000002L,
            YES_NO_CANCEL = 0x00000003L,
            YES_NO = 0x00000004L,
            RETRY_CANCEL = 0x00000005L,
            CANCEL_TRY_CONTINUE = 0x00000006L,
            ICON_ERROR = 0x00000010L,
            ICON_WARNING = 0x00000030L,
            ICON_INFORMATION = 0x00000040L,
            ICON_ASTERISK = 0x00000040L,
            SET_FOREGROUND = 0x00010000L,
            NO_FOCUS = 0x00008000L
        }

        public static int Show(string title, string text, Type type)
        {
            return _callNative(0, text, title, (long)type);
        }

        public static int Show(string title, string text)
        {
            return Show( title, text, Type.SET_FOREGROUND);
        }

        public static int Show(string text, Type type)
        {
            return Show( "", text, type);
        }

        public static int Show(string text)
        {
            return Show("", text, Type.SET_FOREGROUND);
        }


        [DllImport("user32.dll", EntryPoint = "MessageBox", SetLastError = true)]
        private static extern int _callNative(int hwnd, string text, string title, long type);
    }
}
