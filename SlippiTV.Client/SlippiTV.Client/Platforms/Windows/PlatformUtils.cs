using Microsoft.Maui.Handlers;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using ToolTip = Microsoft.UI.Xaml.Controls.ToolTip;

namespace SlippiTV.Client.PlatformUtils;

public static class PlatformUtils
{
    public static void ShowContextMenu(View? view, Point? point)
    {
        var element = (view!.Handler as ViewHandler)!.PlatformView;
        element!.ContextFlyout.ShowAt(element, new FlyoutShowOptions
        {
            Position = new Windows.Foundation.Point(point!.Value.X, point!.Value.Y),

            //Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom
        });
    }

    /// <summary>
    /// Call only after the element is loaded, to ensure the handler and PlatformView are initialized.
    /// </summary>
    public static void SetCursor(this VisualElement element, CursorIcon cursorType)
    {
        if (element.Handler?.PlatformView is FrameworkElement frameworkElement)
        {
            CoreCursorType coreCursorType = cursorType switch
            {
                CursorIcon.Hand => CoreCursorType.Hand,
                CursorIcon.Arrow => CoreCursorType.Arrow,
                CursorIcon.Wait => CoreCursorType.Wait,
                CursorIcon.Help => CoreCursorType.Help,
                CursorIcon.Move => CoreCursorType.SizeAll,
                _ => CoreCursorType.Arrow
            };

            frameworkElement.PointerEntered += (sender, e) =>
            {
                frameworkElement.set_ProtectedCursor(InputCursor.CreateFromCoreCursor(new CoreCursor(coreCursorType, 1)));
            };

            frameworkElement.PointerExited += (sender, e) =>
            {
                frameworkElement.set_ProtectedCursor(InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Arrow, 1)));
            };
        }
    }

    public static void SetImmediateToolTip(this VisualElement element, string toolTip)
    {
        if (element.Handler?.PlatformView is FrameworkElement frameworkElement)
        {
            ToolTipService.SetToolTip(frameworkElement, new ToolTip() { Content = toolTip });

            frameworkElement.PointerEntered += (sender, e) =>
            {
                (ToolTipService.GetToolTip((FrameworkElement)sender) as ToolTip)!.IsOpen = true;
            };

            frameworkElement.PointerExited += (sender, e) =>
            {
                (ToolTipService.GetToolTip((FrameworkElement)sender) as ToolTip)!.IsOpen = false;
            };
        }
    }
}

file static class Accessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    public static extern void set_ProtectedCursor(this UIElement uiElement, InputCursor inputCursor);
}
