using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardViewScript : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject dashboard;
    public GameObject frontCanvas;
    public Button showHideDashboardButton;
    public Button menuButton;
    public Button overview3DButton;
    public Button satelliteStateButton;
    public Button systemInfoButton;
    public Button controlPanelButton;
    public GameObject Logo;
    public GameObject Menu;
    public GameObject Overview3D;
    public GameObject SatelliteState;
    public GameObject SatelliteStateMainPanel;
    public GameObject SatelliteStateBasicInfo;
    private Animation dashboardAnimation;
    private Animation basicInfoAnimation;
    private bool isDashboardShown = true;

    //记录当前dashboard显示的内容,0表示主菜单,1表示3DOverview,2表示SatelliteState,3表示SystemInfo,4表示ControlPanel;
    private int dashboardMenu = 0;
    void Start()
    {
        dashboardAnimation = dashboard.GetComponent<Animation>();
        basicInfoAnimation = SatelliteStateBasicInfo.GetComponent<Animation>();
        //为showHideDashboardButton添加点击事件
        showHideDashboardButton.onClick.AddListener(delegate {
            onShowHideDashboard();
        });
        //为menuButton添加点击事件
        menuButton.onClick.AddListener(delegate {
            onMenuButton();
        });
        //为overview3DButton添加点击事件
        overview3DButton.onClick.AddListener(delegate {
            onOverview3DButton();
        });
        //为satelliteStateButton添加点击事件
        satelliteStateButton.onClick.AddListener(delegate {
            onSatelliteStateButton();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onShowHideDashboard()
    {
        if (dashboardAnimation.isPlaying)
        {
            return;
        }
        if (isDashboardShown)
        {
            dashboardAnimation.Play("DashboardCloseAnim");
            isDashboardShown = false;
        }
        else
        {
            dashboardAnimation.Play("DashboardOpenAnim");
            isDashboardShown = true;
        }
    }

    public void onMenuButton()
    {
        if (dashboardMenu == 0)
        {
            return;
        }
        switch (dashboardMenu)
        {
            case 1:
                Overview3D.SetActive(false);
                frontCanvas.SetActive(true);
                break;
            case 2:
                SatelliteState.SetActive(false);
                Logo.SetActive(true);
                SatelliteStateMainPanel.SetActive(false);
                basicInfoAnimation.Play("BasicInfoCloseAnim");
                //SatelliteStateButton.SetActive(false);
                break;
            case 3:
                //SystemInfoButton.SetActive(false);
                break;
            case 4:
                //ControlPanelButton.SetActive(false);
                break;
        }
        Menu.SetActive(true);
        dashboardMenu = 0;
    }

    public void onOverview3DButton()
    {
        dashboardMenu = 1;
        frontCanvas.SetActive(false);
        Menu.SetActive(false);
        Overview3D.SetActive(true);
    }

    public void onSatelliteStateButton()
    {
        dashboardMenu = 2;
        Menu.SetActive(false);
        SatelliteState.SetActive(true);

        Logo.SetActive(false);
        SatelliteStateMainPanel.SetActive(true);
        //SatelliteStateBasicInfo.SetActive(true);
        basicInfoAnimation.Play("BasicInfoOpenAnim");
    }
}
