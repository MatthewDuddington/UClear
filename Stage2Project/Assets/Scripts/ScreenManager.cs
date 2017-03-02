using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    public delegate void GameEvent();
    public static event GameEvent OnNewGame;
    public static event GameEvent OnExitGame;

    public enum Screens { TitleScreen, InstructionsScreen, GameScreen, ResultScreen, NumScreens }
    
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

        RadiationBar.fillAmount = 0;
        DecontamBar.fillAmount = 0;
        DecontamMarks.fillAmount = 1;

        TransitionTo(Screens.GameScreen);
    }

    public void Instructions()
    {
        TransitionTo(Screens.InstructionsScreen);
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
        if (RadiationBar.fillAmount >= 1)
        {
            print("You lose");
            EndGame();
        }
    }

    public void FillDecontamMeter()
    {
        DecontamBar.fillAmount += 0.1f;
        DecontamMarks.fillAmount = 1 - DecontamBar.fillAmount;

        if (DecontamBar.fillAmount >= 1)
        {
            print("You win");
            EndGame();
        }
    }

    private void GameManager_OnRadiationDamage(float radiationAmount)
    {
        FillRadiationMeter(radiationAmount);
    }

    private void Agent_OnDecontamination()
    {
        print("decontamination");
        FillDecontamMeter();
    }
}
