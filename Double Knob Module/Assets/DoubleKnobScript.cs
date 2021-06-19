using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class DoubleKnobScript : MonoBehaviour {

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMNeedyModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Knobs;
    public TextMesh[] Texts;

    private int[] KnobPos = new int[2];
    private int[] DisplayNumbers = new int[3];
    private int[] DesiredPos = new int[2];
    private int[] ForbiddenKnobState = new int[2];
    private bool Active;
    private bool Flashing;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        StartCoroutine(BrokenScreenFlicker());
        Knobs[0].OnInteract += delegate { KnobPress(0); return false; };
        Knobs[1].OnInteract += delegate { KnobPress(1); return false; };
        Module.OnNeedyActivation += Calculate;
        Module.OnTimerExpired += delegate { Active = false; Flashing = false; if (KnobPos[0] == DesiredPos[0] && KnobPos[1] == DesiredPos[1]) Module.HandlePass(); else { Module.HandleStrike(); Module.HandlePass(); } for (int i = 0; i < 3; i++) { Texts[i].text = ""; } };
    }

    /// <summary>
    /// Handles a knob press.
    /// </summary>
    /// <param name="pos"></param>
    void KnobPress(int pos)
    {
        Knobs[pos].transform.localEulerAngles += new Vector3(0, 90f, 0);
        KnobPos[pos] = (KnobPos[pos] + 1) % 4;
        if (KnobPos[0] == ForbiddenKnobState[0] && KnobPos[1] == ForbiddenKnobState[1] && Active)
        {
            Module.HandleStrike();
            Module.HandlePass();
            Active = false;
            Flashing = false;
            for (int i = 0; i < 3; i++)
                Texts[i].text = "";
        }
    }

    /// <summary>
    /// Calculates each of the restrictions that the screens have.
    /// </summary>
    void Calculate()
    {
        Active = true;
        for (int i = 0; i < 3; i++)
        {
            DisplayNumbers[i] = Rnd.Range(0, 16);
            Texts[i].text = Convert.ToString(DisplayNumbers[i], 2);
            while (Texts[i].text.Length != 4)
            {
                string Cache = "0";
                Cache += Texts[i].text;
                Texts[i].text = Cache;
            }
            Texts[i].text = Texts[i].text.Replace('0', ' ');
        }
        DesiredPos[0] = (Mathf.FloorToInt(DisplayNumbers[0] / 4) + Mathf.FloorToInt(DisplayNumbers[1] / 4)) % 4;
        DesiredPos[1] = ((DisplayNumbers[0] % 4) + (DisplayNumbers[1] % 4)) % 4;
        ForbiddenKnobState[0] = DesiredPos[0];
        ForbiddenKnobState[1] = DesiredPos[1];
        while (ForbiddenKnobState[0] == DesiredPos[0] || ForbiddenKnobState[1] == DesiredPos[1])
        {
            DisplayNumbers[2] = Rnd.Range(0, 16);
            Texts[2].text = Convert.ToString(DisplayNumbers[2], 2);
            while (Texts[2].text.Length != 4)
            {
                string Cache = "0";
                Cache += Texts[2].text;
                Texts[2].text = Cache;
            }
            Texts[2].text = Texts[2].text.Replace('0', ' ');
            ForbiddenKnobState[0] = Mathf.FloorToInt(DisplayNumbers[2] / 4);
            ForbiddenKnobState[1] = DisplayNumbers[2] % 4;
        }
    }

    /// <summary>
    /// Makes the useless screen be broken.
    /// </summary>
    /// <returns></returns>
    private IEnumerator BrokenScreenFlicker()
    {
        while (true)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!Flashing)
                    Texts[i].color = new Color(1, 1, 1, Rnd.Range(0.8f, 1f));
            }
            Texts[3].color = new Color(1, 1, 1, Rnd.Range(0, 0.15f));
            Texts[3].text = Convert.ToString(Rnd.Range(0, 16), 2);
            while (Texts[3].text.Length != 4)
            {
                string Cache = "0";
                Cache += Texts[3].text;
                Texts[3].text = Cache;
            }
            Texts[3].text = Texts[3].text.Replace('0', ' ');
            if (Module.GetNeedyTimeRemaining() <= 5.5 && !Flashing && Active)
            {
                Flashing = true;
                StartCoroutine(TextFlash());
            }
            yield return null;
        }
    }

    private IEnumerator TextFlash()
    {
        while (Flashing)
        {
            for (int i = 0; i < 3; i++)
            {
                Texts[i].color = new Color(1, 1, 1, 0.25f);
            }
            yield return new WaitForSeconds(0.1f);
            for (int i = 0; i < 3; i++)
            {
                Texts[i].color = new Color(1, 1, 1, 1);
            }
            yield return new WaitForSeconds(0.1f);
        }
        for (int i = 0; i < 3; i++)
        {
            Texts[i].color = new Color(1, 1, 1, 1);
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} 1 2' to turn the left knob once and the right knob twice.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] CommandArray = command.Split(' ');
        string[] ValidCommands = { "0", "1", "2", "3" };
        if (CommandArray.Length != 2 || !ValidCommands.Contains(CommandArray[0]) || !ValidCommands.Contains(CommandArray[1]))
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        yield return null;
        for (int i = 0; i < int.Parse(CommandArray[0]); i++)
        {
            Knobs[0].OnInteract();
            yield return null;
        }
        for (int i = 0; i < int.Parse(CommandArray[1]); i++)
        {
            Knobs[1].OnInteract();
            yield return null;
        }
    }
}
