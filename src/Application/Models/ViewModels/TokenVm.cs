﻿namespace YA.UserWorker.Application.Models.ViewModels
{
    public class TokenVm
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }
}
