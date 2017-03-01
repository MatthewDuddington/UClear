using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
//    // Easy accessor for the class instance
//    private static ScreenManager screenManager_;
//    public static ScreenManager Get
//    { 
//        get
//        {
//            if (screenManager_ == null)
//            {
//                Debug.LogError("No ScreenManager present in scene");
//            }
//            return screenManager_;
//        }
//        private set { screenManager_ = value; }
//    } 

    public delegate void GameEvent();
    public static event GameEvent OnNewGame;
    public static event GameEvent OnExitGame;

    public enum Screens { TitleScreen, GameScreen, ResultScreen, NumScreens }
    
    private Canvas [] mScreens;
    private Screens mCurrentScreen;

    private Image RadiationBar;
    private Image DecontamMarks;
    private Image DecontamBar;

    void Awake()
    {
        mScreens = new Canvas[(int)Screens.NumScreens];
        Canvas[] screens = GetComponentsInChildren<Canvas>();
        for (int count = 0; count < screens.Length; ++count)
        {
            for (int slot = 0; slot < mScreens.Length; ++slot)
            {
                if (mScreens[slot] == null && ((Screens)slot).ToString() == screens[count].name)
                {
                    mScreens[slot] = screens[count];
                    break;
                }
            }
        }

        for (int screen = 1; screen < mScreens.Length; ++screen)
        {
            mScreens[screen].enabled = false;
        }
        
        RadiationBar = GameObject.Find("RadiationBar").GetComponent<Image>();
        DecontamBar = GameObject.Find("DecontamBar").GetComponent<Image>();

        mCurrentScreen = Screens.TitleScreen;
    }

    public void StartGame()
    {
        if(OnNewGame != null)
        {
            OnNewGame();
        }

        TransitionTo(Screens.GameScreen);
    }

    public void EndGame()
    {
        if (OnExitGame != null)
        {
            OnExitGame();
        }

        TransitionTo(Screens.ResultScreen);
    }

    private void TransitionTo(Screens screen)
    {
        mScreens[(int)mCurrentScreen].enabled = false;
        mScreens[(int)screen].enabled = true;
        mCurrentScreen = screen;
    }

    public void FillRadiationMeter(float amount)
    {
        RadiationBar.fillAmount += amount;
    }

    public void FillResearchMeter()
    {
        DecontamBar.fillAmount += 1 / GameManager.Get.numberOfResearchersToWin;
        DecontamMarks.fillAmount = 1/DecontamBar.fillAmount;
    }
}
