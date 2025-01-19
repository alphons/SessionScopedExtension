using Microsoft.AspNetCore.Mvc;
using SessionScopedTest.Services;

namespace SessionScopedTest.Controllers;

public class ExampleController(SessionScopedFactory<ExampleService> factory) : ControllerBase
{
	public async Task<IActionResult> Index()
	{
		HttpContext.Session.SetString("Started", DateTime.Now.ToString());

		var result = await factory.Instance.CalcAsync(123, 456);

		return Ok(result);
	}
}
