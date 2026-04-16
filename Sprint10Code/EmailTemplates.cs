public static class EmailTemplates
{
	public static string ParticipantReminder(string name, string courseTitle, string sessionTime)
	{
		return $@"
Hi {name},

This is a reminder from Feminine Intelligence Agency that you have an upcoming microcourse session.

Course: {courseTitle}
Time: {sessionTime}

Thank you for enrolling, and we hope to see you soon!

If you have any questions, contact your Helper.

– FIA Team
";
	}

	public static string HelperReminder(string name, string courseTitle, string sessionTime)
	{
		return $@"
Hi {name},

This is a reminder from Feminine Intelligence Agency that you are scheduled to lead an upcoming microcourse.

Course: {courseTitle}
Time: {sessionTime}

Please be prepared to start on time and assist participants as needed.

If you believe there may be an error in this information, reach out to an Admininstrator.

– FIA Team
";
	}
}