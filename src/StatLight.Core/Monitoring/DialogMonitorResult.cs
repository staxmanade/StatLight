namespace StatLight.Core.Monitoring
{
	internal class DialogMonitorResult
	{
		public bool WasActionTaken { get; set; }
		public string Message { get; set; }

		public static DialogMonitorResult NoSlapdownAction()
		{
			return new DialogMonitorResult { Message = string.Empty, WasActionTaken = false };
		}
	}
}