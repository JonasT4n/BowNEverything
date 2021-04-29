using System;
using UnityEngine;

public interface IElementItemInfo : IDisposable
{
    Sprite ItemSprite { get; }
}