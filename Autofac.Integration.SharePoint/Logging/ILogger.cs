using System;

namespace Autofac.Integration.SharePoint
{
    public interface ILogger
    {
        string Uid { get; }
        void Log(Type type, string message);
        void Log(Type type, string propertyOrMethodSignature, string message);

        void Log(Type type, string propertyOrMethodSignature, LogLevel sharePointLoggerTraceLevel, string messageFormat,
                 params object[] messageParameters);
    }
}