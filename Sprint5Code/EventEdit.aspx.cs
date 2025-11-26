using System;
using System.Globalization;
using System.Web.UI;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// University Admin event editing page.
    /// - Uses EventCatalogService to update event details.
    /// - Allows soft-deleting events so they disappear from participant catalogs.
    /// - All changes are logged via AuditLogService (category="Catalog").
    /// </summary>
    public partial class UniversityAdminEventEdit : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var id = Request.QueryString["id"];
                if (!string.IsNullOrWhiteSpace(id))
                {
                    LoadEvent(id);
                }
                else
                {
                    StatusLabel.CssClass = "status error";
                    StatusLabel.Text = "No event id specified.";
                }
            }
        }

        private void LoadEvent(string id)
        {
            var svc = new EventCatalogService();
            var ev = svc.GetById(id);
            if (ev == null)
            {
                StatusLabel.CssClass = "status error";
                StatusLabel.Text = "Event not found.";
                return;
            }

            EventId.Value = ev.Id;
            UniversityId.Value = ev.UniversityId;
            Title.Text = ev.Title;
            Room.Text = ev.Room;
            StartTime.Text = ev.StartUtc.ToString("yyyy-MM-dd HH:mm");
            EndTime.Text = ev.EndUtc.ToString("yyyy-MM-dd HH:mm");
            IsEnabled.Checked = ev.IsEnabled;
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var svc = new EventCatalogService();
                var ev = svc.GetById(EventId.Value);
                if (ev == null)
                {
                    StatusLabel.CssClass = "status error";
                    StatusLabel.Text = "Event not found.";
                    return;
                }

                ev.Title = Title.Text;
                ev.Room = Room.Text;
                ev.IsEnabled = IsEnabled.Checked;

                if (DateTime.TryParseExact(StartTime.Text.Trim(), "yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var start))
                {
                    ev.StartUtc = start;
                }

                if (DateTime.TryParseExact(EndTime.Text.Trim(), "yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var end))
                {
                    ev.EndUtc = end;
                }

                svc.Save(ev);

                StatusLabel.CssClass = "status";
                StatusLabel.Text = "Event updated. Changes will appear in your catalog.";
            }
            catch (Exception ex)
            {
                StatusLabel.CssClass = "status error";
                StatusLabel.Text = ex.Message;
            }
        }

        protected void BtnDelete_Click(object sender, EventArgs e)
        {
            var svc = new EventCatalogService();
            svc.SoftDelete(EventId.Value, DeleteReason.Text);
            StatusLabel.CssClass = "status";
            StatusLabel.Text = "Event deleted. It will no longer appear in participant catalogs.";
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            // You can redirect to your existing University Admin catalog page.
            Response.Redirect("~/Account/UniversityAdmin/Default.aspx");
        }
    }
}
