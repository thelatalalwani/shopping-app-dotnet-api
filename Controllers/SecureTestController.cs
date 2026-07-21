using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ShoppingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecureTestController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("public")]
    public ActionResult PublicEndpoint()
    {
        return Ok(new
        {
            message = "Anyone can access this endpoint."
        });
    }

    [Authorize]
    [HttpGet("authenticated")]
    public ActionResult AuthenticatedEndpoint()
    {
        var name =
            User.FindFirstValue(ClaimTypes.Name);

        return Ok(new
        {
            message =
                $"Hello {name}. Your JWT is valid."
        });
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("customer")]
    public ActionResult CustomerEndpoint()
    {
        return Ok(new
        {
            message =
                "Only customers can access this endpoint."
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public ActionResult AdminEndpoint()
    {
        return Ok(new
        {
            message =
                "Only administrators can access this endpoint."
        });
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("customer-or-admin")]
    public ActionResult CustomerOrAdminEndpoint()
    {
        return Ok(new
        {
            message =
                "Customers and administrators can access this."
        });
    }
}
