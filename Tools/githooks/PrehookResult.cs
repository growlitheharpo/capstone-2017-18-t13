namespace precommit
{
	public struct PrehookResult
	{
		public string Message { get; }
		public bool Succeeded { get; set; }
		public bool Failed => !Succeeded;

		public PrehookResult(bool s, string m = "")
		{
			Succeeded = s;
			Message = m;
		}
	}
}
