using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 日志工具类，封装GD.Print，提供规范化的日志输出
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 日志级别枚举
        /// </summary>
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Critical
        }

        /// <summary>
        /// 当前日志级别
        /// </summary>
        public static LogLevel CurrentLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="callerMemberName">调用方法名（自动填充）</param>
        /// <param name="callerFilePath">调用文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用行号（自动填充）</param>
        public static void Debug(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (CurrentLevel <= LogLevel.Debug)
            {
                PrintLog(LogLevel.Debug, message, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        /// <summary>
        /// 信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="callerMemberName">调用方法名（自动填充）</param>
        /// <param name="callerFilePath">调用文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用行号（自动填充）</param>
        public static void Info(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (CurrentLevel <= LogLevel.Info)
            {
                PrintLog(LogLevel.Info, message, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="callerMemberName">调用方法名（自动填充）</param>
        /// <param name="callerFilePath">调用文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用行号（自动填充）</param>
        public static void Warning(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (CurrentLevel <= LogLevel.Warning)
            {
                PrintLog(LogLevel.Warning, message, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="callerMemberName">调用方法名（自动填充）</param>
        /// <param name="callerFilePath">调用文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用行号（自动填充）</param>
        public static void Error(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (CurrentLevel <= LogLevel.Error)
            {
                PrintLog(LogLevel.Error, message, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        /// <summary>
        /// 严重错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="callerMemberName">调用方法名（自动填充）</param>
        /// <param name="callerFilePath">调用文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用行号（自动填充）</param>
        public static void Critical(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (CurrentLevel <= LogLevel.Critical)
            {
                PrintLog(LogLevel.Critical, message, callerMemberName, callerFilePath, callerLineNumber);
            }
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="callerMemberName">调用方法名</param>
        /// <param name="callerFilePath">调用文件路径</param>
        /// <param name="callerLineNumber">调用行号</param>
        private static void PrintLog(LogLevel level, string message, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            // 获取当前时间，格式为：YYYY-MM-DD HH:MM:SS.fff
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // 获取类名（从文件路径中提取）
            string className = System.IO.Path.GetFileNameWithoutExtension(callerFilePath);

            // 日志级别颜色映射
            string color = level switch
            {
                LogLevel.Debug => "[color=gray]",
                LogLevel.Info => "[color=white]",
                LogLevel.Warning => "[color=yellow]",
                LogLevel.Error => "[color=red]",
                LogLevel.Critical => "[color=darkred]",
                _ => "[color=white]"
            };

            // 日志输出格式：[时间] [级别] [类名.方法名:行号] 消息
            string logMessage = $"{color}[{time}] [{level.ToString().ToUpper()}] [{className}.{callerMemberName}:{callerLineNumber}] {message}[/color]";

            // 使用GD.PrintRich输出带颜色的日志
            GD.PrintRich(logMessage);
        }
    }
}