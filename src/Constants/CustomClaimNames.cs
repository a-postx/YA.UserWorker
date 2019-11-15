﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YA.TenantWorker.Constants
{
    public class CustomClaimNames
    {
        public const string client_id = nameof(client_id);
        public const string tenant_id = nameof(tenant_id);
        public const string nameidentifier = nameof(nameidentifier);
        public const string username = nameof(username);
        public const string name = nameof(name);
        public const string role = nameof(role);
        public const string useremail = nameof(useremail);
        public const string authsub = nameof(authsub);
        public const string authemail = nameof(authemail);
        public const string language = nameof(language);
        public const string scope = nameof(scope);
    }
}