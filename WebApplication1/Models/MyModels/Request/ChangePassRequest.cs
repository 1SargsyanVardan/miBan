﻿namespace WebApplication1.Models.MyModels.Request
{
    public class ChangePassRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
