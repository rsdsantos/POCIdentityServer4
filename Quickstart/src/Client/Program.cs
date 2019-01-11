using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                //await CallAPIByClientCredentials(); // Chamadas para clients sem utilizar usuário e senha para autorização.
                await CallAPIByResourceOwnerPassword(); // Chamadas para clients utilizando usuário e senha para autorização.

                Console.ReadKey();
            }).Wait();           
        }

        #region AllowedGrantTypes => GrantTypes.ClientCredentials
        static async Task CallAPIByClientCredentials()
        {
            // Recupera os end-points do IdentityServer.
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(@"http://localhost:5000");

            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // Recupera um token de acesso do IdentityServer.
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            else
            {
                Console.WriteLine(tokenResponse.Json);
            }

            // Chama a API passando o token de acesso.
            client.SetBearerToken(tokenResponse.AccessToken); // Define o access token no header.

            var responseAPI = await client.GetAsync("http://localhost:5001/identity");

            if (!responseAPI.IsSuccessStatusCode)
            {
                Console.WriteLine(responseAPI.StatusCode);
            }
            else
            {
                var content = await responseAPI.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
        #endregion

        #region AllowedGrantTypes => GrantTypes.ResourceOwnerPassword
        static async Task CallAPIByResourceOwnerPassword()
        {
            // Recupera os end-points do IdentityServer.
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(@"http://localhost:5000");

            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // Recupera um token de acesso do IdentityServer.
            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "ro.client",
                ClientSecret = "secret",

                UserName = "Alice",
                Password = "password",
                Scope = "api1"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            else
            {
                Console.WriteLine(tokenResponse.Json);
            }

            // Chama a API passando o token de acesso.
            client.SetBearerToken(tokenResponse.AccessToken); // Define o access token no header.

            var responseAPI = await client.GetAsync("http://localhost:5001/identity");

            if (!responseAPI.IsSuccessStatusCode)
            {
                Console.WriteLine(responseAPI.StatusCode);
            }
            else
            {
                var content = await responseAPI.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
        #endregion
    }
}
