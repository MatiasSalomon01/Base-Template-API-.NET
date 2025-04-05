using Microsoft.AspNetCore.Mvc;

namespace WebApi.Common;

[Route(Constants.ProjectName + "/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
}