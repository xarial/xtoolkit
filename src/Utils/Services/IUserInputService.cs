using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Services
{
    /// <summary>
    /// Service to request the input from the user
    /// </summary>
    public interface IUserInputService
    {
        /// <summary>
        /// Requests the input value from the user
        /// </summary>
        /// <param name="prompt">User prompt</param>
        /// <param name="value">Initial value of the input and the value entered by the user. Unchanged if cancelled</param>
        /// <returns>True if user accepted the input, false if cancelled</returns>
        bool TryGetInput(string prompt, ref string value);
    }
}
