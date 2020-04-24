using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
            IActionContextAccessor actionContextAccessor,
            IConfiguration configuration,
            ITenantWorkerDbContext dbContext)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private readonly ILogger<AuthenticateCommand> _log;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IConfiguration _config;
        private readonly ITenantWorkerDbContext _dbContext;

        public async Task<IActionResult> ExecuteAsync(CredentialsSm credentials, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(credentials.Username) || string.IsNullOrEmpty(credentials.Password))
            {
                return new BadRequestResult();
            }

            AppSecrets secrets = _config.Get<AppSecrets>();

            List<User> users = await _dbContext
                .GetEntitiesFromAllTenantsWithTenantAsync<User>(u => u.Username == credentials.Username && u.Password == credentials.Password, cancellationToken);

            if (users.Count == 0)
            {
                _log.LogInformation("Username and/or password for {Username} is invalid.", credentials.Username);
                return new BadRequestObjectResult("Username and/or password is invalid.");
            }

            if (users.Count > 1)
            {
                _log.LogInformation("More than one user found in the system.", credentials.Username);
                throw new Exception("More than one user found in the system.");
            }

            User user = users.First();

            Claim[] claims = new[]
            {
                new Claim(CustomClaimNames.client_id, "web_app"),
                new Claim(CustomClaimNames.tid, user.Tenant.TenantID.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                //Ocelot doesn't transform "sub" claim
                new Claim(CustomClaimNames.authsub, user.Username),
                new Claim(CustomClaimNames.authemail, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(CustomClaimNames.name, user.FirstName + " " + user.LastName),
                new Claim(CustomClaimNames.role, user.Role),
                new Claim(CustomClaimNames.language, "ru"),
                new Claim(CustomClaimNames.scope, "mySuperServiceScope"),
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.JwtSigningKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            DateTime expiry = DateTime.Now.AddDays(Convert.ToInt32(360));

            JwtSecurityToken token = new JwtSecurityToken("https://localhost:7453", "YATenantWorker", claims, DateTime.Now, expiry, creds);

            string wToken = new JwtSecurityTokenHandler().WriteToken(token);

            _log.LogInformation("User {Username} authenticated successfully.", credentials.Username);

            return new OkObjectResult(new TokenVm
            {
                access_token = wToken,
                expires_in = 31104000,
                token_type = "Bearer",
                scope = "mySuperServiceScope"
            });
        }
    }
}
