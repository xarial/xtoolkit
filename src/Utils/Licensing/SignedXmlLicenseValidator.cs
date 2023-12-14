//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Xarial.XToolkit.Licensing.Exceptions;

namespace Xarial.XToolkit.Licensing
{
    /// <summary>
    /// Validates the Signed XML based license generated via <see cref="SignedXmlLicenseGenerator{TLicData}"/>
    /// </summary>
    /// <typeparam name="TLicData">License data type</typeparam>
    public abstract class SignedXmlLicenseValidator<TLicData> : ILicenseValidator<TLicData>
    {
        /// <inheritdoc/>
        /// <exception cref="LicenseFileCorruptedException"></exception>
        /// <exception cref="LicenseFileMalformedException"></exception>
        /// <exception cref="InvalidSignatureException"></exception>
        public TLicData ValidateLicense(byte[] license)
        {
            var xmlDoc = new XmlDocument();

            try
            {
                using (var stream = new MemoryStream(license))
                {
                    xmlDoc.Load(stream);
                }
            }
            catch (Exception ex)
            {
                throw new LicenseFileCorruptedException(ex);
            }

            var publicKey = GetPublicKey();

            var signedXml = new SignedXml(xmlDoc);

            var nodeList = xmlDoc.GetElementsByTagName("Signature");

            try
            {
                signedXml.LoadXml((XmlElement)nodeList[0]);
            }
            catch (CryptographicException ex)
            {
                throw new LicenseFileMalformedException(ex);
            }

            if (!signedXml.CheckSignature(publicKey))
            {
                throw new InvalidSignatureException();
            }

            using (var reader = new XmlNodeReader(xmlDoc))
            {
                var xmlSer = new XmlSerializer(typeof(TLicData));

                var licData = (TLicData)xmlSer.Deserialize(reader);

                AuthorizeLicense(licData);

                return licData;
            }
        }

        /// <summary>
        /// License authorization function
        /// </summary>
        /// <param name="license">License data</param>
        /// <remarks>Check the license data (e.g. device id, expiry date, etc.). Throw <see cref="LicenseAuthorizationException"/> if authorization fails</remarks>
        protected abstract void AuthorizeLicense(TLicData license);
        
        /// <summary>
        /// Gets the public key to read license data
        /// </summary>
        /// <returns></returns>
        protected abstract RSA GetPublicKey();
    }
}
