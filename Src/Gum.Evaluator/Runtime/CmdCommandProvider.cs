using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    public class CmdCommandProvider : ICommandProvider
    {
        public Task ExecuteAsync(string cmdText)
        {
            var tcs = new TaskCompletionSource<int>();

            var psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c " + cmdText;
            psi.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
