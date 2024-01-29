using Microsoft.AspNetCore.Identity;

    namespace WebApplication3.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName {get; set;}
        public string CreditCard {get; set;}
        public string Photo { get; set;}
        public string Gender { get; set;}
        public string AboutMe { get; set;}
        public string DeliveryAddress { get; set;}
        public string TwoFactorCode { get; set;}
        public DateTime PasswordChangeMin { get; set;}
        public DateTime PasswordChangeLimit { get; set;}
    }
}