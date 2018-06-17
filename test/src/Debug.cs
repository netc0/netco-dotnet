using System;

public static class Debug {
    public static void Log(object s) {
        var msg = s != null ? s.ToString() : "";
        Console.WriteLine(msg);
    }
}