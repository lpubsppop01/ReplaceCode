using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Lpubsppop01.ReplaceCode
{
    class CodeParser : IDisposable
    {
        #region Constructor

        Process process;
        List<TaskCompletionSource<AST>> tcsList = new List<TaskCompletionSource<AST>>();

        public CodeParser(string command, string args)
        {
            process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = args;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        #endregion

        #region Process Event Handlers

        StringBuilder outputBuffer = new StringBuilder();

        void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                if (outputBuffer.Length == 0) return;
                var outputString = outputBuffer.ToString();
                outputBuffer.Clear();

                if (!tcsList.Any()) return;
                var tcs = tcsList.First();
                tcsList.RemoveAt(0);

                var serializer = new DataContractJsonSerializer(typeof(AST), AST.ComponentTypes);
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(outputString)))
                {
                    var ast = serializer.ReadObject(ms) as AST;
                    tcs.SetResult(ast);
                }
            }
            else
            {
                outputBuffer.AppendLine(e.Data);
            }
        }

        void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new InvalidOperationException(e.Data);
        }

        #endregion

        #region Action Methods

        public AST Parse(string filePath)
        {
            var task = ParseAsync(filePath);
            task.Wait();
            return task.Result;
        }

        public Task<AST> ParseAsync(string filePath)
        {
            var tcs = new TaskCompletionSource<AST>();
            tcsList.Add(tcs);
            process.StandardInput.WriteLine(filePath);
            process.StandardInput.Flush();
            return tcs.Task;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (process.HasExited) return;
            process.Kill();
        }

        #endregion
    }
}
