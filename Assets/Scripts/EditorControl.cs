using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Config;

public class EditorControl : MonoBehaviour
{
    public RawImage Pic;
    public RectTransform Lst_Root;
    public GameObject Tem_MaterialSlider;

    private RenderTexture RT;
    private Dictionary<string, MaterialProSlider> mDic_Slider_Property = new Dictionary<string, MaterialProSlider>();
    private Dictionary<string, float> mDic_LastValue_Property = new Dictionary<string, float>();
    private Dictionary<string, Material> mDic_Materials_Name = new Dictionary<string, Material>();

    private void Awake()
    {
        ConfigInitiator.InitAllConfig();
        foreach (var materialcfg in Materials.s_rows)
        {
            if(!mDic_Materials_Name.ContainsKey(materialcfg.Name))
            mDic_Materials_Name.Add(materialcfg.Name, Resources.Load<Material>("Materials/" + materialcfg.Name));
        }
    }

    private void Start()
    {
        RegisterEvent();
        RenderTexture Disttexture = RenderTexture.GetTemporary(Pic.texture.width, Pic.texture.height, 0);
        mDic_Materials_Name["BSC"].SetFloat("B", 1);
        mDic_Materials_Name["BSC"].SetFloat("S", 1);
        mDic_Materials_Name["BSC"].SetFloat("C", 1);
        Graphics.Blit(Pic.texture, Disttexture, mDic_Materials_Name["BSC"]);

        RT = Disttexture;
    }

    private void RegisterEvent()
    {
        foreach (var materailPro in Materials.s_rows[0].SliderProperties)
        {
            mDic_Slider_Property.Add(materailPro, Instantiate(Tem_MaterialSlider,Lst_Root).GetComponent<MaterialProSlider>());
            mDic_Slider_Property[materailPro].gameObject.SetActive(true);
            mDic_LastValue_Property.Add(materailPro, 0f);

            mDic_Slider_Property[materailPro].Text_ProName.text = materailPro;
            mDic_Slider_Property[materailPro].Slider_ProValue.value = 1;
            mDic_Slider_Property[materailPro].Slider_ProValue.onValueChanged.AddListener((val) =>
            {
                if(Mathf.Abs(val - mDic_LastValue_Property[materailPro]) >= 0.01)
                {
                    mDic_LastValue_Property[materailPro] = val;
                    var materail = mDic_Materials_Name["BSC"];
                    materail.SetFloat(materailPro, val);
                    Pic.texture = PostEffectsCore.Instance.UpdateImage(RT,materail);
                }
            });
        }


        //for (int i = 0; i < Materials.s_rows.Count; i++)
        //{
        //    for (int j = 0; j < Materials.s_rows[i].SliderProperties.Count; j++)
        //    {

        //    }
        //}
    }
}
