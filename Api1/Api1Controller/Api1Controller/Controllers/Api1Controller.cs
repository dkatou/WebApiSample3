using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Api1.Api1Model.Models;
using Api1.Api1Model.Data;
using Api1.Api1Logic.Logics;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.UriParser;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;

namespace Api1Controller.Controllers
{
    [ApiVersion("1")]
    [ODataRoutePrefix("Api1")]// Edmに対応するEntityの名前を設定する
    public class Api1Controller : ODataController
    {
        private readonly Api1Context _context;

        private readonly BlogLogic _logic;

        public Api1Controller(Api1Context context)
        {
            _context = context;
            _logic = new BlogLogic(context);
        }

        [ODataRoute]// ODataのルートを有効にする
        [EnableQuery]// ODataのクエリを有効にする
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlog()
        {
            return await _logic.GetBlog();
        }

        [HttpGet("{id}")]
        [ODataRoute("{id}")]
        [EnableQuery]
        [Produces("application/json")]
        public async Task<ActionResult<Blog>> GetBlog(int id)
        {
            string param1 = Request.Query["param1"];
            bool param2;
            if(!bool.TryParse(Request.Query["param2"], out param2))
            {
                param2 = true;
            };

            var blog = await _logic.GetBlog(id);

            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }

        [ODataRoute("{id}")]
        public async Task<IActionResult> PutBlog(int id, [FromBody] Blog blog)
        {
            if (id != blog.BlogId)
            {
                return BadRequest();
            }

            DateTime tsStamp = DateTime.Now;
            string usStamp = "testUser";
            string asStamp = "testApp";

            await _logic.PutBlog(blog, tsStamp, usStamp, asStamp);

            try
            {
                _context.Entry(blog).State = EntityState.Modified;
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!_logic.BlogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return NoContent();
        }

        [ODataRoute]
        [ProducesResponseType(typeof(Blog), StatusCodes.Status200OK)]
        public async Task<ActionResult<Blog>> Post([FromBody] Blog blog)
        {
            DateTime tsStamp = DateTime.Now;
            string usStamp = "testUser";
            string asStamp = "testApp";

            await _logic.PostBlog(blog, tsStamp, usStamp, asStamp);

            return CreatedAtAction(nameof(GetBlog), new { id = blog.BlogId }, blog);
        }

        [HttpDelete("{id}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<ActionResult<Blog>> DeleteBlog(int id)
        {
            if (!_logic.BlogExists(id))
            {
                return NotFound();
            }

            return await _logic.DeleteBlog(id);
        }
    }
}
