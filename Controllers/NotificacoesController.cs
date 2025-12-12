using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VigiLant.Controllers;

[Authorize]
public class NotificacoesController : Controller
{
    private readonly ILogger<NotificacoesController> _logger;

    public NotificacoesController(ILogger<NotificacoesController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

}

