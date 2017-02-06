using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VRTK;

/// <summary>
/// Wrapper class around VRTK_HeadsetFade.
/// </summary>
public class HeadsetFadeWrapper : IFader
{
    private VRTK_HeadsetFade _headsetFade;

    public HeadsetFadeWrapper(VRTK_HeadsetFade headsetFade)
    {
        _headsetFade = headsetFade;
        _headsetFade.HeadsetFadeStart += OnFadeStart;
        _headsetFade.HeadsetFadeComplete += OnFadeComplete;
        _headsetFade.HeadsetUnfadeStart += OnUnfadeStart;
        _headsetFade.HeadsetUnfadeComplete += OnUnfadeComplete;
    }

    public event FadeEventHandler FadeComplete;
    public event FadeEventHandler FadeStart;
    public event FadeEventHandler UnfadeComplete;
    public event FadeEventHandler UnfadeStart;

    public void Fade(Color color, float duration)
    {
        _headsetFade.Fade(color, duration);
    }

    public void Unfade(float duration)
    {
        _headsetFade.Unfade(duration);
    }

    public bool IsFaded()
    {
        return _headsetFade.IsFaded();
    }

    public bool IsTransitioning()
    {
        return _headsetFade.IsTransitioning();
    }

    private void OnFadeStart(object sender, HeadsetFadeEventArgs e)
    {
        FadeStart(sender);
    }

    private void OnFadeComplete(object sender, HeadsetFadeEventArgs e)
    {
        FadeComplete(sender);
    }

    private void OnUnfadeStart(object sender, HeadsetFadeEventArgs e)
    {
        UnfadeStart(sender);
    }

    private void OnUnfadeComplete(object sender, HeadsetFadeEventArgs e)
    {
        UnfadeComplete(sender);
    }
}
