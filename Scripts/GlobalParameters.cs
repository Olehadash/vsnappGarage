using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalParameters
{
    public static CallMessage gcmsg;
    public static bool IsBusy = false;
    public static bool isLogined = false;
    public static bool IsUserLogined = false;
    public static readonly Color unselectedButtonColor = Color.white;
    public static readonly Color selectedButtonColor = new Color32(63, 84, 152, 255);
    public static readonly Color disabledButtonColor = new Color32(200, 200, 200, 255);

    public static readonly Color unselectedTextColor = Color.black;
    public static readonly Color selectedTextColor = Color.white;
    public static readonly Color disabledTextColor = new Color32(150, 150, 150, 255);

    public static readonly Color alphaColor = new Color(1f, 1f, 1f, 0f);

    public static readonly string[] forbiddenSymbols =
{
        "_", "\\", "-"
    };
}
