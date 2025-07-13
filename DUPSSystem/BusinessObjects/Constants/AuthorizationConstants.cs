namespace BusinessObjects.Constants
{
	public static class Roles
	{
		public const string Guest = "Guest";
		public const string Member = "Member";
		public const string Staff = "Staff";
		public const string Consultant = "Consultant";
		public const string Manager = "Manager";
		public const string Admin = "Admin";

		public const string AuthenticatedRoles = "Member,Staff,Consultant,Manager,Admin";
		public const string ManagementRoles = "Staff,Manager,Admin";
		public const string SeniorRoles = "Manager,Admin";
		public const string AdminOnly = "Admin";
		public const string ConsultantRoles = "Consultant,Manager,Admin";

	}
}
