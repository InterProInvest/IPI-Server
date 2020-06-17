using HES.Core.Enums;
using HES.Core.Interfaces;
using System;
using System.Timers;

namespace HES.Core.Services
{
    public class ToastService : IToastService, IDisposable
    {
        public event Action<string, ToastLevel> OnShow;
        public event Action OnHide;
        private Timer _timer;

        public void ShowToast(string message, ToastLevel level)
        {
            OnShow?.Invoke(message, level);

            if (level == ToastLevel.Success || level == ToastLevel.Notify)
                StartCountdown();
        }

        public void HideToast(object source, ElapsedEventArgs args)
        {
            OnHide?.Invoke();
        }

        private void StartCountdown()
        {
            SetCountdown();

            if (_timer.Enabled)
            {
                _timer.Stop();
                _timer.Start();
            }
            else
            {
                _timer.Start();
            }
        }

        private void SetCountdown()
        {
            if (_timer == null)
            {
                _timer = new Timer(3500);
                _timer.Elapsed += HideToast;
                _timer.AutoReset = false;
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}