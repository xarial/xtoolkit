using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace Xarial.XToolkit.Licensing
{
    /// <summary>
    /// Private key based license generator
    /// </summary>
    /// <typeparam name="TLicData">License data</typeparam>
    /// <remarks>This service generates the license as signed XML file</remarks>
    public class SignedXmlLicenseGenerator<TLicData> : ILicenseGenerator<TLicData>
    {
        private readonly RSA m_PrivateKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="privateKey">Private key (e.g. X509Certificate2::GetRSAPrivateKey)</param>
        public SignedXmlLicenseGenerator(RSA privateKey) 
        {
            m_PrivateKey = privateKey;
        }

        /// <inheritdoc/>
        public byte[] GenerateLicense(TLicData licData)
        {
            var xmlDoc = new XmlDocument();
            var nav = xmlDoc.CreateNavigator();

            using (var xmlWriter = nav.AppendChild())
            {
                var xmlSer = new XmlSerializer(typeof(TLicData));
                xmlSer.Serialize(xmlWriter, licData);
            }

            var signedXml = new SignedXml(xmlDoc);

            signedXml.SigningKey = m_PrivateKey;

            var reference = new Reference()
            {
                Uri = ""
            };

            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            signedXml.AddReference(reference);

            signedXml.ComputeSignature();

            var xmlDigitalSignature = signedXml.GetXml();

            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

            if (xmlDoc.FirstChild is XmlDeclaration)
            {
                xmlDoc.RemoveChild(xmlDoc.FirstChild);
            }

            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = new XmlTextWriter(stream, new UTF8Encoding(false)))
                {
                    xmlDoc.WriteTo(xmlWriter);
                }

                return stream.ToArray();
            }
        }
    }
}
