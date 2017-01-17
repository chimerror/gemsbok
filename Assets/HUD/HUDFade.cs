using System.Collections;
using System.Collections.Generic;
using VRTK;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Animator))]
public class HUDFade : VRTK_HeadsetFade
{
    private Image _fadeImage;
    private Animator _animator;

    private void Start()
    {
        _fadeImage = GetComponent<Image>();
        _fadeImage.color = Color.clear;
        _animator = GetComponent<Animator>();
    }

    public override void Fade(Color color, float duration)
    {
        base.Fade(color, duration);
        _fadeImage.color = new Color(color.r, color.g, color.b, 0.0f);
        _animator.SetFloat("Duration", 1.0f / duration);
        _animator.SetTrigger("StartFade");
    }

    public override void OnHeadsetFadeComplete(HeadsetFadeEventArgs e)
    {
        base.OnHeadsetFadeComplete(e);
        var color = _fadeImage.color;
    }

    public override void Unfade(float duration)
    {
        base.Unfade(duration);
        _animator.SetFloat("Duration", 1.0f / duration);
        _animator.SetTrigger("StartUnfade");
    }

    public override void OnHeadsetUnfadeComplete(HeadsetFadeEventArgs e)
    {
        base.OnHeadsetUnfadeComplete(e);
        var color = _fadeImage.color;
    }
}
