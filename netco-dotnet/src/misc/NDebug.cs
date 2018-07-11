using System;
namespace netco {
    internal static class NDebug {
        public static event Action<object> LogPrinter;

        public static void Log(object obj) {
            if (LogPrinter != null)
                LogPrinter.Invoke(obj);
        }
    }
}
