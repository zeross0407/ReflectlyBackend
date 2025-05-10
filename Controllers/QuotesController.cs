using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reflectly.Entity;
using Reflectly.Service;
using Reflectly.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace Reflectly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {



        private readonly IConfiguration _configuration;
        private readonly Account_Service _Account_Service;
        private readonly TokenService _Token_Service;
        private readonly Quote_Service  _Quote_Service;

        public QuotesController(IConfiguration configuration,
                                Account_Service _Service,
                                TokenService Token_Service,
                                Quote_Service Quote_Service)
        {
            _configuration = configuration;
            _Account_Service = _Service;
            _Token_Service = Token_Service;
            _Quote_Service = Quote_Service;
        }

        public class DTT {

            public string q { get; set; }
            public string a { get; set; }
            public string c { get; set; }
            public string h { get; set; }

        }


        // POST: api/quotes
        [HttpPost("a")]
        public IActionResult PostQuotes([FromBody] List<DTT> ip)
        {
            //List<DTT> l = JsonConvert.DeserializeObject<List<DTT>>(ip);
            List<Quote> l = new List<Quote>();
            for(int i = 0; i <ip.Count; i++)
            {
                var quote = new Quote
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    title = ip[i].q,
                    author = ip[i].a,
                    categoryid = "651234567890123456789000"
                };
                l.Add(quote);


            }

            return Ok(JsonConvert.SerializeObject(l));
        }


        // POST: api/quotes
        [HttpPost("b")]
        public IActionResult get([FromBody] List<Quote> ip)
        {
            List<string> l = new List<string>();
            for (int i = 0; i < ip.Count; i++)
            {
                l.Add(ip[i].Id);

            }

            return Ok(JsonConvert.SerializeObject(l));
        }









        //[Authorize]
        [HttpGet("wallpaper")]
        public IActionResult GetWallpaper(string mediaId)
        {
            if (!System.IO.File.Exists(@"E:\Wall\" + mediaId + ".webp"))
            {
                return NotFound();
            }
            return PhysicalFile(@"E:\Wall\" + mediaId + ".webp", "image/jpeg");
        }


        [Authorize]
        [HttpGet("Quotes")]
        public async Task<IActionResult> GetQuotes(int type)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (type == 0)
                    return Ok(JsonConvert.SerializeObject(await _Quote_Service.GetAllAsync()));
                if (type == 1)
                {
                    return Ok(JsonConvert.SerializeObject(await _Quote_Service.GetAll_Hearted_Quotes_Async(userId)));
                }

                var rs = await _Quote_Service.Getby_CategoryID_Async(type);
                return Ok(JsonConvert.SerializeObject(rs));
            }
            catch(Exception e)
            {
                return BadRequest();
            }

        }

        [Authorize]
        [HttpGet("Heart")]
        public async Task<IActionResult> GetHeart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rs = await _Quote_Service.GetAll_Hearted_Async(userId);
            return Ok(JsonConvert.SerializeObject(rs));
        }


        [Authorize]
        [HttpGet("new_quotes")]
        public async Task<IActionResult> GetNewQuotes(int number)
        {
            var rs = await _Quote_Service.GetAllAsync();

            rs.Sort((a, b) => (new ObjectId(b.Id)).CreationTime.CompareTo((new ObjectId(a.Id)).CreationTime));

            var newestQuotes = rs.Take(number).ToList();

            return Ok(JsonConvert.SerializeObject(newestQuotes));
        }




    }

}
