using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
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

        GameManager.OnRadiationDamage += GameManager_OnRadiationDamage;
        Agent.OnDecontamination += Agent_OnDecontamination;
        
        RadiationBar  = GameObject.Find("RadiationBar").GetComponent<Image>();
        DecontamBar   = GameObject.Find("DecontamBar").GetComponent<Image>();
        DecontamMarks = GameObject.Find("DecontamMarks").GetComponent<Image>();

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

    public void FillDecontamMeter()
    {
        DecontamBar.fillAmount += 1 / GameManager.Get.numberOfResearchersToWin;
        DecontamMarks.fillAmount = 1 / DecontamBar.fillAmount;
    }

    private void GameManager_OnRadiationDamage(int radiationAmount)
    {
        FillRadiationMeter(radiationAmount);
    }

    private void Agent_OnDecontamination()
    {
        FillDecontamMeter();
    }
}
