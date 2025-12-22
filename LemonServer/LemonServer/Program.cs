using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PresentMonFps;

namespace LemonServer
{
    class Program
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        static private Task fpsTask;
        static private readonly object fpsLock = new object();

        static uint currentPid = 0;
        static volatile float FPS = 0;
        static CancellationTokenSource fpsCts;

        static void Main(string[] args)
        {
            if (!FpsInspector.IsAvailable)
            {
                Console.WriteLine("This server is only available for Windows.");
                return;
            }

            if (args.Length == 0 || !int.TryParse(args[0], out int port))
            {
                Console.WriteLine("Usage: dotnet run <port>");
                port = 8080;
            }

            HardwareInfo hwInfo = new HardwareInfo();

            string url = $"http://*:{port}/";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine($"Server started on {url}");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                hwInfo.refresh();
                UpdateFPSPid();

                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/")
                {
                    var responseData = new
                    {
                        cpuName = hwInfo.cpuName,
                        gpuName = hwInfo.gpuName,
                        totalMemory = hwInfo.totalMemory,
                        availableMemory = hwInfo.availableMemory,
                        cpuTemp = hwInfo.cpuTemp,
                        cpuLoad = hwInfo.cpuLoad,
                        gpuTemp = hwInfo.gpuTemp,
                        gpuLoad = hwInfo.gpuLoad,
                        gpuTotalVram = hwInfo.gpuTotalVram,
                        gpuAvailVram = hwInfo.gpuAvailVram,
                        currFps = FPS
                    };

                    string jsonResponse = JsonSerializer.Serialize(responseData);
                    byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = (int)HttpStatusCode.OK;

                    using (Stream output = response.OutputStream)
                    {
                        output.Write(buffer, 0, buffer.Length);
                    }
                }
                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/exit")
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                    break;
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Close();
                }
            }

            FpsInspector.StopTraceSession();
            fpsCts?.Cancel();
            fpsCts?.Dispose();
            hwInfo.close();
        }

        static void UpdateFPSPid()
        {
            IntPtr hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out uint processId);

            if (currentPid != processId)
            {
                // Prints .exe file
                // string exeName = Path.GetFileName(Process.GetProcessById((int)processId).MainModule.FileName);
                // Console.WriteLine($"{exeName}");

                currentPid = processId;
                StartFPS(processId);
            }
        }

        static void StartFPS(uint pid)
        {
            lock (fpsLock)
            {
                currentPid = pid;

                // Cancel previous instance
                fpsCts?.Cancel();
                fpsCts?.Dispose();
                fpsCts = new CancellationTokenSource();
                var token = fpsCts.Token;

                fpsTask = Task.Run(async () =>
                {
                    try
                    {
                        await FpsInspector.StartForeverAsync(new FpsRequest(pid) { PeriodMillisecond = 10 }, result =>
                        {
                            float fps = (float)result.Fps;
                            FPS = fps;
                        }, token);
                    }
                    catch (OperationCanceledException)
                    {
                        // normal during cancellation
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        FPS = 0;
                    }
                });
            }
        }
    }
}
