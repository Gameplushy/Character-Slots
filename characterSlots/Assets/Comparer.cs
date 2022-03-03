using KModkit;
using System;
using System.Linq;
using UnityEngine;

public static class Comparer { 
    public static bool Compare(int operand1, int operand2, string operatorWanted)
    {
        return Compare((double)operand1, operand2, operatorWanted);
    }

    public static bool Compare(double operand1, int operand2, string operatorWanted)
    {
        switch (operatorWanted)
        {
            case "more":
                return operand1 > operand2;
            case "less":
                return operand1 < operand2;
            case "exact":
                return operand1 == operand2;
            case "emore":
                return operand1 >= operand2;
            case "eless":
                return operand1 <= operand2;
            default:
                throw new ArgumentException(operatorWanted + " is not a valid operator.");
        }
    }

    public static bool SpecialNumberCompare(int operand, string specialCase)
    {
        bool answer =false;
        switch (specialCase)
        {
            case "prime":
                int[] primeNumbers = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53 };
                answer = primeNumbers.Any(p => p == operand);//operand.EqualsAny(primeNumbers);
                break;
            case "even":
                answer = operand % 2 == 0;
                break;
            case "odd":
                answer = operand % 2 == 1;
                break;
            default:
                answer = operand == int.Parse(specialCase);
                break;
        }
        return answer;
    }

    public static bool IsLetterOrNumber(char character, char motif)
    {
        if (motif == 'X')
        {
            return 'A' >= character && character <= 'Z';
        }
        else if (motif == '#')
        {
            return '0' >= character && character <= '9';
        }
        else throw new ArgumentException(motif+" is not X nor #...");
    }

    public static int GetEdgeworkNumber(string whatDoYouWant, KMBombInfo b)
    {
        switch (whatDoYouWant)
        {
            case "batteries":
                return b.GetBatteryCount();
                
            case "holders":
                return b.GetBatteryHolderCount();
                
            case "ports":
                return b.GetPortCount();
                
            case "ind":
                return b.GetIndicators().Count();
            default:
                throw new ArgumentException();
        }
    }
}

