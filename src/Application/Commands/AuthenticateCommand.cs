﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YA.TenantWorker.Application.Interfaces;
using YA.TenantWorker.Application.Models.SaveModels;
using YA.TenantWorker.Application.Models.ViewModels;
using YA.TenantWorker.Constants;
using YA.TenantWorker.Core.Entities;

namespace YA.TenantWorker.Application.Commands
{
    public class AuthenticateCommand : IAuthenticateCommand
    {
        public AuthenticateCommand(ILogger<AuthenticateCommand> logger,
            IConfiguration configuration,
            ITenantWorkerDbContext workerDbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dbContext = workerDbContext ?? throw new ArgumentNullException(nameof(workerDbContext));
        }

        private readonly ILogger<AuthenticateCommand> _log;
        private readonly IConfiguration _config;
        private readonly ITenantWorkerDbContext _dbContext;

        public async Task<IActionResult> ExecuteAsync(CredentialsSm credentials, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(credentials.Username) || string.IsNullOrEmpty(credentials.Password))
            {
                return new BadRequestResult();
            }

            KeyVaultSecrets secrets = _config.Get<KeyVaultSecrets>();

            using (_log.BeginScopeWith((Logs.Username, credentials.Username)))
            {
                User user = await _dbContext.GetEntityWithTenantAsync<User>(u => u.Username == credentials.Username && u.Password == credentials.Password, cancellationToken);

                if (user != null)
                {
                    Claim[] claims = new[]
                    {
                        new Claim("client_id", "marklimit"),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                        new Claim("user_id", user.UserID.ToString()),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim("name", user.FirstName + " " + user.LastName),
                        new Claim("role", "Administrator"),
                        new Claim("tenant_id", user.Tenant.TenantID.ToString()),
                        new Claim("language", "ru"),
                        new Claim("scope", "identityServerYATenantWorker")
                    };

                    SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.JwtSigningKey));
                    SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    DateTime expiry = DateTime.Now.AddDays(Convert.ToInt32(360));
                    JwtSecurityToken token = new JwtSecurityToken(
                        "https://localhost:7453",
                        "YATenantWorker",
                        claims,
                        DateTime.Now,
                        expires: expiry,
                        signingCredentials: creds
                    );

                    string wToken = new JwtSecurityTokenHandler().WriteToken(token);

                    return new OkObjectResult(new TokenVm
                    {
                        access_token = wToken,
                        expires_in = 31104000,
                        token_type = "Bearer",
                        scope = "identityServerYATenantWorker"
                    });
                }
                else
                {
                    return new BadRequestObjectResult("Username and/or password is invalid.");
                }
            }
        }
    }
}
