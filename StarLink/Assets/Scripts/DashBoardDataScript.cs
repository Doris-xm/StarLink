using System.Collections;
using System.Collections.Generic;
//using SatelliteInspector.Scripts.PrefabScripts;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SatelliteInspector.Scripts.PrefabScripts
{
public class DashBoardDataScript : MonoBehaviour
{   
    public GameObject Satellites;
    public TMP_Dropdown dropdown;
    public Button showhideSatelliteButton;
    public Button showhideOrbitButton;

    public Button showhideAllButton;
    public GameObject Satellite;
    public Text TextElevation;
    public Text TextAzimuth;
    public Text TextDistance;
    public Text TextLatitude;
    public Text TextLongitude;
    public Text TextAltitude;
    public Sprite showSatImage;
    public Sprite hideSatImage;
    public Sprite showOrbitImage;
    public Sprite hideOrbitImage;
    public Sprite showAllImage;
    public Sprite hideAllImage;
    private bool showhideSatFlag;
    private bool showhideOrbitFlag;
    private bool showhideAllFlag;
    
    // Start is called before the first frame update
    void Start()
    {   
        //获取Satellites下的所有子物体的名称，添加到dropdown的选项中
        foreach (Transform child in Satellites.transform)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(child.name));
        }
        //当dropdown的值改变时，调用onValueChanged函数
        dropdown.onValueChanged.AddListener(delegate {
            onValueChanged();
        });
        //当showhideButton被点击时，调用showhide函数
        showhideOrbitFlag = false;
        showhideSatFlag = false;
        showhideAllFlag = false;
        showhideSatelliteButton.onClick.AddListener(delegate {
            showhideSat();
        });
        showhideOrbitButton.onClick.AddListener(delegate {
            showhideOrbit();
        });
        showhideAllButton.onClick.AddListener(delegate {
            showhideAll();
        });
        Satellite = Satellites.transform.Find(dropdown.options[0].text).gameObject;
    }

    // Update is called once per frame
    void Update()
    {   
        
        SatelliteScript satelliteScript = Satellite.GetComponent<SatelliteScript>();
        TextElevation.text = satelliteScript.getElevation();
        TextAzimuth.text = satelliteScript.getAzimuth();
        TextDistance.text = satelliteScript.getDistance();
        TextLatitude.text = satelliteScript.getLatitude();
        TextLongitude.text = satelliteScript.getLongitude();
        TextAltitude.text = satelliteScript.getAltitude();

    }
    public void onValueChanged()
    {
        Debug.Log(dropdown.options[dropdown.value].text);
        //在Satellites下找到名字为dropdown.value的子物体
        Satellite = Satellites.transform.Find(dropdown.options[dropdown.value].text).gameObject;
        if(Satellite.GetComponent<SatelliteScript>().isChecked == true){
            showhideSatelliteButton.GetComponent<Image>().sprite = hideSatImage;
            showhideSatFlag = true;
        }
        else{
            showhideSatelliteButton.GetComponent<Image>().sprite = showSatImage;
            showhideSatFlag = false;
        }
        if(Satellite.GetComponent<SatelliteScript>().IsSelected == true){
            showhideOrbitButton.GetComponent<Image>().sprite = hideOrbitImage;
            showhideOrbitFlag = true;
        }
        else{
            showhideOrbitButton.GetComponent<Image>().sprite = showOrbitImage;
            showhideOrbitFlag = false;
        }
    }

    public void showhideSat(){
        if(showhideSatFlag == false){
            showhideSatFlag = true;
            showhideSatelliteButton.GetComponent<Image>().sprite = hideSatImage;
            SatelliteScript satelliteScript = Satellite.GetComponent<SatelliteScript>();
            satelliteScript.isChecked = true;
            satelliteScript.gameObject.SetActive(true);
        }
        else{
            showhideSatFlag = false;
            showhideSatelliteButton.GetComponent<Image>().sprite = showSatImage;
            SatelliteScript satelliteScript = Satellite.GetComponent<SatelliteScript>();
            satelliteScript.gameObject.SetActive(false);
            satelliteScript.isChecked = false;
        }
    }

    public void showhideOrbit(){
        if(showhideOrbitFlag == false){
            showhideOrbitFlag = true;
            showhideOrbitButton.GetComponent<Image>().sprite = hideOrbitImage;
            SatelliteScript satelliteScript = Satellite.GetComponent<SatelliteScript>(); 
            satelliteScript.IsSelected = true; 
        }
        else{
            showhideOrbitFlag = false;
            showhideOrbitButton.GetComponent<Image>().sprite = showOrbitImage;
            SatelliteScript satelliteScript = Satellite.GetComponent<SatelliteScript>();
            satelliteScript.IsSelected = false;
        }
    }
    public void showhideAll(){
        if(showhideAllFlag == false){
            showhideAllFlag = true;
            showhideAllButton.GetComponent<Image>().sprite = hideAllImage;
            foreach (Transform child in Satellites.transform)
            {
                child.gameObject.SetActive(true);
                child.GetComponent<SatelliteScript>().isChecked = true;
            }
        }
        else{
            showhideAllFlag = false;
            showhideAllButton.GetComponent<Image>().sprite = showAllImage;
            foreach (Transform child in Satellites.transform)
            {
                child.gameObject.SetActive(false);
                child.GetComponent<SatelliteScript>().isChecked = false; 
            }
        }
        if(Satellite.GetComponent<SatelliteScript>().isChecked == true){
            showhideSatelliteButton.GetComponent<Image>().sprite = hideSatImage;
            showhideSatFlag = true;
        }
        else{
            showhideSatelliteButton.GetComponent<Image>().sprite = showSatImage;
            showhideSatFlag = false;
        }
    }
}
}
