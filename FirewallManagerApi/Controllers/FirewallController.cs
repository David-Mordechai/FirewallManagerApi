using FirewallManagerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FirewallManagerApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FirewallController : ControllerBase
{
    private readonly IFirewallManagerService _firewallManagerService;

    public FirewallController(IFirewallManagerService firewallManagerService)
    {
        _firewallManagerService = firewallManagerService;
    }

    [HttpPost(Name = "AddRule")]
    public void AddRule(string[] remoteIPs)
    {
        _firewallManagerService.DeleteRule();
        _firewallManagerService.AddRule(remoteIPs);
    }

    [HttpDelete(Name = "DeleteRule")]
    public void DeleteRule()
    {
        _firewallManagerService.DeleteRule();
    }
}