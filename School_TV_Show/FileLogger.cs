namespace School_TV_Show
{
    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logFilePath;

        public FileLogger(string categoryName, string logFilePath)
        {
            _categoryName = categoryName;
            _logFilePath = logFilePath;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true; // Log tất cả cấp độ

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            var logRecord = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] {_categoryName}: {message}";

            if (exception != null)
            {
                logRecord += $"{Environment.NewLine}Exception: {exception}";
            }

            try
            {
                var logDir = Path.GetDirectoryName(_logFilePath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir!);
                }

                File.AppendAllText(_logFilePath, logRecord + Environment.NewLine, System.Text.Encoding.UTF8);
            }
            catch
            {
                // Đừng throw lỗi log, tránh ảnh hưởng app chính
            }
        }
    }
}
