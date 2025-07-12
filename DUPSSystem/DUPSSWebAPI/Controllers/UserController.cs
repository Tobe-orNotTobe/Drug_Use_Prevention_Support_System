using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	[Authorize(Roles = "Admin")]
	public class UserController : ODataController
	{
		private readonly IUserService _service;

		public UserController(IUserService service)
		{
			_service = service;
		}

		// GET: odata/Users
		[EnableQuery(PageSize = 20)]
		public IActionResult Get()
		{
			return Ok(_service.GetAccounts().AsQueryable());
		}

		// GET: odata/Users(5)
		[EnableQuery]
		public IActionResult Get([FromODataUri] int key)
		{
			var account = _service.GetAccountById(key);
			if (account == null)
			{
				return NotFound();
			}
			return Ok(account);
		}

		// POST: odata/Users
		[EnableQuery]
		public IActionResult Post([FromBody] User User)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				_service.SaveAccount(User);
				return Created(User);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		// PUT: odata/Users(5)
		[EnableQuery]
		public IActionResult Put([FromODataUri] int key, [FromBody] User User)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (key != User.UserId)
			{
				return BadRequest("Key mismatch");
			}

			var existingAccount = _service.GetAccountById(key);
			if (existingAccount == null)
			{
				return NotFound();
			}

			try
			{
				_service.UpdateAccount(User);
				return Updated(User);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		// DELETE: odata/Users(5)
		public IActionResult Delete([FromODataUri] int key)
		{
			var User = _service.GetAccountById(key);
			if (User == null)
			{
				return NotFound();
			}

			try
			{
				_service.DeleteAccount(User);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
