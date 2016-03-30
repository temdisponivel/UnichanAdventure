using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CallbackHelper
{
    static public IEnumerator WaitForSecondsAndCall(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }
}