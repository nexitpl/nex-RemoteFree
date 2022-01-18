using System;
using System.Diagnostics;
using nexRemoteFree.Shared.Utilities;

namespace nexRemoteFree.Shared.Services
{
    public interface IProcessInvoker
    {
        string InvokeProcessOutput(string command, string arguments);
    }

    public class ProcessInvoker : IProcessInvoker
    {
        public string InvokeProcessOutput(string command, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo(command, arguments)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "RunAs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                var proc = Process.Start(psi);
                proc.WaitForExit();

                return proc.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Nie uda�o si� rozpocz�� procesu.");
                return string.Empty;
            }
        }
    }
}