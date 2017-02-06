using UnityEngine;
using VRTK;

public delegate void FadeEventHandler(object sender);

/// <summary>
/// Wrapper interface to abstract the HeadsetFade and HUD Fade
/// </summary>
public interface IFader
{
    event FadeEventHandler FadeStart;
    event FadeEventHandler FadeComplete;
    event FadeEventHandler UnfadeStart;
    event FadeEventHandler UnfadeComplete;

    bool IsFaded();
    bool IsTransitioning();
    void Fade(Color color, float duration);
    void Unfade(float duration);
}
