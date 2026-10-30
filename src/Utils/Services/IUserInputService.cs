using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// <inheritdoc/>
        bool TryGetInput(string prompt, ref string value);

        /// <summary>
        /// Requests the input value from the user, input box is displayed at the specified position
        /// </summary>
        /// <param name="prompt">User prompt</param>
        /// <param name="pos">Position of the input box in device independent units</param>
        /// <param name="value">Initial value of the input and the value entered by the user. Unchanged if cancelled</param>
        bool TryGetInput(string prompt, Point pos, ref string value);

        /// <summary>
        /// Requests the input value from the user, input box is displayed at the mouse cursor
        /// </summary>
        /// <param name="prompt">User prompt</param>
        /// <param name="value">Initial value of the input and the value entered by the user. Unchanged if cancelled</param>
        bool TryGetInputAtCursor(string prompt, ref string value);
    }
}
