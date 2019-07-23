using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Config;

public class EditorControl : MonoBehaviour
{
    public RawImage Pic;

    public RectTransform ProSliderRoot;
    public RectTransform MaBtnRoot;
    public RectTransform ProToggleRoot;

    public GameObject Tem_ProSlider;
    public GameObject Tem_MaterialBtn;
    public GameObject Tem_ProToggle;

    private RenderTexture RT;
    private List<MaterialBtn> m_MaBtnLst = new List<MaterialBtn>();
    private List<MaterialProSlider> m_ProSliderLst = new List<MaterialProSlider>();
    private List<MaterialProToggle> m_ProToggleLst = new List<MaterialProToggle>();

    //private Dictionary<string, Materials.Row> m_MaCfgDic = new Dictionary<string, Materials.Row>();
    private Dictionary<string, Dictionary<string,float>> m_ProLastValDic = new Dictionary<string, Dictionary<string, float>>();
    private Dictionary<string, Material> m_MaterialDic = new Dictionary<string, Material>();

    private void Awake()
    {
        ConfigInitiator.InitAllConfig();
        foreach (var maCfg in Materials.s_rows)
        {
            if (m_MaterialDic.ContainsKey(maCfg.Name))
                continue;

            m_MaterialDic.Add(maCfg.Name, Resources.Load<Material>("Materials/" + maCfg.Name));
            m_ProLastValDic.Add(maCfg.Name, new Dictionary<string, float>());
            foreach (var pro in maCfg.SliderProperties)
            {
                m_ProLastValDic[maCfg.Name][pro.name] = pro.def;
            }
            _InitShaderView(maCfg);
        }
    }

    private void Start()
    {
        RegisterEvent();
        RenderTexture Disttexture = RenderTexture.GetTemporary(Pic.texture.width, Pic.texture.height, 0);
        m_MaterialDic["BSC"].SetFloat("B", 1);
        m_MaterialDic["BSC"].SetFloat("S", 1);
        m_MaterialDic["BSC"].SetFloat("C", 1);
        Graphics.Blit(Pic.texture, Disttexture, m_MaterialDic["BSC"]);

        RT = Disttexture;
    }

    private void _InitShaderView(Materials.Row maCfg)
    {
        var tem = Instantiate(Tem_MaterialBtn, MaBtnRoot).GetComponent<MaterialBtn>();
        m_MaBtnLst.Add(tem);
        tem.Text_Name.text = maCfg.Name;
        tem.Btn.onClick.AddListener(() =>
        {
            _UpdateProView(maCfg);
        });
    }

    private void _UpdateProView(Materials.Row maCfg)
    {
        #region Slider 表现
        var sliderlst = maCfg.SliderProperties;
        if (sliderlst.Count <= m_ProSliderLst.Count)
        {
            for (int i = m_ProSliderLst.Count - sliderlst.Count ; i > 0; i--)
            {
                m_ProSliderLst[m_ProSliderLst.Count - i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = sliderlst.Count - m_ProSliderLst.Count ; i > 0; i--)
            {
                m_ProSliderLst.Add(Instantiate(Tem_ProSlider,ProSliderRoot).GetComponent<MaterialProSlider>());
            }
        }

        for (int i = 0; i < sliderlst.Count; i++)
        {
            int j = i;
            m_ProSliderLst[i].gameObject.SetActive(true);
            m_ProSliderLst[i].Text_ProName.text = sliderlst[i].name;
            m_ProSliderLst[i].Slider_ProValue.value = m_ProLastValDic[maCfg.Name][sliderlst[i].name];
            m_ProSliderLst[i].Slider_ProValue.minValue = sliderlst[i].min;
            m_ProSliderLst[i].Slider_ProValue.maxValue = sliderlst[i].max;

            m_ProSliderLst[i].Slider_ProValue.onValueChanged.RemoveAllListeners();
            m_ProSliderLst[i].Slider_ProValue.onValueChanged.AddListener((val) =>
            {
                if(Mathf.Abs(val - m_ProLastValDic[maCfg.Name][sliderlst[j].name]) > 0.01)
                {
                    m_ProLastValDic[maCfg.Name][sliderlst[j].name] = val;
                    var materail = m_MaterialDic[maCfg.Name];
                    materail.SetFloat(sliderlst[j].name, val);
                    Pic.texture = PostEffectsCore.Instance.UpdateImage(RT, materail);
                }
            });
        }
        #endregion
    }

    private void RegisterEvent()
    {
        foreach (var materailPro in Materials.s_rows[0].SliderProperties)
        {
            //m_ProSliderDic.Add(materailPro, Instantiate(Tem_ProSlider,ProSliderRoot).GetComponent<MaterialProSlider>());
            //m_ProSliderDic[materailPro].gameObject.SetActive(true);
            //m_ProLastValDic.Add(materailPro, 0f);

            //m_ProSliderDic[materailPro].Text_ProName.text = materailPro;
            //m_ProSliderDic[materailPro].Slider_ProValue.value = 1;
            //m_ProSliderDic[materailPro].Slider_ProValue.onValueChanged.AddListener((val) =>
            //{
            //    if(Mathf.Abs(val - m_ProLastValDic[materailPro]) >= 0.1)
            //    {
            //        m_ProLastValDic[materailPro] = val;
            //        var materail = m_MaterialDic["BSC"];
            //        materail.SetFloat(materailPro, val);
            //        Pic.texture = PostEffectsCore.Instance.UpdateImage(RT,materail);
            //    }
            //});
        }


        //for (int i = 0; i < Materials.s_rows.Count; i++)
        //{
        //    for (int j = 0; j < Materials.s_rows[i].SliderProperties.Count; j++)
        //    {

        //    }
        //}
    }
}
