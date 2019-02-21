using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScreenTextureAllocator{
    static public void allocateTexture(ref RenderTexture texture, float sizeScale = 1) {
        if(texture.width != (int)(Screen.width * sizeScale) || texture.height != (int)(Screen.height * sizeScale))
        {
            var temp = texture;
            texture = generateRenderTexture(sizeScale);
            RenderTexture.Destroy(temp);
        }
    }
    static public void allocateTexture(ref RenderTexture texture, Action beforeDestroy, float sizeScale = 1) {
        if(texture.width != (int)(Screen.width * sizeScale) || texture.height != (int)(Screen.height * sizeScale))
        {
            var temp = texture;
            texture = generateRenderTexture(sizeScale);
            beforeDestroy();
            RenderTexture.Destroy(temp);
        }
        else {
            beforeDestroy();
        }
    }
    static public RenderTexture generateRenderTexture(float sizeScale = 1)
    {
        var r = new RenderTexture((int)(Screen.width * sizeScale), (int)(Screen.height * sizeScale), 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        r.filterMode = FilterMode.Trilinear;
        return r;

    }
    static public bool fovEnabled = true;

}
