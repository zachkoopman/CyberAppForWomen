using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CyberApp_FIA.Services
{
    /// <summary>
    /// Tracks participant privacy preferences in a simple XML file.
    /// - New participants should be created with privacyOn = true by default.
    /// - Only University Admins from the same university should see full details.
    /// - Other roles see redacted labels when privacy is on.
    /// </summary>
    public class ParticipantPrivacyService
    {
        private readonly string _path;

        public ParticipantPrivacyService()
        {
            var ctx = HttpContext.Current ?? throw new InvalidOperationException("No HttpContext.");
            var appData = ctx.Server.MapPath("~/App_Data");
            Directory.CreateDirectory(appData);

            _path = Path.Combine(appData, "participantPrivacy.xml");
            EnsureStore();
        }

        private void EnsureStore()
        {
            if (File.Exists(_path)) return;

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("participants",
                    new XAttribute("version", "1")
                )
            );
            doc.Save(_path);
        }

        /// <summary>
        /// Ensure a participant has a privacy record.
        /// - If missing, create a new entry with privacyOn = true.
        /// - If present, keep their current privacy choice but update basic info.
        /// This helper can be called from any "create participant" flow.
        /// </summary>
        public void EnsureDefaultPrivacy(string participantId, string universityId, string email, string displayName)
        {
            if (string.IsNullOrWhiteSpace(participantId)) throw new ArgumentNullException(nameof(participantId));

            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) throw new InvalidOperationException("Invalid participantPrivacy.xml.");

            var node = root.Elements("participant")
                .FirstOrDefault(p => (string)p.Attribute("id") == participantId);

            var now = DateTime.UtcNow;

            if (node == null)
            {
                // Create a new entry with privacyOn = true by default.
                node = new XElement("participant",
                    new XAttribute("id", participantId),
                    new XAttribute("universityId", universityId ?? string.Empty),
                    new XAttribute("privacyOn", "true"),
                    new XElement("email", email ?? string.Empty),
                    new XElement("displayName", displayName ?? string.Empty),
                    new XElement("createdUtc", now.ToString("o")),
                    new XElement("updatedUtc", now.ToString("o"))
                );
                root.Add(node);

                // Optional audit entry so you can prove that privacy was defaulted.
                var audit = new AuditLogService();
                audit.AppendFromSession(
                    category: "Consent",
                    actionType: "ParticipantPrivacyDefaultedOn",
                    targetType: "ParticipantAccount",
                    targetId: participantId,
                    targetLabel: displayName ?? email ?? "Participant",
                    notes: "New participant created with privacy set to ON by default.",
                    meta: new Dictionary<string, string>
                    {
                        { "universityId", universityId ?? string.Empty },
                        { "email", email ?? string.Empty }
                    });
            }
            else
            {
                // Only update metadata; respect the existing privacyOn flag.
                node.SetAttributeValue("universityId", universityId ?? string.Empty);
                node.SetElementValue("email", email ?? string.Empty);
                node.SetElementValue("displayName", displayName ?? string.Empty);
                node.SetElementValue("updatedUtc", now.ToString("o"));
            }

            doc.Save(_path);
        }

        /// <summary>
        /// Check if the participant's privacy flag is on.
        /// If the participant is unknown, we treat them as private.
        /// </summary>
        public bool IsPrivacyOn(string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId)) return true;

            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) return true;

            var node = root.Elements("participant")
                .FirstOrDefault(p => (string)p.Attribute("id") == participantId);

            if (node == null)
            {
                return true;
            }

            var value = (string)node.Attribute("privacyOn") ?? "true";
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a safe name for display based on the viewer's role.
        /// - If privacy is on and viewer is NOT the UniversityAdmin from the same university
        ///   and NOT the participant themselves, we return a generic label.
        /// - Otherwise we return the participant's name.
        /// </summary>
        public string GetDisplayNameForViewer(
            string participantId,
            string viewerUserId,
            string viewerRole,
            string viewerUniversityId)
        {
            var info = GetParticipantInfo(participantId);

            // If we could not look up the participant, show a simple generic label.
            if (info == null)
            {
                return "Participant";
            }

            // Participant can always see their own details.
            if (!string.IsNullOrEmpty(viewerUserId) &&
                string.Equals(viewerUserId, participantId, StringComparison.OrdinalIgnoreCase))
            {
                return info.DisplayName;
            }

            var privacyOn = IsPrivacyOn(participantId);

            if (!privacyOn)
            {
                // If privacy is off, show full name to all admin roles.
                return info.DisplayName;
            }

            // Privacy is ON: only a UniversityAdmin from the same university
            // sees full details. Everyone else sees a redacted label.
            if (string.Equals(viewerRole, "UniversityAdmin", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(viewerUniversityId, info.UniversityId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return info.DisplayName;
            }

            // You can adjust this label to match your UI tone.
            return "Private participant";
        }

        /// <summary>
        /// Returns a safe email string for display (or empty) for non-UA views.
        /// </summary>
        public string GetEmailForViewer(
            string participantId,
            string viewerUserId,
            string viewerRole,
            string viewerUniversityId)
        {
            var info = GetParticipantInfo(participantId);
            if (info == null) return string.Empty;

            // Same rule as display name: only UA for same university sees full email.
            var privacyOn = IsPrivacyOn(participantId);

            if (!privacyOn)
            {
                return info.Email;
            }

            if (!string.IsNullOrEmpty(viewerUserId) &&
                string.Equals(viewerUserId, participantId, StringComparison.OrdinalIgnoreCase))
            {
                return info.Email;
            }

            if (string.Equals(viewerRole, "UniversityAdmin", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(viewerUniversityId, info.UniversityId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return info.Email;
            }

            // Return empty so helpers and other roles do not see contact details.
            return string.Empty;
        }

        private ParticipantPrivacyInfo GetParticipantInfo(string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId)) return null;

            var doc = XDocument.Load(_path);
            var root = doc.Root;
            if (root == null) return null;

            var node = root.Elements("participant")
                .FirstOrDefault(p => (string)p.Attribute("id") == participantId);

            if (node == null) return null;

            return new ParticipantPrivacyInfo
            {
                ParticipantId = participantId,
                UniversityId = (string)node.Attribute("universityId") ?? string.Empty,
                Email = (string)node.Element("email") ?? string.Empty,
                DisplayName = (string)node.Element("displayName") ?? "Participant"
            };
        }

        private sealed class ParticipantPrivacyInfo
        {
            public string ParticipantId { get; set; }
            public string UniversityId { get; set; }
            public string Email { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
