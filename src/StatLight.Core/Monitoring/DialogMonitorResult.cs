namespace StatLight.Core.Monitoring
{
    public class DialogMonitorResult
	{
		public bool WasActionTaken { get; set; }
		public string Message { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Slapdown")]
        public static DialogMonitorResult NoSlapdownAction()
		{
			return new DialogMonitorResult { Message = string.Empty, WasActionTaken = false };
		}
	}
}