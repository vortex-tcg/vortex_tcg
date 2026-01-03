using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Api.Logs.GameLog.DTOs
{
	public class GameLogDTO
	{
		public Guid Id { get; set; }
		public string Label { get; set; } = default!;
		public int TurnNumber { get; set; }
		public Guid? UserId { get; set; }
		public List<Guid>? ActionIds { get; set; }
	}

	public class GameLogCreateDTO
	{
		public string Label { get; set; } = default!;
		public int TurnNumber { get; set; }
		public Guid? UserId { get; set; }
		public List<Guid>? ActionIds { get; set; }
	}
}
