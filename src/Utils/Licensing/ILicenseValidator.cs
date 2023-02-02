using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Licensing.Exceptions;

namespace Xarial.XToolkit.Licensing
{
    /// <summary>
    /// Validates the custom software license
    /// </summary>
    /// <typeparam name="TLicData">License data type</typeparam>
    public interface ILicenseValidator<TLicData>
    {
        /// <summary>
        /// Validates the license and returns the license data instance
        /// </summary>
        /// <param name="license">License buffer</param>
        /// <returns>License data</returns>
        /// <remarks>Throw <see cref="LicenseValidationException"/> for the invalid license</remarks>
        TLicData ValidateLicense(byte[] license);
    }

    /// <summary>
    /// Additional methods of <see cref="ILicenseValidator{TLicData}"/>
    /// </summary>
    public static class LicenseValidatorExtension
    {
        /// <summary>
        /// Validates the license from the file
        /// </summary>
        /// <typeparam name="TLicData">License data type</typeparam>
        /// <param name="licVal">Validator</param>
        /// <param name="filePath">File path</param>
        /// <returns>License data</returns>
        /// <exception cref="LicenseNotFoundException"/>
        public static TLicData ValidateLicenseFile<TLicData>(this ILicenseValidator<TLicData> licVal, string filePath)
        {
            if (File.Exists(filePath))
            {
                return licVal.ValidateLicense(File.ReadAllBytes(filePath));
            }
            else
            {
                throw new LicenseNotFoundException(filePath);
            }
        }
    }
}
