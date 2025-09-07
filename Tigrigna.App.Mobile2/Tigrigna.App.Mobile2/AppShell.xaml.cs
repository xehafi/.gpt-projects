using Tigrigna.App.Mobile2.Views;

namespace Tigrigna.App.Mobile2
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("lesson", typeof(LessonPage));
        }
    }
}
