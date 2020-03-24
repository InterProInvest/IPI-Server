namespace HES.Core.Crypto
{
    public class DataProtectionParams
    {
        public string Version { get; set; } = "1.0";
        public int IterationCount { get; set; } = 100_000;
        public string HashAlgorithmName { get; set; } = "SHA256";
        public string Salt { get; set; }
        public string VerificationCode { get; set; }
    }
}