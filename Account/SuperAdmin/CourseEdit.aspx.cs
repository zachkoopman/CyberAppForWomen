using System;
using System.Web.UI;
using CyberApp_FIA.Services;

namespace CyberApp_FIA.Account
{
    /// <summary>
    /// Simple Super Admin course editor page.
    /// Uses CourseCatalogService to create or update courses and
    /// logs all changes via the AuditLogService.
    /// </summary>
    public partial class SuperAdminCourseEdit : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var id = Request.QueryString["id"];
                if (!string.IsNullOrWhiteSpace(id))
                {
                    LoadCourse(id);
                }
            }
        }

        private void LoadCourse(string id)
        {
            var svc = new CourseCatalogService();
            var course = svc.GetById(id);
            if (course == null)
            {
                StatusLabel.CssClass = "status error";
                StatusLabel.Text = "Course not found.";
                return;
            }

            CourseId.Value = course.Id;
            OwnerUniversity.Text = course.OwnerUniversity;
            ShortCode.Text = course.ShortCode;
            Title.Text = course.Title;
            Description.Text = course.Description;
            IsPublished.Checked = course.IsPublished;
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var svc = new CourseCatalogService();
                var record = new CourseRecord
                {
                    Id = CourseId.Value,
                    OwnerUniversity = OwnerUniversity.Text,
                    ShortCode = ShortCode.Text,
                    Title = Title.Text,
                    Description = Description.Text,
                    IsPublished = IsPublished.Checked
                };

                var saved = svc.Save(record);

                CourseId.Value = saved.Id;
                StatusLabel.CssClass = "status";
                StatusLabel.Text = "Course saved. Changes will appear in the catalog view.";
            }
            catch (Exception ex)
            {
                // Simple error message so Super Admins know why saving failed.
                StatusLabel.CssClass = "status error";
                StatusLabel.Text = ex.Message;
            }
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            // You can swap this for a redirect to your existing catalog page.
            Response.Redirect("~/Account/SuperAdmin/Default.aspx");
        }
    }
}
