using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace RuustyPowerShellModules
{
    [Cmdlet(VerbsLifecycle.Start, "ExeWithOutput")]
    public class StartExeWithOutput : Cmdlet
    {
        private bool _abortWrite;
        private ProgressRecord _progressRecord;

        private AsyncOperation _asyncOp;
        private AutoResetEvent _autoResetEvent;

        private string[] argCollection;

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Executable to start")]
        public string FilePath { get; set; }

        [Parameter(Position = 1, HelpMessage = "Arguments for executable")]
        public string[] ArgumentList
        {
            get { return argCollection; }
            set { argCollection = value; }
        }

        [Parameter(Mandatory = false, HelpMessage = "Working Directory")]
        public string cwd { get; set; } = "";


        protected override void BeginProcessing()
        {
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            _asyncOp = AsyncOperationManager.CreateOperation(null);
            _autoResetEvent = new AutoResetEvent(false);
        }

        protected override void ProcessRecord()
        {
            var task = Task.Factory.StartNew(LaunchProcess);
            do
            {
                Application.DoEvents();
            }
            while (!_autoResetEvent.WaitOne(250));
            Application.DoEvents();
            task.Wait();
        }

        protected override void StopProcessing()
        {
            WriteDebug(string.Format("StopProcessing ThreadId: {0} - {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name));
            _abortWrite = true;
            WriteWarning("Aborting ....");
        }

        protected override void EndProcessing()
        {
            Debug.WriteLine("EndProcessing ThreadId: " + Thread.CurrentThread.ManagedThreadId);
        }

        void DoLongOperation()
        {
            string args = string.Empty;
            if (argCollection != null)
            {
                args = String.Join(" ", argCollection);
            }
            _progressRecord = new ProgressRecord(0, string.Format("Starting - {0}", FilePath), args);

            Debug.WriteLine("DoWork ThreadId: " + Thread.CurrentThread.ManagedThreadId);
                for (int i = 0; i < 10; i++)
                {
                    _asyncOp.Post(WriteProcessAsync, string.Format("message:{0}",i));
                    Thread.Sleep(500);
                }


            _asyncOp.Post(WriteWarningAsync,"Just about to exit");

                _autoResetEvent.Set();
        }

        void LaunchProcess()
        {
            string args = string.Empty;
            if (argCollection != null)
            {
                args = String.Join(" ", argCollection);
            }
            _progressRecord = new ProgressRecord(0, string.Format("Starting - {0}", FilePath), args);
            WriteProgress(_progressRecord);
            Debug.WriteLine("DoWork ThreadId: " + Thread.CurrentThread.ManagedThreadId);
            for (int i = 0; i < 10; i++)
            {
                _asyncOp.Post(WriteProcessAsync, string.Format("message:{0}", i));
                Thread.Sleep(500);
            }
            ////* Create your Process
            Process process = new Process();
            process.StartInfo.FileName = FilePath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = false;// true;

            //* Optional process configuration
            if (!String.IsNullOrEmpty(args)) { process.StartInfo.Arguments = args; }
            if (!String.IsNullOrEmpty(cwd)) { process.StartInfo.WorkingDirectory = cwd; }

            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                //Console.WriteLine(e.Data);
                //this.WriteVerbose(e.Data);
                _asyncOp.Post(WriteProcessAsync, e.Data);

            });


            //* Start process and handlers
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                //process.BeginErrorReadLine();
                process.WaitForExit();
                WriteVerbose("After WaitForExit");

            }
            catch (Exception e)
            {
                WriteDebug("Exception");
                ErrorRecord er = new ErrorRecord(e, "", ErrorCategory.InvalidOperation, FilePath);
                base.ThrowTerminatingError(er);
                //throw new System.Management.Automation.RuntimeException(string.Format("{0} ExitCode:{1}", FilePath, process.ExitCode), e);
            }
            finally
            {
                if (process.ExitCode != 0)
                {
                    //throw new System.Management.Automation.RuntimeException(string.Format("{0} ExitCode:{1}", FilePath, process.ExitCode));
                    //ErrorRecord er = new ErrorRecord(e, "", ErrorCategory.InvalidOperation, FilePath);
                    //base.ThrowTerminatingError(er);

                }
            }
            _asyncOp.Post(WriteWarningAsync, "Just about to exit");
            _autoResetEvent.Set();
        }

        private void WriteProcessAsync(object message)
        {
            //_progressRecord.StatusDescription = "((WimMessageProcess)message).Path";
            //_progressRecord.StatusDescription = (string)message;
            //WriteProgress(_progressRecord);
            //WriteVerbose("WriteProcessAsync");
            WriteObject((string)message);
        }
  

        private void WriteWarningAsync(object message)
        {
            WriteWarning((string)message);
        }

        /*
        private void WriteErrorAsync(object message)
        {
            var errorMessage = (WimMessageError)message;
            var errorRecord = new ErrorRecord(new Win32Exception(errorMessage.Win32ErrorCode), errorMessage.Path,
                ErrorCategory.WriteError, null);

            WriteError(errorRecord);
        }
*/

    }
}