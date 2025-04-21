using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel.PrescriptionRequest
{
    public class SigningRequest
    {
        public string? SignedInfo { get; set; }
        public string Mode { get; set; } = "production";
        public string CertificatePassword { get; set; } = string.Empty;
        public string CertificateBase64 { get; set; } = string.Empty;
    }
    public class SignatureResponse
    {
        public string? SignatureValue { get; set; }
        public string? Certificate { get; set; }
        public string? SignatureAlgorithm { get; set; }
        public KeyInfo? KeyInfo { get; set; }
    }

    public class KeyInfo
    {
        public X509Data? X509Data { get; set; }
    }

    public class X509Data
    {
        public string? X509Certificate { get; set; }
    }
}
