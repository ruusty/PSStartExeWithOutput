using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Collections;

namespace RuustyPowerShellModules
{
    /// <summary>
    /// <paratype="synopsis >Runs an executable</paratype>
    /// <para type="description">Runs an executable and displays the stdout and stderr to Write-Verbose</para>
    /// <para type="description">Can also write stdout and stderr to log files</para>
    /// </summary>
    /// <example>
    ///   <code>Start-ExeWithOutput -FilePath "sleep.exe" -ArgumentList 5</code>
    ///   <para>Sleep for 5 seconds</para>
    /// </example>
    /// <code>Start-ExeWithOutput -FilePath "ping.exe" -ArgumentList @("127.0.0.1", "-n", "5") -verbose </code>
    ///  <para>Ping localhost 5 times and display output on Write-Verbose</para>
    /// <example>
    ///
    /// </example>
    [Cmdlet(VerbsLifecycle.Start, "ExeWithOutput", SupportsShouldProcess = true)]
    public class StartExeWithOutput : Cmdlet
    {
        private AsyncOperation _asyncOp;
        private AutoResetEvent _autoResetEvent;

        Nullable<int> _processId = null;
        private Task<int> task = null;

        private string[] argCollection;
        string args = string.Empty;
        private int[] exitCodeCollection =  {0};
        private List<int> exitCodeList;

       [Parameter(Position = 0, Mandatory = true, HelpMessage = "Executable to start")]
        public string FilePath { get; set; }

        [Parameter(Position = 1, HelpMessage = "Arguments for FilePath executable")]
        public string[] ArgumentList
        {
            get { return argCollection; }
            set { argCollection = value; }
        }

        [Parameter(Mandatory = false, HelpMessage = "Working Directory")]
        public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();


        /// <summary>
        /// <para type="description">Specifies the optional path and file name where the stdout are written.</para>
        /// </summary>
        [Parameter( Mandatory = false,
            HelpMessage = "Enter Stdout log path")]
        public string LogPathStdout { get; set; }

        /// <summary>
        /// <para type="description">Specifies the optional path and file name where the stderr are written.</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "Enter Stderr log path")]
        public string LogPathStderr { get; set; }
        [Parameter (Mandatory = false,HelpMessage ="Valid Exit codes")]
        public int[] ExitCodeList
        {
            get { return exitCodeCollection; }
            set { exitCodeCollection = value; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            _asyncOp = AsyncOperationManager.CreateOperation(null);
            _autoResetEvent = new AutoResetEvent(false);

            if (argCollection != null)
            {
                args = String.Join(" ", argCollection);
            }
            exitCodeList = ExitCodeList.ToList();
        }

#pragma warning disable 1591
        protected override void ProcessRecord()
        {//process each item in the pipeline
            string target = string.Format("'{0}' {1} at ", FilePath, args, WorkingDirectory);
            if (ShouldProcess(target, "Start"))
            {
                _asyncOp.Post(WriteProgressAsync, args);
                try
                {
                    task = Task<int>.Factory.StartNew(() =>
                    {
                        return Start(FilePath, args, WorkingDirectory, LogPathStdout, LogPathStderr);
                    });
                    do
                    {
                        Debug.WriteLine("ProcessRecord-Message Pump Loop");
                        Application.DoEvents();
                    }
                    while (!_autoResetEvent.WaitOne(250));
                    Application.DoEvents();
                    task.Wait();

                    Trace.WriteLine(string.Format("Task.Result='{0}'", task.Result));
                    if (!exitCodeList.Contains(task.Result))
                    {
                        string ExitCodeCsv = string.Empty;
                        ExitCodeCsv = string.Join(",", exitCodeList.Select(p => p));
                        var e = new System.Management.Automation.RuntimeException(String.Format("{0} ExitCode '{1}' not in valid exit codes ({2})", FilePath, task.Result, ExitCodeCsv));
                        var errorRecord = new ErrorRecord(e, string.Format("Unexpected ExitCode of {0} for {1}", task.Result, FilePath), ErrorCategory.InvalidResult, null);
                        WriteError(errorRecord);
                    }
                }
                catch (AggregateException ae)
                {
                    var errorRecord = new ErrorRecord(ae.InnerException, "Unexpected ExitCode for " + ae.InnerException.Message, ErrorCategory.InvalidResult, null);
                    Console.WriteLine("Task has {0}" , task.Status);
                    Console.WriteLine(ae.InnerException);
                    WriteError(errorRecord);
                }
                finally
                {
                    task.Dispose();
                }
                WriteObject(task.Result);
            }
        }

        protected override void StopProcessing()
        {//to handle abnormal termination
            WriteDebug(string.Format("StopProcessing ThreadId: {0} - {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
            WriteWarning("Aborting ....");
            _autoResetEvent.Set();
        }

        protected override void EndProcessing()
        {//do the finalization
            Debug.WriteLine("EndProcessing ThreadId: " + Thread.CurrentThread.ManagedThreadId);
        }

        ///
        ///  _asyncOp.Post(WriteProcessAsync,string)
        ///
        ///
        private void WriteProgressAsync(object message)
        {
            string msg = (string)message;
            ProgressRecord _progressRecord;
            if (String.IsNullOrEmpty(msg))
            {
                msg = " ";
            }
            _progressRecord = new ProgressRecord(0, FilePath, msg);
            WriteProgress(_progressRecord);
        }


        private void WriteObjectAsync(object returnCode)
        {
            WriteObject(returnCode);
        }

        private void WriteVerboseAsync(object message)
        {
            WriteVerbose((string)message);
        }

        private void WriteWarningAsync(object message)
        {
            WriteWarning((string)message);
        }


        private void WriteErrorAsync(object message)
        {
            var errorMessage = (string)message;
            var errorRecord = new ErrorRecord(new System.Management.Automation.RuntimeException(errorMessage), errorMessage,    ErrorCategory.WriteError, null);
            WriteError(errorRecord);
        }

        //
        // Do the needful
        private int Start(
            string filePath,
            string args = "",
            string cwd = "",
            string StdoutLogPath = "",
            string StderrLogPath = ""
            )
    {
        object _locker = new object();
        Trace.WriteLine(string.Format("Starting '{0}' {1}", filePath, args));

        //* Create your Process
        Process process = new Process();
        process.StartInfo.FileName = filePath;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.RedirectStandardError = false;

        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        bool isStdoutLogFileRedirect = (!String.IsNullOrEmpty(StdoutLogPath));
        bool isStderrLogFileRedirect = (!String.IsNullOrEmpty(StderrLogPath));
        //* Set output and error (asynchronous) handlers
            process.OutputDataReceived += (s, e) =>
        {
            if (isStdoutLogFileRedirect)
            {
                lock (_locker)
                {
                    File.AppendAllLines(StdoutLogPath, new string[] { e.Data });
                }
            }
            _asyncOp.Post(WriteVerboseAsync, (string)e.Data);
            Console.WriteLine(e.Data);
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (isStderrLogFileRedirect)
            {
                lock (_locker)
                {
                    File.AppendAllLines(StderrLogPath, new string[] { e.Data });
                }
            }
            if (!String.IsNullOrEmpty(e.Data))
            {
            _asyncOp.Post(WriteVerboseAsync, (string)e.Data);
            Console.WriteLine(e.Data);
            }
        };

        process.Exited += (s, e) =>
        {
            string msg = string.Format("Exit time:{0} Exit code:{1}", process.ExitTime, process.ExitCode);
            Console.WriteLine(msg);
            _asyncOp.Post(WriteVerboseAsync, msg);
            _autoResetEvent.Set();
        };


        //* Optional process configuration
        if (!String.IsNullOrEmpty(args)) { process.StartInfo.Arguments = args; }
        if (!String.IsNullOrEmpty(cwd)) { process.StartInfo.WorkingDirectory = cwd; }

        //* Start process and handlers
        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _processId = process.Id;
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
            _asyncOp.Post(WriteErrorAsync, ex.Message);
            _autoResetEvent.Set();
        }
        return process.ExitCode;
    }
  }
}