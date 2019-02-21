using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using PseudoTools;
public class PositionDistanceQueue {
    public Func<Vector2> posGet;
    public float distanceMax;
    public int nodeCountMax;
    public Vector2 PositionBefore {
        get {
            return queue.First.Value;
        }
    }
    private LinkedList<Vector2> queue;
    private float distacneCount;
    private Vector2 lastPos;

    public PositionDistanceQueue(Func<Vector2> posGet, float distanceMax, int nodeCountMax)
    {
        this.posGet = posGet;
        this.distanceMax = distanceMax;
        this.nodeCountMax = nodeCountMax;
    }

    public IEnumerator Update() {
        queue = new LinkedList<Vector2>();
        distacneCount = 0;
        lastPos = posGet();
        queue.AddLast(lastPos);
        float distancePart = distanceMax / (float)(nodeCountMax - 1);
        yield return null;
        while(true) {
            var dir = posGet() - lastPos;
            lastPos = posGet();
            distacneCount += dir.magnitude;
            while(distacneCount > distancePart) {
                distacneCount -= distancePart;
                queue.AddLast(posGet());
                if(queue.Count > nodeCountMax) {
                    queue.RemoveFirst();
                }
            }
            yield return null;
        }
    }
}