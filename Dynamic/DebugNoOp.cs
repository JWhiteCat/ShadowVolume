namespace ShadowVolume
{
    public static class DebugNoOp
    {
        public static void Log(object message) {}
        public static void LogError(object message) {}
        public static void LogFormat(string format, params object[] args) {}
    }
}