using log4net;
using log4net.Config;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MyLogger
{
    public class SubscribeLogger : ILog
    {
        private static readonly ILog log;
        private static object lockObj = new object();
        private static Dictionary<string, SubscribeLogger> _subscribeLoggerDict { get; set; }
        static SubscribeLogger()
        {
            var directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Log4net");
            XmlConfigurator.Configure(new FileInfo(Path.Combine(directory, "log4net.config")));
            log = LogManager.GetLogger("SubscribeLogger");
            _subscribeLoggerDict = new Dictionary<string, SubscribeLogger>();
        }
        public static SubscribeLogger GetLogger(string name)
        {
            lock(lockObj)
            {
                if (_subscribeLoggerDict.ContainsKey(name))
                {
                    return _subscribeLoggerDict[name];
                }
                else
                {
                    var logger = new SubscribeLogger();

                    _subscribeLoggerDict.Add(name, logger);
                    return logger;
                }
            }

        }
        public event Action<object> OnDebug;
        public event Action<object, Exception> OnDebugWithException;


        public event Action<object> OnInfo;
        public event Action<object, Exception> OnInfoWithException;


        public event Action<object> OnWarn;
        public event Action<object, Exception> OnWarnWithException;


        public event Action<object> OnError;
        public event Action<object, Exception> OnErrorWithException;


        public event Action<object> OnFatal;
        public event Action<object, Exception> OnFatalException;


        private SubscribeLogger() { }
        public bool IsDebugEnabled => log.IsDebugEnabled;

        public bool IsInfoEnabled => log.IsInfoEnabled;

        public bool IsWarnEnabled => log.IsWarnEnabled;

        public bool IsErrorEnabled => log.IsErrorEnabled;

        public bool IsFatalEnabled => log.IsFatalEnabled;

        ILogger ILoggerWrapper.Logger => log.Logger;


        public void Debug(object message)
        {
            log.Debug(message);
            OnDebug?.Invoke(message);
        }

        public void Debug(object message, Exception exception)
        {
            log.Debug(message, exception);
            OnDebugWithException?.Invoke(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            
            log.DebugFormat(format, args);
            var message = string.Format(format, args);
            OnDebug?.Invoke(message);
        }

        public void DebugFormat(string format, object arg0)
        {
            DebugFormat(format, new object[] { arg0 });
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            DebugFormat(format, new object[] { arg0, arg1 });
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            DebugFormat(format, new object[] { arg0, arg1, arg2 });
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            
            log.DebugFormat(provider, format, args);
            var mesage = string.Format(provider, format, args);
            OnDebug?.Invoke(mesage);
        }

        public void Error(object message)
        {
            log.Error(message);
            OnError?.Invoke(message);
        }

        public void Error(object message, Exception exception)
        {
            log.Error(message, exception);
            OnErrorWithException?.Invoke(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
            var message = string.Format(format, args);
            OnError?.Invoke(message);
        }

        public void ErrorFormat(string format, object arg0)
        {
            ErrorFormat(format, new object[] { arg0});
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            ErrorFormat(format, new object[] { arg0, arg1 });
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            ErrorFormat(format, new object[] { arg0, arg1, arg2 });
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.ErrorFormat(provider, format, args);
            var mesage = string.Format(provider, format, args);
            OnError?.Invoke(mesage);
        }

        public void Fatal(object message)
        {
            log.Fatal(message);
            OnFatal?.Invoke(message);
        }

        public void Fatal(object message, Exception exception)
        {
            log.Fatal(message, exception);
            OnFatalException?.Invoke(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            log.FatalFormat(format, args);
            var message = string.Format(format, args);
            OnFatal?.Invoke(message);

        }

        public void FatalFormat(string format, object arg0)
        {
            FatalFormat(format, new object[] { arg0 });
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            FatalFormat(format, new object[] { arg0, arg1 });
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            FatalFormat(format, new object[] { arg0, arg1,arg2 });

        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.FatalFormat(provider, format, args);
            var message = string.Format(provider, format, args);
            OnFatal?.Invoke(message);
        }

        public void Info(object message)
        {
            log.Info(message);
            OnInfo?.Invoke(message);
        }

        public void Info(object message, Exception exception)
        {
            log.Info(message, exception);
            OnInfoWithException?.Invoke(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            log.InfoFormat(format, args);
            var message = string.Format(format, args);
            OnInfo?.Invoke(message);
        }

        public void InfoFormat(string format, object arg0)
        {
            InfoFormat(format, new object[] { arg0 });
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            InfoFormat(format, new object[] { arg0, arg1});
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            InfoFormat(format, new object[] { arg0, arg1, arg2 });
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.InfoFormat(provider, format, args);
            var message = string.Format(provider, format, args);
            OnInfo?.Invoke(message);
        }

        public void Warn(object message)
        {
            log.Warn(message);
            OnWarn?.Invoke(message);
        }

        public void Warn(object message, Exception exception)
        {
            log.Warn(message, exception);
            OnWarnWithException?.Invoke(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            log.WarnFormat(format, args);
            var message = string.Format(format, args);
            OnWarn?.Invoke(message);
        }

        public void WarnFormat(string format, object arg0)
        {
            WarnFormat(format, new object[] { arg0 });
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            WarnFormat(format, new object[] { arg0, arg1 });
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            WarnFormat(format, new object[] { arg0, arg1, arg2 });
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            log.WarnFormat(provider, format, args);
            var message = string.Format(provider, format, args);
            OnWarn?.Invoke(message);
        }
    }
}
