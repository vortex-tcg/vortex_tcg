using System;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Api.User.DTOs
{
	public class UserDTO
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = default!;
		public string LastName { get; set; } = default!;
		public string Username { get; set; } = default!;
		public string Email { get; set; } = default!;
		public int CurrencyQuantity { get; set; }
		public string Language { get; set; } = default!;
		public Role Role { get; set; }
		public UserStatus Status { get; set; }
		public Guid? RankId { get; set; }
	}

	public class UserCreateDTO
	{
		public string FirstName { get; set; } = default!;
		public string LastName { get; set; } = default!;
		public string Username { get; set; } = default!;
		public string Password { get; set; } = default!;
		public string Email { get; set; } = default!;
		public int CurrencyQuantity { get; set; }
		public string Language { get; set; } = default!;
		public Role Role { get; set; }
		public UserStatus Status { get; set; }
		public Guid? RankId { get; set; }
	}
}
