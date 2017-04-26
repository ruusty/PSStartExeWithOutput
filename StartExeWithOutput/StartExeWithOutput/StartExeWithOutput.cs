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


        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Executable to start")]
        public string FilePath { get; set; }

        void OutputHandler(object sendingProcess, object outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine("outLine.Data");
            //WriteObject(outLine.Data);
            //this.WriteVerbose(outLine.Data);
        }

        protected override void BeginProcessing()
        {
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            _asyncOp = AsyncOperationManager.CreateOperation(null);
            _autoResetEvent = new AutoResetEvent(false);
        }

        protected override void ProcessRecord()
        {
            var task = Task.Factory.StartNew(DoLongOperation);
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
            _progressRecord = new ProgressRecord(0, string.Format("Applying Windows Image - {0}", "imageNameNode.TypedValue"), "Starting");
            Debug.WriteLine("DoWork ThreadId: " + Thread.CurrentThread.ManagedThreadId);
                for (int i = 0; i < 20; i++)
                {
                    _asyncOp.Post(WriteProcessAsync, string.Format("message:{0}",i));
                    //_backgroundWorker.ReportProgress(i * 5);
                    Thread.Sleep(1000);
                }
                _autoResetEvent.Set();
        }

        private void WriteProcessAsync(object message)
        {
            _progressRecord.StatusDescription = "((WimMessageProcess)message).Path";
            WriteProgress(_progressRecord);
            WriteVerbose("WriteProcessAsync");
            WriteObject("Line of text");
        }

    }
}