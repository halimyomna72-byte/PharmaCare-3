using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace PharmaCare
{
    // ─────────────────────────────────────────────
    //  Alert Sound Manager — Cross-Platform
    // ─────────────────────────────────────────────
    public static class AlertManager
    {
        private static volatile bool _soundActive = false;
        private static Thread _alertThread = null;
        private static readonly object _lock = new object();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Beep(uint dwFreq, uint dwDuration);

        public static void StartAlert()
        {
            lock (_lock)
            {
                if (_alertThread != null && _alertThread.IsAlive) return;
                _soundActive = true;
                _alertThread = new Thread(AlertLoop) { IsBackground = true };
                _alertThread.Start();
            }
        }

        public static void StopAlert()
        {
            _soundActive = false;
        }

        private static void AlertLoop()
        {
            while (_soundActive)
            {
                BeepSafe(800, 250);
                if (!_soundActive) break;
                Thread.Sleep(50);
                BeepSafe(1000, 250);
                if (!_soundActive) break;
                Thread.Sleep(50);
                BeepSafe(1200, 350);
                if (!_soundActive) break;

                for (int i = 0; i < 10; i++)
                {
                    if (!_soundActive) break;
                    Thread.Sleep(100);
                }
            }
        }

        private static void BeepSafe(uint freq, uint duration)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Beep(freq, duration);
                else
                    Console.Beep((int)freq, (int)duration);
            }
            catch { /* silent fail */ }
        }
    }
}