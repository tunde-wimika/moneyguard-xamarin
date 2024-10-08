using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Wimika.MoneyGuard.Core.Android.REST;

namespace AndroidTestApp
{
    internal class GenericResult<T>
    {
        public T Data { get; set; }
        public bool IsError { get; set; }

        public string ErrorMessage { get; set; }
    }
    internal class LoginService : RestApiClientBase
    {
        //public LoginService() : base("https://bankservice.azurewebsites.net/")
        public LoginService() : base("https://moneyguardservice.azurewebsites.net/")
        {
        }

        public async Task<GenericResult<SessionResponse>> Session(string username, string password)
        {
            var result = await PostJsonAsync(
$"api/v1/account/auth/emails/signin",
UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {
    Password = password,
    Email = username,
})),
new Dictionary<string, string> {
                                {"accept", "application/json" }
 });

            Console.WriteLine(result);
            return JsonConvert.DeserializeObject<GenericResult<SessionResponse>>(result);
        }
    }
}