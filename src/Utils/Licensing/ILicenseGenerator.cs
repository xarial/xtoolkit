//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System.IO;

namespace Xarial.XToolkit.Licensing
{
    /// <summary>
    /// Service to geenrate a software license
    /// </summary>
    /// <typeparam name="TLicData">License data</typeparam>
    public interface ILicenseGenerator<TLicData> 
    {
        /// <summary>
        /// Generates license from the license data
        /// </summary>
        /// <param name="licData">License data</param>
        /// <returns>License buffer</returns>
        byte[] GenerateLicense(TLicData licData);
    }

    /// <summary>
    /// Additional methods for <see cref="ILicenseGenerator{TLicData}"/>
    /// </summary>
    public static class LicenseGeneratorExtension
    {
        /// <summary>
        /// Generates the license file
        /// </summary>
        /// <typeparam name="TLicData">License data type</typeparam>
        /// <param name="licGen">License generator</param>
        /// <param name="licData">License data</param>
        /// <param name="filePath">Path to save license file</param>
        public static void GenerateLicenseFile<TLicData>(this ILicenseGenerator<TLicData> licGen, TLicData licData, string filePath)
            => File.WriteAllBytes(filePath, licGen.GenerateLicense(licData));
    }
}
