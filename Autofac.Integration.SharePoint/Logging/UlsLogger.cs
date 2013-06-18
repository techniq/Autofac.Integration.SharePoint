using System;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint
{
    public class UlsLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _uid;

        public UlsLogger(string categoryName)
        {
            _categoryName = categoryName;
            _uid = Guid.NewGuid().ToString("N");
        }

        #region ILogger Members

        public string Uid
        {
            get { return _uid; }
        }

        public void Log(Type type, string message)
        {
            Log(type, "", message);
        }

        public void Log(Type type, string propertyOrMethodSignature, string message)
        {
            Log(type, propertyOrMethodSignature, LogLevel.Verbose, message);
        }

        public void Log(Type type, string propertyOrMethodSignature, LogLevel sharePointLoggerTraceLevel,
                        string messageFormat, params object[] messageParameters)
        {
            // implement this
            TraceSeverity traceSeverity;
            EventSeverity eventSeverity;
            switch (sharePointLoggerTraceLevel)
            {
                case LogLevel.Information:
                    traceSeverity = TraceSeverity.Medium;
                    eventSeverity = EventSeverity.Information;
                    break;
                case LogLevel.Warning:
                    traceSeverity = TraceSeverity.High;
                    eventSeverity = EventSeverity.Warning;
                    break;
                case LogLevel.Unexpected:
                    traceSeverity = TraceSeverity.Unexpected;
                    eventSeverity = EventSeverity.Warning;
                    break;
                case LogLevel.Error:
                    traceSeverity = TraceSeverity.High;
                    eventSeverity = EventSeverity.Error;
                    break;
                case LogLevel.Verbose:
                default:
                    traceSeverity = TraceSeverity.Verbose;
                    eventSeverity = EventSeverity.Verbose;
                    break;
            }
            var category = new SPDiagnosticsCategory(_categoryName, traceSeverity, eventSeverity);
            SPDiagnosticsService.Local.WriteTrace(101, category, traceSeverity,
                                                  string.Format(messageFormat, messageParameters), null);
        }

        #endregion
    }
}

public enum LogLevel
{
    Verbose,
    Information,
    Warning,
    Error,
    Unexpected
}