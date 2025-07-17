using Azure.Core;
using BusinessObjects;
using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories.Interfaces;
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	public class UsersController : ODataController
	{
		private readonly IUserService _service;

		public UsersController(IUserService service)
		{
			_service = service;
		}

		[EnableQuery(PageSize = 20)]
		public IActionResult Get()
		{
			try
			{
				var users = _service.GetAccounts();
				return Ok(users.AsQueryable());
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[EnableQuery]
		public IActionResult Get([FromODataUri] int key)
		{
			try
			{
				var account = _service.GetAccountById(key);
				if (account == null)
				{
					return NotFound();
				}
				return Ok(account);
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[EnableQuery]
		public IActionResult Post([FromBody] CreateUserRequest User)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				_service.SaveAccount(User);
				return Ok();
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

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
				return BadRequest(new { error = ex.Message });
			}
		}

		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var User = _service.GetAccountById(key);
				if (User == null)
				{
					return NotFound();
				}

				_service.DeleteAccount(User);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}
