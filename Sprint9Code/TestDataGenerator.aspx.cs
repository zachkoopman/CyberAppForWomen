using System;
using App_Code.Services; // make sure TestDataGenerator.cs is here

namespace Account.Participant
{
    public partial class TestDataGenerator : System.Web.UI.Page
    {
        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            int count = 0;
            if (int.TryParse(txtUserCount.Text, out count))
            {
                var generator = new TestDataGeneratorService(); // your service class
                var result = generator.GenerateUsers(count); // returns a string or HTML
                ltOutput.Text = result;
            }
            else
            {
                ltOutput.Text = "<span style='color:red'>Enter a valid number!</span>";
            }
        }
    }
}