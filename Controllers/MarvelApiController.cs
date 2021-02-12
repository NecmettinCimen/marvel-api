using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NetCoreMarvelApi.Controllers
{
    [ApiController]
    [Route("v1/public/")]
    public class MarvelApiController : ControllerBase
    {
        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        [ResponseCache(Duration = 1000 * 60 * 60, VaryByQueryKeys=new string[] { "offset" } )]
        [HttpGet("characters")]
        public async Task<IActionResult> Characters(int offset=0)
        {
            var apikey = "6d2d8b84abbd3048de5ea3d6ff5b8dfe";
            var privatekey = "533086743316e2d16877de7dcaa25a9379c3b503";
            var ts = new DateTime().ToString();
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, ts + privatekey + apikey);

                var client = new HttpClient();
                var req = await client.GetAsync($"https://gateway.marvel.com:443/v1/public/characters?apikey={apikey}&hash={hash}&ts={ts}&offset={offset}");
                var res = await req.Content.ReadAsStringAsync();
                return Content(res, "application/json");
            }
        }
    }
}
