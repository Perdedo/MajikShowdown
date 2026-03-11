using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Utility
{
    public static float highestAbsElement(Vector3 vector) //retorna o elemento absoluto mais alto do vetor
    {
        int highest = 0;
        for(int i = 1; i<3; i++)
        {
            if(Mathf.Abs(vector[i]) > Mathf.Abs(vector[highest]))
            {
                highest = i;
            }
        }
        return vector[highest];
    }
    public static Vector3 highestAxis(Vector3 vector) //retorna a maior axis do vetor
    {
        float i = highestAbsElement(vector);
        if(i == vector.x)
        {
            return Vector3.right * Mathf.Sign(vector.x);
        }
        else if (i == vector.y)
        {
            return Vector3.up * Mathf.Sign(vector.y);
        }
        else
        {
            return Vector3.forward * Mathf.Sign(vector.z);
        }
    }

}
