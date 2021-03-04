using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AspectRatio { Undefined = -1, _16x9, _16x10, _18x9, _18_5x9, _19_5x9, _5x3, _4x3, _4_3x3 }
/// <summary>
/// Статический класс, хранящий в себе список всех возможных AspectRatio на таргет устройствах
/// </summary>
public class AspectRatioTable
{
    private static readonly double[] existRatios = new double[] { 1.77, 1.66, 1.33, 1.43, 2.16, 2.05, 2, 1.6 };
    private const double knownDifference = 0.01;
    public static AspectRatio GetAspectRatioByRatio(double ratio)
    { // предоставляет информацию о нынешнем AspectRatio устройства параметрах необходимо передать обычное соотношение между шириной и высотой дисплея
      // результатом деления ширины на длину можно получить число, но оно будет не точным(спасибо float), 
      // по этому допускаем разброс в 0.01(knownDifference) при помощи функции AdjustCalculation()
        ratio = AdjustCalculation(ratio);
        switch(ratio)
        {
            case 1.77:
                return AspectRatio._16x9;

            case 1.66:
                return AspectRatio._5x3;

            case 1.33:
                return AspectRatio._4x3;

            case 1.43:
                return AspectRatio._4_3x3;

            case 2.16:
                return AspectRatio._19_5x9;

            case 2.05:
                return AspectRatio._18_5x9;

            case 2:
                return AspectRatio._18x9;

            case 1.6:
                return AspectRatio._16x10;

            default:
                return AspectRatio.Undefined;
        }
    }

    private static double AdjustCalculation(double ratio)
    {
        for (int i = 0; i < existRatios.Length; ++i)
        {
            if (Abs(existRatios[i] - ratio) < knownDifference)
                return existRatios[i];
        }
        return -1;
    }

    private static double Abs(double value)
    {
        return value < 0 ? value * -1 : value;
    }
}