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

  // GET calculator/substract/4/2/
  [HttpGet("subtract/{x}/{y}")]
  public int Subtract(int x, int y)
  {
    _logger.LogInformation($"{x} subtract {y} is {x - y}");
    return x - y;
  }

  // GET calculator/multiply/4/2/
  [HttpGet("multiply/{x}/{y}")]
  public int Multiply(int x, int y)
  {
    _logger.LogInformation($"{x} multiply {y} is {x * y}");
    return x * y;
  }

  // GET calculator/divide/4/2/
  [HttpGet("divide/{x}/{y}")]
  public int Divide(int x, int y)
  {
    _logger.LogInformation($"{x} divide {y} is {x / y}");
    return x / y;
  }
}
