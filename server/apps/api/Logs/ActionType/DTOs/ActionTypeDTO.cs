
using System;
using System.Collections.Generic;
using ActionTypeModel = VortexTCG.DataAccess.Models.ActionType;

namespace VortexTCG.Api.Logs.ActionType.DTOs
{
	public class ActionTypeDTO
	{
		public Guid Id { get; set; }
		public string ActionDescription { get; set; } = default!;
		public Guid GameLogId { get; set; }
		public Guid? ParentId { get; set; }
		public List<Guid>? ChildIds { get; set; }
	}

	public class ActionTypeCreateDTO
	{
		public string ActionDescription { get; set; } = default!;
		public Guid GameLogId { get; set; }
		public Guid? ParentId { get; set; }
	}
}
