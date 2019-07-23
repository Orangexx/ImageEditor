using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public class PostEffectsCore : MonoSingleton<PostEffectsCore>
{
    #region 变量
    private bool overturnX = false;
    private bool overturnY = false;//翻转状态
    private bool clockw = false;
    private bool anclockw = false;//旋转状态
    #endregion

    #region  平台、材质检查
    private void CheckResources()
    {
        bool isSupported = CheckSupport();
        if (isSupported == false)
        {
            NotSupported();
        }
    }
    private bool CheckSupport()
    {
        if (SystemInfo.supportsImageEffects == false)
        {
            Debug.LogWarning("This platform does not support image effects or render textures.");
            return false;
        }
        return true;
    }
    private void NotSupported()
    {
        enabled = false;
    }
    void Awake()
    {
        CheckResources();
    }
    public Material CheckShaderAndCreateMaterial(Shader shader, Material material)
    {
        if (shader == null)
        {
            return null;
        }
        if (shader.isSupported && material && material.shader == shader)
            return material;
        if (!shader.isSupported)
        {
            return null;
        }
        else
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            if (material)
                return material;
            else
                return null;
        }
    }

    #endregion

    #region 处理材质方法
    public Texture UpdateImage(RenderTexture RT,Material material)//BSCshader更新
    {
        RenderTexture Disttexture = RenderTexture.GetTemporary(RT.width, RT.height, 0);
        Graphics.Blit(RT, Disttexture, material);
        RT = Disttexture;
        int width = Disttexture.width;
        int height = Disttexture.height;
        Resources.UnloadUnusedAssets();
        return Disttexture;

        //image.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);//因为可能image已经缩放了
        //                                                                           //弃用，Rt转Sprite太慢//sprite = null;
        //                                                                           //sprite = Sprite.Create(Viewtexture, new Rect(0, 0, Viewtexture.width, Viewtexture.height), new Vector2(0.5f, 0.5f));//因为居中显示所以.5f
        //                                                                           //Debug.Log(System.DateTime.Now.Second.ToString() + "  " + System.DateTime.Now.Millisecond.ToString());//消耗检测
        //                                                                           //image.sprite = sprite;
        //image.GetComponent<RawImage>().texture = Disttexture;
        //Refresh();
    }
    #endregion
}