using System;
using Autofac.Integration.SharePoint.Demo.Playground;
using Autofac.Integration.SharePoint.Forms;
using Microsoft.SharePoint.WebControls;

namespace Autofac.Integration.SharePoint.Demo.Layouts.AutofacPlayground
{
    [InjectProperties]
    public partial class Play : LayoutsPageBase, IPlaygroundView
    {
        private PlaygroundPresenter _presenter;
        public PlaygroundPresenterFactory PresenterFactory { get; set; }

        // these are here just to test property injection
        public ILogger Logger { get; set; }
        public IPlayInterface PlayInterface { get; set; }
        public ISiteLevelDependency SiteLevelDependency { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _presenter = PresenterFactory(this);

            litComments.Text = string.Format(@"Autofac Initialization Time: {0}<br/>Autofac Config File: {1}",
                                             Application["_autofac_init"] ?? string.Empty,
                                             Application["_autofac_config"] ?? string.Empty);
            litComments.Text += string.Format(@"<br/>ILogger -> resolves to {0}", Logger.GetType().FullName);
            litComments.Text += string.Format(@"<br/>IPlayInterface -> resolves to {0}", PlayInterface.GetType().FullName);
            litComments.Text += string.Format(@"<br/>ISiteLevelDependency -> resolves to {0}", SiteLevelDependency.GetType().FullName);
        }

        public bool IsPostback
        {
            get { return Page.IsPostBack; }
        }

        protected void DoTest1(object sender, EventArgs e)
        {
            _presenter.TestPostbackWithPropertyInjection();
        }

        protected void DoTest2(object sender, EventArgs e)
        {
            _presenter.TestPostbackWithServiceLocator();
        }

        protected void DoTest3(object sender, EventArgs e)
        {
            _presenter.TestWithElevatedPrivileges();
        }

        protected void DoTest4(object sender, EventArgs e)
        {
            _presenter.TestWithHttpContextCurrentIsNull();
        }

        protected void DoTest5(object sender, EventArgs e)
        {
            // make the page async for this test
            this.AsyncMode = true;
            _presenter.TestInBackgroundThread();
        }

        public string SuccessMessage
        {
            get
            {
                if (this.lblMessage.ForeColor == System.Drawing.Color.Blue)
                    return this.lblMessage.Text;
                else
                    return string.Empty;
            }
            set
            {
                this.lblMessage.ForeColor = System.Drawing.Color.Blue;
                this.lblMessage.Text = value;
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (this.lblMessage.ForeColor == System.Drawing.Color.Red)
                    return this.lblMessage.Text;
                else
                    return string.Empty;
            }
            set
            {
                this.lblMessage.ForeColor = System.Drawing.Color.Red;
                this.lblMessage.Text = value;
            }
        }
    }
}
