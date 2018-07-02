using System;
namespace netco {
    public static class Debug {
        public static event Action<object> LogPrinter;

        public static void Log(object obj) {
            if (LogPrinter != null)
                LogPrinter.Invoke(obj);
        }
    }
}
