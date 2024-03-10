using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasProgress
{
    public event EventHandler<OnProgressChangedEnventArgs> OnProgressChanged;
    public class OnProgressChangedEnventArgs : EventArgs
    {
        public float progressNormalized;

    }
}
