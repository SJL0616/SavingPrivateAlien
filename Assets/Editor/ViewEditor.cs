//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (EnemyDetectionController))]
public class ViewEditor : Editor
{
    void OnSceneGUI()
    {
        EnemyDetectionController fow = (EnemyDetectionController)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.lookOutViewRadius);//큰 원(360도) 그리기
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.attackViewRadius);//공격 원 그리기
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.immediateCombatRadius);//즉각 공격 원 그리기


        Vector3 viewAngleA = fow.DirFromAngle(-fow.normalViewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.normalViewAngle / 2, false);
        Vector3 viewAngleC = fow.DirFromAngle(-fow.normalViewAngle / 2, false);
        Vector3 viewAngleD = fow.DirFromAngle(fow.normalViewAngle / 2, false);
        Vector3 viewAngleE = fow.DirFromAngle(-fow.normalViewAngle / 2, false);
        Vector3 viewAngleF = fow.DirFromAngle(fow.normalViewAngle / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.lookOutViewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.lookOutViewRadius);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleC * fow.attackViewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleD * fow.attackViewRadius);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleE * fow.attackViewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleF * fow.attackViewRadius);

        Handles.color = Color.red;
        foreach (Transform visible in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visible.transform.position);
        }
    }
}
