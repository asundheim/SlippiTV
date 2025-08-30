using SlippiTV.Client.PlatformUtils;

namespace SlippiTV.Client;

public class CursorProperties
{
    public static readonly BindableProperty CursorProperty = BindableProperty.CreateAttached("Cursor", typeof(CursorIcon), typeof(CursorProperties), CursorIcon.Arrow, propertyChanged: CursorChanged);

    private static void CursorChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        if (bindable is VisualElement visualElement)
        {
            if (!visualElement.IsLoaded)
            {
                visualElement.Loaded += ElementLoaded;
            }
            else
            {
                visualElement.SetCursor((CursorIcon)newvalue);
            }
        }

        void ElementLoaded(object? sender, EventArgs e)
        {
            visualElement.SetCursor((CursorIcon)newvalue);
            visualElement.Loaded -= ElementLoaded;
        }
    }

    public static CursorIcon GetCursor(BindableObject view) => (CursorIcon)view.GetValue(CursorProperty);

    public static void SetCursor(BindableObject view, CursorIcon value) => view.SetValue(CursorProperty, value);
}

public enum CursorIcon
{
    Arrow,
    Hand,
    Help,
    Wait,
    Move
}