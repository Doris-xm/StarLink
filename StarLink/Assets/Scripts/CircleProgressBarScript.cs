using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircleProgressBarScript : MonoBehaviour
{
    // Start is called before the first frame update
    public string Info;
    public string unit;
    public double maxData;
    public double minData;
    public Image progressBar;
    public TMP_Text infoText;
    public TMP_Text dataText;
    public TMP_Text unitText;
    public double speed;
    private double percent;
    //private double speed = 0.001;
    void Start()
    {
        percent = 0;
        infoText.text = Info;
        unitText.text = unit;
        dataText.text = minData.ToString("f2");
    }

    // Update is called once per frame
    void Update()
    {
        percent += speed;
        if (percent >= 1)
        {
            percent = 0;
        }
        progressBar.fillAmount = (float)percent;
        dataText.text = (minData + (maxData - minData) * percent).ToString("f2");
    }
}
