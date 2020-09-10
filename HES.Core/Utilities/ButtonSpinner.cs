using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Utilities
{
    public class ButtonSpinnerService
    {
        private HashSet<Func<Task>> _locker = new HashSet<Func<Task>>();

        public async Task SpinAsync(Func<Task> func, object buttonSubmitSpinner)
        {
            if (!_locker.Add(func))
                return;

            try
            {
                buttonSubmitSpinner.GetType().GetMethod("ToggleSpinner").Invoke(buttonSubmitSpinner, new object[] { });
                await func.Invoke();
            }
            finally
            {
                _locker.Remove(func);
                buttonSubmitSpinner.GetType().GetMethod("ToggleSpinner").Invoke(buttonSubmitSpinner, new object[] { });
            }
        }
    }
}
