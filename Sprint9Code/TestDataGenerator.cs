using System;
using System.Xml.Linq;
using System.Linq;

public class TestDataGenerator
{
    public static void AddFakeUsers(string path, int count)
    {
        var doc = XDocument.Load(path);

        for (int i = 0; i < count; i++)
        {
            var id = Guid.NewGuid().ToString("N");

            var user = new XElement("user",
                new XAttribute("id", id),
                new XAttribute("role", "Participant"),

                new XElement("firstName", "Test"),
                new XElement("lastName", $"User{i}"),
                new XElement("email", $"test{i}@example.com"),
                new XElement("university", "Arizona State University"),

                new XElement("passwordHash", ""),
                new XElement("passwordSalt", ""),
                new XElement("createdAt", DateTime.UtcNow.ToString("o")),
                new XElement("consentAcceptedAt", DateTime.UtcNow.ToString("o")),
                new XElement("consentIp", "::1")
            );

            doc.Root.Add(user);
        }

        doc.Save(path);
    }

    public static void AddFakeCourses(string path, int count)
    {
        var doc = XDocument.Load(path);

        for (int i = 0; i < count; i++)
        {
            var course = new XElement("course",
                new XAttribute("id", Guid.NewGuid().ToString("N")),
                new XAttribute("status", "Published"),
                new XAttribute("createdAt", DateTime.UtcNow.ToString("o")),
                new XAttribute("createdBy", "test@fia.org"),

                new XElement("title", $"Test Course {i}"),
                new XElement("summary", "Auto-generated test course."),
                new XElement("duration", "10 Min"),
                new XElement("externalLink", ""),

                new XElement("tags",
                    new XElement("tag", "test"),
                    new XElement("tag", "auto")
                ),

                new XElement("requiredRules",
                    new XElement("rule", new XAttribute("id", "Quiz"))
                ),

                new XElement("prerequisites"),
                new XElement("startTime"),
                new XElement("endTime"),
                new XElement("maxParticipants")
            );

            doc.Root.Add(course);
        }

        doc.Save(path);
    }
}