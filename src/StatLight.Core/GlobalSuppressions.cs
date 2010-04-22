// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project. 
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc. 
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File". 
// You do not need to add suppressions to this file manually. 
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Scope = "member", Target = "StatLight.Extensions.#RaiseEventSafely`1(System.EventHandler,System.Object,!!0)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Scope = "member", Target = "StatLight.Core.Common.ILogger.#Error(System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Scope = "type", Target = "StatLight.Core.Common.StatLightException")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "buttonClickInvokePattern", Scope = "member", Target = "StatLight.Core.Monitoring.DebugAssertMonitor.#ExecuteDialogSlapDown(System.Action`1<System.String>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "okButton", Scope = "member", Target = "StatLight.Core.Monitoring.DebugAssertMonitor.#ExecuteDialogSlapDown(System.Action`1<System.String>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "StatLight.Core.Monitoring.DialogMonitorRunner.#_logger")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MessageBox", Scope = "member", Target = "StatLight.Core.Monitoring.MessageBoxMonitor.#ExecuteDialogSlapDown(System.Action`1<System.String>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "buttonClicInvokePattern", Scope = "member", Target = "StatLight.Core.Monitoring.MessageBoxMonitor.#ExecuteDialogSlapDown(System.Action`1<System.String>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "okButton", Scope = "member", Target = "StatLight.Core.Monitoring.MessageBoxMonitor.#ExecuteDialogSlapDown(System.Action`1<System.String>)")]

//TODO: work on...
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "StatLight.Core.Reporting.TestResultAggregator")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Scope = "member", Target = "StatLight.Core.Reporting.TestResultAggregator.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "StatLight.Core.Reporting.TestResultAggregator.#Dispose()")]
//End TODO work on...
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Runnable", Scope = "member", Target = "StatLight.Core.Reporting.Messages.TestOutcome.#NotRunnable")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RunTime", Scope = "member", Target = "StatLight.Core.Reporting.Providers.Console.ConsoleTestCompleteMessage.#WriteOutCompletionStatement(StatLight.Core.Reporting.TestReport,System.DateTime)")]
