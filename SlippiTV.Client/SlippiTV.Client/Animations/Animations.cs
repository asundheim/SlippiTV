using SlippiTV.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlippiTV.Client.Animations;

public static class Animations
{
    public static void Pulse(this VisualElement visualElement, Func<bool> repeat)
    {
        Animation pulse = new Animation();
        Animation pulseOut = new Animation(v => visualElement.Scale = v, 1, 1.02, Easing.CubicIn);
        Animation pulseIn = new Animation(v => visualElement.Scale = v, 1.02, 1, Easing.CubicOut);
        pulse.Add(0, 0.5, pulseOut);
        pulse.Add(0.5, 1, pulseIn);

        visualElement.Animate("PulseAnimation", pulse, length: 1000, repeat: repeat);
    }

    public static void Pulse2(this VisualElement visualElement, Func<bool> repeat)
    {
        Animation pulse = new Animation();
        Animation pulseOut = new Animation(v => visualElement.Scale = v, 1, 1.2, Easing.CubicIn);
        Animation pulseIn = new Animation(v => visualElement.Scale = v, 1.2, 1, Easing.CubicOut);
        pulse.Add(0, 0.5, pulseOut);
        pulse.Add(0.5, 1, pulseIn);

        visualElement.Animate("PulseAnimation", pulse, length: 2000, repeat: repeat);
    }

    public static void Pulse3(this VisualElement visualElement, Func<bool> repeat)
    {
        Animation pulse = new Animation();
        Animation pulseOut = new Animation(v => visualElement.Scale = v, 1, 1.1, Easing.CubicIn);
        Animation pulseIn = new Animation(v => visualElement.Scale = v, 1.1, 1, Easing.CubicOut);
        pulse.Add(0, 0.5, pulseOut);
        pulse.Add(0.5, 1, pulseIn);

        visualElement.Animate("PulseAnimation", pulse, length: 1000, repeat: repeat);
    }
}
