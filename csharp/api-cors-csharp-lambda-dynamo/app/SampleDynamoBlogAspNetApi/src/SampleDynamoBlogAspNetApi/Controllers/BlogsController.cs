using Microsoft.AspNetCore.Mvc;
using SampleDynamoBlogAspNetApi.Models;
using SampleDynamoBlogAspNetApi.Repositories;

namespace SampleDynamoBlogAspNetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BlogsController : ControllerBase
{
  private readonly IBlogRepository _blogRepository;
  private ILogger<BlogsController> _logger;

  public BlogsController(ILogger<BlogsController> logger, IBlogRepository blogRepository)
  {
    _logger = logger;
    _blogRepository = blogRepository;
  }

  // GET blogs/
  [HttpGet("")]
  public async Task<List<Blog>> GetBlogs()
  {
    return await this._blogRepository.GetAll();
  }

  // GET blogs/abcdefg123456
  [HttpGet("{Id}")]
  public async Task<Blog> GetBlog(string Id)
  {
    return await this._blogRepository.GetOne(Id);
  }

  // POST blogs/abcdefg123456
  [HttpPost("")]
  public async Task<Blog> CreateBlog(Blog blog)
  {
    return await this._blogRepository.Create(blog);
  }

  // PATCH blogs/abcdefg123456
  [HttpPatch("{Id}")]
  public async Task<Blog> UpdateBlog(string Id, Blog blog)
  {
    return await this._blogRepository.Update(Id, blog);
  }

  // DELETE blogs/abcdefg123456
  [HttpDelete("{Id}")]
  public async Task<string> DeleteBlog(string Id)
  {
    return await this._blogRepository.Delete(Id);
  }
}
