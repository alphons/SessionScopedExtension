using Microsoft.AspNetCore.Mvc;
using SessionScopedTest.Services;

namespace SessionScopedTest.Controllers;

public class ExampleController(SessionScopedFactory<ExampleService> factory) : ControllerBase
{
	public async Task<IActionResult> Index()
	{
		HttpContext.Session.SetString("Started", DateTime.Now.ToString());

		var result = await factory.Instance.SetNameAsync($"alphons {Guid.NewGuid()}");

		return Ok("name is set, check url /Example/WhatsYourName");
	}
	public async Task<IActionResult> WhatsYourName()
	{
		var name = await factory.Instance.GetNameAsync();

		return Ok(name);
	}

}
