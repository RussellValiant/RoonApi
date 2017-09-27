using Microsoft.Extensions.Logging;
#if UNDOK 
using Rts.Base.Async;
using Rts.Base.Logger;
#endif
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestRoonApi
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args != null && args.Length > 1)
            {
#if UNDOK
                if (args[0].ToUpper() == "CONTROL")
                {
                    AsyncInline.Run (async() => { await Start(args[1]); }) ;
                }
#endif
            }
            else
            {
                Application.Run(new Test());
            }
        }
#if UNDOK
        static async Task Start (string ipAddress)
        {
            var loggerFac = new LoggerFactoryEx(LogLevel.Information);
            loggerFac.AddDebug(LogLevel.Trace);
            var adaptor = new RoonControlAdaptorUnDok();
            await adaptor.Start(ipAddress, "BLOCK Wz", loggerFac.CreateLogger("UnDok") as ILoggerEx);
            var controller = new RoonApiLib.RoonControl();
            bool rc = await controller.Start(adaptor, "RICSRV", 60000, 1000, loggerFac.CreateLogger("Roon"));
            await controller.ProcessDeviceChangesLoop();
        }
#endif
    }
}
