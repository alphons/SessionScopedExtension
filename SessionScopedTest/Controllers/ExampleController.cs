using Microsoft.AspNetCore.Mvc;
using SessionScopedTest.Services;

namespace SessionScopedTest.Controllers;

public class ExampleController(SessionScopedFactory<ExampleService> factory) : ControllerBase
{
	private readonly ExampleService exampleService = factory.Instance;
	public async Task<IActionResult> Index()
	{
		HttpContext.Session.SetString("Started", DateTime.Now.ToString());

		var result = await exampleService.SetNameAsync($"alphons {Guid.NewGuid()}");

		return Ok("name is set, check url /Example/WhatsYourName");
	}
	public async Task<IActionResult> WhatsYourName()
	{
		var name = await exampleService.GetNameAsync();

		return Ok(name);
	}

}
