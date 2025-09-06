namespace Tigrinya.App.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("lesson", typeof(Pages.LessonPlayerPage));
        Routing.RegisterRoute("placement", typeof(Pages.PlacementPage));
        Routing.RegisterRoute("qa", typeof(Pages.QaReviewPage));
    }
}
