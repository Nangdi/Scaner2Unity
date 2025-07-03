using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
public interface ICharacterAni
{
    public void Idle();
    public void Walk(Vector3 destination);
    public void Run(Vector3 destination);
    public void Reaction1();
    public void Reaction2();
    public void Reaction3();
}
