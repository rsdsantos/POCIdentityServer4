using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
    /*
        Definição de escopo: Escopos representam algo que você quer proteger e que os clientes querem utilizar.

        Por contexto:
            OAuth2 - Escopos representam APIs
            OIDC - Escopos representam dados de identidade como ID, Nome, E-mail, etc.
     */

    public static class Config
    {
        // Registra os recursos transmitidos pelos tokens nos fluxos que utilizam Authorization Code.
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        // Registra os recursos de APIs que serão protegidos pelo IdentityServer.
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                // Exemplo simples
                new ApiResource("api1", "My API"),

                // Exemplo detalhado
                new ApiResource
                {
                    Name = "api2",

                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // Inclui as seguintes claims no access_token (além do subject id)
                    UserClaims = { JwtClaimTypes.Name, JwtClaimTypes.Email },

                    // Associa dois escopos de exemplo
                    Scopes =
                    {
                        new Scope()
                        {
                            Name = "api2.full_access",
                            DisplayName = "Full access to API 2",
                        },
                        new Scope
                        {
                            Name = "api2.read_only",
                            DisplayName = "Read only access to API 2"
                        }
                    }
                }
            };
        }

        // Registra os clients que terão acesso aos recursos.
        // Clients representam aplicativos que podem solicitar tokens ao IdentityServer.
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                // Console Application
                #region Client Credentials
                new Client{
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials, // Autenticação sem necessidade de um usuário (apenas clientid/secret).
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api1" } // Escopo definido no método GetApis().
                },
                #endregion
                #region Resource Owner Password
                new Client{
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, // Autenticação que exige um usuário e senha válidos.
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api1" }
                },
	            #endregion               

                // WebSite MVC
                #region Implict Flow
                //new Client{
                //    ClientId = "mvc",
                //    ClientName = "MVC Client",
                //    AllowedGrantTypes = GrantTypes.Implicit,

                //    RedirectUris = { "http://localhost:5002/signin-oidc" }, // Redirecionamento após o login.                    
                //    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" }, // Redirecionamento após o logout.

                //    RequireConsent = false, // Habilita / desabilita o consentimento de escopos após o login.

                //    AllowedScopes = new List<string>
                //    {
                //        IdentityServerConstants.StandardScopes.OpenId,
                //        IdentityServerConstants.StandardScopes.Profile
                //    }
                //},
	            #endregion
                #region Hybrid Flow
                new Client{
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Hybrid,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = { "http://localhost:5002/signin-oidc" }, // Redirecionamento após o login.                    
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" }, // Redirecionamento após o logout.

                    RequireConsent = false, // Habilita / desabilita o consentimento de escopos após o login.

                    // Escopos que serão permitidos no access_token.
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    },

                    AllowOfflineAccess = true, // Habilita o escopo offline_access, que permite requerer tokens de longa duração para o acesso da API
                },
                #endregion

                // WebSite SPA
                #region Authentication Code Flow     
                new Client
                {
                    ClientId = "js",
                    ClientName = "JavaScript Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =           { "http://localhost:5003/callback.html" },
                    PostLogoutRedirectUris = { "http://localhost:5003/index.html" },
                    AllowedCorsOrigins =     { "http://localhost:5003" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    }
                }
                #endregion
            };
        }

        // Registra os usuários que serão utilizados para autenticação.
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "Alice",
                    Password = "password",

                    Claims = new []
                    {
                        new Claim("name", "Alice Cooper"),
                        new Claim("website", "https://www.conectcar.com")
                    }
                },

                new TestUser
                {
                    SubjectId = "2",
                    Username = "Bob",
                    Password = "password",

                    Claims = new []
                    {
                        new Claim("name", "Bob Burnquist"),
                        new Claim("website", "https://www.conectcar.com")
                    }
                }
            };
        }
    }
}