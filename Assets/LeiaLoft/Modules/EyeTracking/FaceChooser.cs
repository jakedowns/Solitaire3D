#if UNITY_STANDALONE_WIN
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BlinkTrackingUnityPlugin;

public class FaceChooser
{
    int numFaces;
    float faceX;
    float faceY;
    float faceZ;
    float oldFaceX;
    float oldFaceY;
    float oldFaceZ = -600f; // Bogus value
    int hysteresisCounter = 0;
    const int hysteresisCounterFull = 50;
    int closestFaceInZIndex = 0;
    int chosenFaceIndex = 0;
    int previousChosenFaceIndex = 0;

    public int ChosenFaceIndex(LeiaHeadTracking.Engine.Result trackingResult)
    {
        numFaces = trackingResult.numDetectedFaces;

        closestFaceInZIndex = 0;
        for(int i = 1; i < numFaces ; i++)
        {
            if (trackingResult.detectedFaces[i].z < trackingResult.detectedFaces[closestFaceInZIndex].z)
            {
                closestFaceInZIndex = i;
            }
        }

        if (numFaces > 0)
        {
            faceX = trackingResult.detectedFaces[closestFaceInZIndex].x;
            faceY = trackingResult.detectedFaces[closestFaceInZIndex].y;
            faceZ = trackingResult.detectedFaces[closestFaceInZIndex].z;
            chosenFaceIndex = closestFaceInZIndex;
        }
        else
        {
            faceX = oldFaceX;
            faceY = oldFaceY;
            faceZ = oldFaceZ;
            return previousChosenFaceIndex;
        }
        // Set the old face to something valid, first time only
        // This happens because oldFaceZ is intentionally defaulted to less than 0.0
        if (oldFaceZ < 0)
        {
            oldFaceX = faceX;
            oldFaceY = faceY;
            oldFaceZ = faceZ;
            previousChosenFaceIndex = chosenFaceIndex;
        }
        // Check how far we are from faces previous position
        float x1 = oldFaceX;
        float y1 = oldFaceY;
        float z1 = oldFaceZ;
        float x2 = faceX;
        float y2 = faceY;
        float z2 = faceZ;
        float dist = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2);
        dist = Mathf.Sqrt(dist);
        const float minDistance = 50f;
        const float maxDistance = 100f;
        if (dist > maxDistance)
        {
            hysteresisCounter++; //TODO: make this dependent on deltatime
            if (hysteresisCounter > hysteresisCounterFull)
            {
                oldFaceX = faceX;
                oldFaceY = faceY;
                oldFaceZ = faceZ;
                previousChosenFaceIndex = chosenFaceIndex;
                hysteresisCounter = 0;
                //possibly add animated 3d to 2d to 3d transition
                //smooth camera movement transition from old face to new face position
                //reduce disparity to 0 while traveling, increase back once in position
            }
            else
            {
                // Find better face
                bool betterMatch = false;
                int i;

                string minInterocularDistanceStr = PlayerPrefs.GetString("minInterocularDistance");

                if (minInterocularDistanceStr == "")
                {
                    minInterocularDistanceStr = "0";
                }

                float minInterocularDistance = 3; //float.Parse(minInterocularDistanceStr);

                for (i = 0; i < numFaces; ++i)
                {
                    if (i == closestFaceInZIndex)
                    {
                        continue;
                    }
                     //TODO: add this in later to skip faces with low interoculars (after updating blink DLL from Suki)
                    //if (trackingResult.detectedFaces[i].interocularDistance < minInterocularDistance) //3
                    //{
                    //    continue;
                    //}
                    x2 = trackingResult.detectedFaces[i].x;
                    y2 = trackingResult.detectedFaces[i].y;
                    z2 = trackingResult.detectedFaces[i].z;
                    dist = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2);
                    dist = Mathf.Sqrt(dist);
                    if (dist < minDistance)
                    {
                        betterMatch = true;
                        break;
                    }
                }
                // Hysterisis should kick in
                if (betterMatch)
                {
                    faceX = trackingResult.detectedFaces[i].x;
                    faceY = trackingResult.detectedFaces[i].y;
                    faceZ = trackingResult.detectedFaces[i].z;
                    chosenFaceIndex = i;
                }
                oldFaceX = faceX;
                oldFaceY = faceY;
                oldFaceZ = faceZ;
                previousChosenFaceIndex = chosenFaceIndex;
            }
        }
        else
        {
            oldFaceX = faceX;
            oldFaceY = faceY;
            oldFaceZ = faceZ;
            hysteresisCounter = 0;
        }
        
        return chosenFaceIndex;
    }
}
#endif