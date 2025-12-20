using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CameraSequenceMove : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveDuration = 2f;
    public float rotateDuration = 1.2f;

    void Start()
    {
        StartCoroutine(CameraSequence());
    }

    IEnumerator CameraSequence()
    {
        // ===== Step 1 =====
        Vector3 posA = new Vector3(-7f, 4.131451f, -19.32f);
        Vector3 rotA = new Vector3(26.678f, -5.509f, 0f);

        Vector3 posB = new Vector3(-7f, 4.131451f, 4.99f);
        Vector3 rotB1 = new Vector3(26.678f, -5.509f, 0f);
        Vector3 rotB2 = new Vector3(26.678f, 14.302f, 0f);
        Vector3 rotB3 = new Vector3(26.678f, 87.179f, 0f);

        // 初始化（保險）
        transform.position = posA;
        transform.rotation = Quaternion.Euler(rotA);

        // A → B（位置移動，角度不變）
        yield return MoveAndRotate(posA, rotA, posB, rotB1, moveDuration);

        // 原地旋轉到 14.302
        yield return RotateOnly(rotB1, rotB2, rotateDuration);

        // 原地旋轉到 87.179
        yield return RotateOnly(rotB2, rotB3, rotateDuration);

        // 旋轉回 -5.509
        yield return RotateOnly(rotB3, rotB1, rotateDuration);

        // 結束，停住
        yield break;
    }

    IEnumerator MoveAndRotate(
        Vector3 startPos,
        Vector3 startRot,
        Vector3 endPos,
        Vector3 endRot,
        float duration)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Lerp(
                Quaternion.Euler(startRot),
                Quaternion.Euler(endRot),
                t
            );

            yield return null;
        }
    }

    IEnumerator RotateOnly(Vector3 fromRot, Vector3 toRot, float duration)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            transform.rotation = Quaternion.Lerp(
                Quaternion.Euler(fromRot),
                Quaternion.Euler(toRot),
                t
            );

            yield return null;
        }
    }
}

