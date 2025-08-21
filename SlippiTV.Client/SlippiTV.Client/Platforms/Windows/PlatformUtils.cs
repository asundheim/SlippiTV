
using Microsoft.Maui.Handlers;

namespace SlippiTV.Client.PlatformUtils;

public static class PlatformUtils
{
    public static void ShowContextMenu(View? view, Point? point)
    {
        var element = (view!.Handler as ViewHandler)!.PlatformView;
        element!.ContextFlyout.ShowAt(element, new Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions
        {
            Position = new Windows.Foundation.Point(point!.Value.X, point!.Value.Y),

            //Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom
        });
    }
}