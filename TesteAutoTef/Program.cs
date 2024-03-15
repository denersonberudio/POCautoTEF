using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback += CertificateValidationCallback;

        // Endereço do site
        string siteUrl = "pokeapi.co";

        try
        {

            // Criar um cliente HTTP
            using (HttpClient client = new HttpClient(handler))
            {
                // Tentar fazer uma requisição para o servidor
                try
                {

                    HttpResponseMessage response = await client.GetAsync($"https://{siteUrl}/api/v2/ability/battle-armor");
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Resposta do servidor:");
                    Console.WriteLine(responseBody);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Erro ao conectar ao servidor: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar certificado: {ex.Message}");
        }
    }

    private static bool CertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {

        // 2. Get the highest level Certification Authority (CA) in the chain
        X509Certificate2 highestCACertificate = new X509Certificate2(certificate); //chain.ChainElements[chain.ChainElements.Count - 1].Certificate;

        // 3. Assure that the CA certificate is valid!
        bool isCACertificateValid = highestCACertificate.Verify();

        //4 Compare the expected thumbprint 
        string publickey = CalculateCertificateFingerprint(highestCACertificate);
        bool isValidPublicKey = "70c7964ae7d364e52fb7d32f66254a33d0fbc7d4963c69ba9bd2ebfefe8b83ef".Equals(publickey);

        return isCACertificateValid && isValidPublicKey;
    }

    static string CalculateCertificateFingerprint(X509Certificate2 certificate)
    {
        // Calcular a impressão digital SHA-256 do certificado
        using (var sha256 = SHA256.Create())
        {
            byte[] certBytes = certificate.GetRawCertData();
            byte[] hashBytes = sha256.ComputeHash(certBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
