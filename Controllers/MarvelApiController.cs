using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NetCoreMarvelApi.Controllers
{
    [ApiController]
    [Route("v1/public/")]
    public class MarvelApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MarvelApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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
        async Task<IActionResult> GetMarvelRequest(string url)
        {
            var apikey = _configuration.GetSection("MarvelApi").GetValue<string>("apikey");
            var privatekey = _configuration.GetSection("MarvelApi").GetValue<string>("privatekey");
            var ts = new DateTime().ToString();
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, ts + privatekey + apikey);

                var client = new HttpClient();
                var req = await client.GetAsync($"https://gateway.marvel.com:443/v1/public{url.Replace("#hash#", $"apikey={apikey}&hash={hash}&ts={ts}")}");
                var res = await req.Content.ReadAsStringAsync();
                return Content(res, "application/json");
            }
        }

        [ResponseCache(Duration = 1000 * 60 * 60, VaryByQueryKeys = new string[] { "offset", "nameStartsWith" })]
        [HttpGet("characters")]
        public async Task<IActionResult> Characters(int offset = 0, string nameStartsWith = "")
        {
            string url = $"/characters?#hash#&offset={offset}";
            if (!string.IsNullOrEmpty(nameStartsWith))
                url += $"&nameStartsWith={nameStartsWith}";
            return await GetMarvelRequest(url);
        }
        [ResponseCache(Duration = 1000 * 60 * 60, VaryByQueryKeys = new string[] { "characterId" })]
        [HttpGet("characters/{characterId}/comics")]
        public async Task<IActionResult> Comics(int characterId)
        {
            string url = $"/characters/{characterId}/comics?#hash#";
            return await GetMarvelRequest(url);
        }

        [ResponseCache(Duration = 1000 * 60 * 60, VaryByQueryKeys = new string[] { "offset", "titleStartsWith" })]
        [HttpGet("comics")]
        public async Task<IActionResult> Comics(int offset = 0, string titleStartsWith = "")
        {
            string url = $"/comics?#hash#&offset={offset}";
            if (!string.IsNullOrEmpty(titleStartsWith))
                url += $"&titleStartsWith={titleStartsWith}";
            return await GetMarvelRequest(url);
        }
        [ResponseCache(Duration = 1000 * 60 * 60, VaryByQueryKeys = new string[] { "comicId" })]
        [HttpGet("comics/{comicId}/characters")]
        public async Task<IActionResult> Characters(int comicId)
        {
            string url = $"/comics/{comicId}/characters?#hash#";
            return await GetMarvelRequest(url);
        }
    }
}
