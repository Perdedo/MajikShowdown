using System.Collections.Generic;
using UnityEngine;

public interface IMovingGround 
{
    public List<FloatingRigidbody> FRig { get; set; }
    public Vector3 GetVelocity();
}
