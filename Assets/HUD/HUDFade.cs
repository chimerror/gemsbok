using System.Collections;
using System.Collections.Generic;
using VRTK;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Animator))]
public class HUDFade : MonoBehaviour, IFader
{
    private Image _fadeImage;
    private Animator _animator;
    private bool _isTransitioning;

    public event FadeEventHandler FadeStart;
    public event FadeEventHandler FadeComplete;
    public event FadeEventHandler UnfadeStart;
    public event FadeEventHandler UnfadeComplete;

    private void Start()
    {
        _fadeImage = GetComponent<Image>();
        _fadeImage.color = Color.clear;
        _animator = GetComponent<Animator>();
    }

    public void Fade(Color color, float duration)
    {
        _fadeImage.color = new Color(color.r, color.g, color.b, 0.0f);
        _animator.SetFloat("Duration", 1.0f / duration);
        _animator.SetTrigger("StartFade");
        _isTransitioning = true;
        if (FadeStart != null)
        {
            FadeStart(this);
        }
        CancelInvoke("OnUnfadeComplete");
        Invoke("OnFadeComplete", duration);
    }

    public void Unfade(float duration)
    {
        _animator.SetFloat("Duration", 1.0f / duration);
        _animator.SetTrigger("StartUnfade");
        _isTransitioning = true;
        if (UnfadeStart != null)
        {
            UnfadeStart(this);
        }
        CancelInvoke("OnFadeComplete");
        Invoke("OnUnfadeComplete", duration);
    }

    public bool IsFaded()
    {
        return _fadeImage.color.a == 1.0f;
    }

    public bool IsTransitioning()
    {
        return _isTransitioning;
    }

    private void OnFadeComplete()
    {
        _isTransitioning = false;
        if (FadeComplete != null)
        {
            FadeComplete(this);
        }
    }

    private void OnUnfadeComplete()
    {
        _isTransitioning = false;
        if (UnfadeComplete != null)
        {
            UnfadeComplete(this);
        }
    }
}
