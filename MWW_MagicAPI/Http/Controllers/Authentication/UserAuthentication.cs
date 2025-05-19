using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MWW_MagicAPI.Services;

namespace MWW_MagicAPI.Http.Controllers.Authentication;
[Route("[controller]")]
public class UserAuthentication : Controller
{
    private readonly ILogger<UserAuthentication> _logger;
    private readonly IAuthService _authService;

    public UserAuthentication(ILogger<UserAuthentication> logger,
        IAuthService authService)
    {
        _authService = authService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error!");
    }
}