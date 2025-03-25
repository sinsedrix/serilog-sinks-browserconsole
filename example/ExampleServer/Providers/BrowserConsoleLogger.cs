using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ExampleServer.Providers
{
    public class BrowserConsoleLogger : ILogger
    {
        private readonly string _categoryName;
        private IJSRuntime _jsRuntime;
        private readonly List<LogItem> _logQueue = new();
        private bool _initialized = false;

        public BrowserConsoleLogger(string categoryName)
        {
            _categoryName = categoryName.Split('.').Last();
        }

        public void SetJsRuntime(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            FlushLogs();
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            var formattedMessage = $"[{_categoryName}] {message}";
            var item = new LogItem { Level = logLevel, Exception = exception?.Message, Message = formattedMessage };

            if (_initialized && _jsRuntime != null)
            {
                await InvokeLog(item);
            }
            else
            {
                _logQueue.Add(item);
            }
        }

        private async void FlushLogs()
        {
            _initialized = true;
            if (_jsRuntime != null)
            {
                foreach (var log in _logQueue)
                {
                    await InvokeLog(log);
                }
                _logQueue.Clear();
            }
        }

        private Task InvokeLog(LogItem item)
        {
            var t = item.Level switch
            {
                LogLevel.Trace => _jsRuntime.InvokeVoidAsync("console.trace", item.Message, item.Exception),
                LogLevel.Debug => _jsRuntime.InvokeVoidAsync("console.debug", item.Message, item.Exception),
                LogLevel.Information => _jsRuntime.InvokeVoidAsync("console.info", item.Message, item.Exception),
                LogLevel.Warning => _jsRuntime.InvokeVoidAsync("console.warn", item.Message, item.Exception),
                LogLevel.Error => _jsRuntime.InvokeVoidAsync("console.error", item.Message, item.Exception),
                _ => _jsRuntime.InvokeVoidAsync("console.log", item.Message, item.Exception),
            };
            //await t;
            return Task.CompletedTask;
        }
    }

    public class LogItem
    {
        public LogLevel Level {get; set;}
        public string Exception { get; set;}
        public string Message { get; set; }
    }

    public class BrowserConsoleLoggerProvider : ILoggerProvider
    {
        private readonly List<BrowserConsoleLogger> _loggers = new();

        public ILogger CreateLogger(string categoryName)
        {
            var logger = new BrowserConsoleLogger(categoryName);
            _loggers.Add(logger);
            return logger;
        }

        public void SetJsRuntime(IJSRuntime jsRuntime)
        {
            foreach (var logger in _loggers)
            {
                logger.SetJsRuntime(jsRuntime);
            }
        }

        public void Dispose() { }
    }

}
