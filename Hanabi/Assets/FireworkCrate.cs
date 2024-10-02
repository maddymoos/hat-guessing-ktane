using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
using KeepCoding;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using UnityEngine.XR.WSA.Input;
using Wawa.Recall;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using System.IO;
using UnityEditorInternal;
using Newtonsoft.Json.Linq;
public class FireworkCrate : MonoBehaviour
{

    static private int _moduleIdCounter = 1;
    private int _moduleId;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Bomb;

    public MeshRenderer Background;
    private int bgoffset;
    private float speed = 5;

    public Transform[] ModifierButtons;
    public KMSelectable StartButton;
    public KMSelectable[] PlayButtons;

    public Transform PlayScale;
    public Transform SetupScale;

    public Color[] SelectButtonColors;
    public Color[] SuitColors;
    public Color[] NumberColors;
    public Sprite[] CardBacks;
    public Sprite[] SuitImages;
    public Sprite[] NumberNumbers;


    public Transform[] WiggleTransforms;
    private float[] wigglenums;
    private bool[] wiggleup;

    public Transform[] Psychic;
    public Transform[] Cards;
    public TextMesh CardCounter;
    public TextMesh[] PlayDisplay;
    public TextMesh[] HighScoreDisplay;
    public TextMesh[] BestHandDisplay;
    public TextMesh[] TotalScore;
    public TextMesh[] AddedScore;
    public SpriteRenderer SuitClue;
    public SpriteRenderer RiffRaffOverlay;
    private bool[] green = new bool[2];
    private bool[] over = new bool[2];
    private bool[] ActiveModifiers;
    private bool started;
    private bool play;
    private bool animating;
    private bool anyclue;
    private bool newbest;
    private int Round;
    private int OmniStreak;

    private bool Solved;
    public TextMesh[] SolvedText;

    private int Cluing = 0;
    private int ClueValue = 0;
    private int discards = 4;
    private int totalscore = 0;

    private int edging = 1;
    private int riffrafftimer = 0;
    private int ledge = 0;

    private string lasthand = "";
    private List<string> playedhands = new List<string>();

    private Vector3 NullHand = new Vector3(0, .016f, 0f);
    private Vector3 Zero = new Vector3(0, 0, 1);
    private Vector3 One = new Vector3(0.005f, 0.005f, 1);
    private Vector3[][] HandPositions =
    {
        new Vector3[]{ new Vector3(0,.016f,-.055f)},
        new Vector3[]{ new Vector3(-.02f,.016f,-.055f),new Vector3(.02f,.016f,-.055f)},
        new Vector3[]{ new Vector3(-.03f,.016f,-.055f),new Vector3(0f,.016f,-.055f),new Vector3(.03f,.016f,-.055f)},
        new Vector3[]{ new Vector3(-.045f,.016f,-.055f),new Vector3(-.015f,.016f,-.055f),new Vector3(.015f,.016f,-.055f),new Vector3(.045f,.016f,-.055f)},
        new Vector3[]{ new Vector3(-.06f,.016f,-.055f),new Vector3(-.03f,.016f,-.055f),new Vector3(0f,.016f,-.055f),new Vector3(.03f,.016f,-.055f),new Vector3(.06f,.016f,-.055f)},
    };
    private List<string> ActiveSuit = new List<string>();
    private List<Tuple<string, int>> Deck = new List<Tuple<string, int>>();
    private List<Tuple<string, int>> HandCards = new List<Tuple<string, int>>();
    private List<string> RealActiveSuit = new List<string>();

    public AudioSource[] LoopingSounds;

    private Coroutine[] ButtonAnims = new Coroutine[3];
    private Coroutine ChipCountCoroutine;

    private string[] SuitOrder =
    {
        "Red",
        "Yellow",
        "Green",
        "Blue",
        "Purple",
        "Teal",
        "Black",
        "Rainbow",
        "Brown",
        "White",
        "Pink",
        "Omni",
        "Muddy Rainbow",
        "Null",
        "Light Pink",
        "Dark Null",
        "Dark Brown",
        "Cocoa Rainbow",
        "Gray",
        "Dark Rainbow",
        "Gray Pink",
        "Dark Pink",
        "Dark Omni",
        "Risky Dice",
        "Riff-Raff"
    };
    private string[] Modifiers =
    {
        "Red",
        "Yellow",
        "Green",
        "Blue",
        "Purple",
        "Teal",
        "Black",
        "Rainbow",
        "Brown",
        "White",
        "Pink",
        "Omni",
        "Muddy Rainbow",
        "Dark Rainbow",
        "Null",
        "Light Pink",
        "Dark Null",
        "Dark Brown",
        "Gray",
        "Dark Omni",
        "Risky Dice",
        "Riff-Raff"
    };
    private string[] Colorless =
    {
        "White",
        "Null",
        "Light Pink",
        "Gray",
        "Gray Pink",
        "Dark Null",
        "Riff-Raff"
    };
    private string[] Multicolor =
    {
        "Rainbow",
        "Omni",
        "Muddy Rainbow",
        "Dark Rainbow",
        "Cocoa Rainbow",
        "Dark Omni"
    };
    private string[] Numberless =
    {
        "Brown",
        "Null",
        "Muddy Rainbow",
        "Dark Brown",
        "Dark Null",
        "Cocoa Rainbow",
        "Riff-Raff"
    };
    private string[] EveryNumber =
    {
        "Pink",
        "Omni",
        "Light Pink",
        "Dark Pink",
        "Dark Omni",
        "Gray Pink"
    };
    private string[] Singlet =
    {
        "Black",
        "Dark Null",
        "Dark Brown",
        "Cocoa Rainbow",
        "Dark Rainbow",
        "Gray Pink",
        "Gray",
        "Dark Pink",
        "Dark Omni",
    };
    private Dictionary<string, Tuple<string, string>> ModifierList = new Dictionary<string, Tuple<string, string>>
    {
        {"Red",           "Anti-Red (-1 Suit)",        "Fives +15"},
        {"Yellow",        "Anti-Yellow (-1 Suit)",     "Straight x1.2"},
        {"Green",         "Anti-Green (-1 Suit)",      "Pairs +5"},
        {"Blue",          "Anti-Blue (-1 Suit)",       "Full House x1.2"},
        {"Purple",        "Anti-Purple (-1 Suit)",     "Threes Wild Colors"},
        {"Teal",          "Teal (+1 Suit)",            "Flush x1.2"},
        {"Black",         "Black (+1 [X] Suit)",       "Black +10"},
        {"Rainbow",       "Rainbow (+1 [M] Suit)",     "Rainbows Wild Color"},
        {"Brown",         "Brown (+1 [0] Suit)",       "3 Of A Kind x1.3"},
        {"White",         "White (+1 [C] Suit)",       "Ignored For Flush"},
        {"Pink",          "Pink (+1 [#] Suit)",        "Pink in Pairs x1.5"},
        {"Omni",          "Omni (+1 [#M] Suit)",       "Every Omni x1.75"},
        {"Muddy Rainbow", "Mud R. (+1 [0M] Suit)",     "Unused Muddy R. x2"},
        {"Null",          "Null (+1 [0C] Suit)",       "Single Null +Discard"},
        {"Light Pink",    "L Pink (+1 [#C] Suit)",     "L. Pink +5xL. Pink"},
        {"Dark Null",     "D Null (+1 [S0C] Suit)",    "One-Time Use x2.5"},
        {"Dark Brown",    "D Brown (+1 [S0] Suit)",    "Straights Need 4"},
        {"Cocoa Rainbow", "Cocoa R (+1 [S0M] Suit)",   " "},
        {"Gray",          "Gray (+1 [SC] Suit)",       "Flushes Need 4"},
        {"Dark Rainbow",  "Dark R (+1 [SM] Suit)",     "See The Future"},
        {"Gray Pink",     "G Pink (+1 [S#C] Suit)",    " "},
        {"Dark Pink",     "D Pink (+1 [S#] Suit)",     " "},
        {"Dark Omni",     "D Omni (+1 [S#M] Suit)",    "Hands x1.5^Streak"},
        {"Risky Dice",    "Risky Dice (+1 6 Suit)",    "Risky Dice are 6"},
        {"Riff-Raff",     "Riff-Raff [E]",             "+2 to 4 Modifiers"}
    };

    private float ButtonEasing(float t)
    {
        if (t < 0.5f)
            return Easing.InOutSine(t * 2, 1, 0, 1);
        if (t < 0.8f)
            return Easing.InOutSine((t - 0.5f) / 0.3f, 0, 1.25f, 1);
        return Easing.InOutSine((t - 0.8f) / 0.2f, 1.25f, 1, 1);
    }

    void Awake()
    {

        //do you have QKRisi's? if not, ya ur outdated
        _moduleId = _moduleIdCounter++;
        for (int j = 0; j < 2; j++)
        {
            int i = j;
            ModifierButtons[i].GetComponent<KMSelectable>().OnInteract += delegate
            {
                if (!started)
                    ModifierSelect(i);
                return false;
            };
            ModifierButtons[i].GetComponent<KMSelectable>().OnHighlight += delegate
            {
                if (!started)

                    HL(true, i);
            };
            ModifierButtons[i].GetComponent<KMSelectable>().OnHighlightEnded += delegate
            {
                HL(false, i);
            };
        }
        StartButton.OnHighlight += delegate
        {
            if (!started)

                StartButton.transform.Find("Highlight").GetComponent<SpriteRenderer>().enabled = true;
        };
        StartButton.OnHighlightEnded += delegate
        {
            StartButton.transform.Find("Highlight").GetComponent<SpriteRenderer>().enabled = false;
        };
        StartButton.OnInteract += delegate
        {
            if (!started)

                GenerateDeck();
            return false;
        };
        PlayButtons[0].OnInteract += delegate
        {
            StartCoroutine(Clue(true));
            return false;
        };
        PlayButtons[1].OnInteract += delegate
        {
            StartCoroutine(Clue(false));
            return false;
        };
        PlayButtons[2].OnInteract += delegate
        {
            //Here is where I suffer for the next 10 hours.
            StartCoroutine(PlayHand());
            return false;
        };
    }

    IEnumerator ButtonAnim(Transform trans, float duration = 0.2f)
    {
        //Debug.Log(trans.name);
        trans.localScale = Vector3.one * 0.05f;
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            trans.localScale = Vector3.one * 0.05f * ((ButtonEasing(timer / duration) / 10) + 0.9f);
        }
        trans.localScale = Vector3.one * 0.05f;
    }

    IEnumerator PlayHand()
    {
        Cluing = 0;
        play = false;
        string playedhand = "";
        int score = 0;
        List<int> ActiveCards = new List<int>();
        yield return null;
        if (CountNum(HandCards, 5) == 5)
        {
            playedhand = "Five Fives";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 500;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.FiveFives[Rnd.Range(0, HandResponses.FiveFives.Count())]);
        }
        else if (CountNum(HandCards, HandCards[0].Item2) == 5 && Flush(HandCards))
        {
            playedhand = "Flush Five";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 300;
            for(int i=0;i<5;i++)
            if (ActiveModifiers[18] && HandCards.Where(x => x.Item1 == HandCards[i].Item1 || x.Item1 == "Rainbow" || x.Item1 == "White" || (x.Item2 == 3 && ActiveModifiers[4])).Count() == 4)
            {
                Audio.PlaySoundAtTransform("DWhiteSplash", transform);
                Debug.LogFormat("[Hanabi Poker #{0}]: You have been blessed by Gray! This hand is a Flush.", _moduleId);
                    yield return new WaitForSeconds(.4f);
                    break;
            }
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.FlushFive[Rnd.Range(0, HandResponses.FlushFive.Count())]);
        }
        else if (Flush(HandCards) && Straight(HandCards))
        {
            playedhand = "Straight Flush";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 250;
            for (int i = 0; i < 5; i++)
                if (ActiveModifiers[18] && HandCards.Where(x => x.Item1 == HandCards[i].Item1 || x.Item1 == "Rainbow" || x.Item1 == "White" || (x.Item2 == 3 && ActiveModifiers[4])).Count() == 4)
                {
                    Audio.PlaySoundAtTransform("DWhiteSplash", transform);
                    Debug.LogFormat("[Hanabi Poker #{0}]: You have been blessed by Gray! This hand is a Flush.", _moduleId);
                    yield return new WaitForSeconds(.4f);
                    break;
                }
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.StraightFlush[Rnd.Range(0, HandResponses.StraightFlush.Count())]);

        }
        else if (Flush(HandCards) && FullHouse(HandCards))
        {
            playedhand = "Flush House";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 100;
            for (int i = 0; i < 5; i++)
                if (ActiveModifiers[18] && HandCards.Where(x => x.Item1 == HandCards[i].Item1 || x.Item1 == "Rainbow" || x.Item1 == "White" || (x.Item2 == 3 && ActiveModifiers[4])).Count() == 4)
                {
                    Audio.PlaySoundAtTransform("DWhiteSplash", transform);
                    Debug.LogFormat("[Hanabi Poker #{0}]: You have been blessed by Gray! This hand is a Flush.", _moduleId);
                    yield return new WaitForSeconds(.4f);
                    break;
                }
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.FlushHouse[Rnd.Range(0, HandResponses.FlushHouse.Count())]);
        }
        else if (Straight(HandCards))
        {
            playedhand = "Straight";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 50;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.Straight[Rnd.Range(0, HandResponses.Straight.Count())]);
        }
        else if (Flush(HandCards))
        {
            playedhand = "Flush";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 40;
            for (int i = 0; i < 5; i++)
                if (ActiveModifiers[18] && HandCards.Where(x => x.Item1 == HandCards[i].Item1 || x.Item1 == "Rainbow" || x.Item1 == "White" || (x.Item2 == 3 && ActiveModifiers[4])).Count() == 4)
                {
                    Audio.PlaySoundAtTransform("DWhiteSplash", transform);
                    Debug.LogFormat("[Hanabi Poker #{0}]: You have been blessed by Gray! This hand is a Flush.", _moduleId);
                    yield return new WaitForSeconds(.4f);
                    break;
                }
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.Flush[Rnd.Range(0, HandResponses.Flush.Count())]);

        }
        else if (CountNum(HandCards, 1) == 5)
        {
            playedhand = "Five Ones";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 40;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.FiveOnes[Rnd.Range(0, HandResponses.FiveOnes.Count())]);
        }
        else if (CountNum(HandCards, HandCards[0].Item2) == 5)
        {
            playedhand = "Five Of A Kind";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 45;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.FiveOAK[Rnd.Range(0, HandResponses.FiveOAK.Count())]);
        }
        else if (CountNum(HandCards, NotAlone(HandCards)[0]) == 4)
        {
            playedhand = "Four Of A Kind";
            ActiveCards = NumIndexes(HandCards, NotAlone(HandCards)[0]);
            score = 30;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.FourOAK[Rnd.Range(0, HandResponses.FourOAK.Count())]);
        }
        else if (FullHouse(HandCards))
        {
            playedhand = "Full House";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 25;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}! It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.FullHouse[Rnd.Range(0, HandResponses.FullHouse.Count())]);
        }
        else if (CountNum(HandCards, NotAlone(HandCards)[0]) == 3)
        {
            playedhand = "Three Of A Kind";
            ActiveCards = NumIndexes(HandCards, NotAlone(HandCards)[0]);
            score = 15;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}. It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.ThreeOAK[Rnd.Range(0, HandResponses.ThreeOAK.Count())]);
        }
        else if (CountNum(HandCards, NotAlone(HandCards)[0]) == 2 && CountNum(HandCards, NotFirstNumberButWithListInt(NotAlone(HandCards))[0]) == 2)
        {
            playedhand = "Two Pair";
            ActiveCards = NumIndexes(HandCards, NotAlone(HandCards)[0]);
            foreach (int index in NumIndexes(HandCards, NotFirstNumberButWithListInt(NotAlone(HandCards))[0]))
                ActiveCards.Add(index);
            score = 10;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}. It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.TwoPair[Rnd.Range(0, HandResponses.TwoPair.Count())]);
        }
        else if (CountNum(HandCards, NotAlone(HandCards)[0]) == 2)
        {
            playedhand = "Pair";
            ActiveCards = NumIndexes(HandCards, NotAlone(HandCards)[0]);
            score = 5;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played a {1}. It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.Pair[Rnd.Range(0, HandResponses.Pair.Count())]);
        }
        else
        {
            playedhand = "Trash!";
            ActiveCards = new List<int> { 0, 1, 2, 3, 4 };
            score = 1;
            Debug.LogFormat("[Hanabi Poker #{0}]: You played {1} It scores a base of {2} chips. {3}", _moduleId, playedhand, score, HandResponses.Trash[Rnd.Range(0, HandResponses.Trash.Count())]);
        }

        if (ActiveSuit.Count() > 5)
            score = (int)(score * (5f / ActiveSuit.Count()));
        if (ActiveSuit.Count() > 5)
            Debug.LogFormat("[Hanabi Poker #{0}]: Due to your deck having {1} suits, your earned chips are now {2}.", _moduleId, ActiveSuit.Count(), score);
        for (int i = 0; i < 2; i++)
        {
            PlayDisplay[i].color = new Color(1 - i, 1 - i, 1 - i, 1);
            AddedScore[i].color = new Color(1 - i, 1 - i, 1 - i, 1);
            TotalScore[i].color = new Color(1 - i, 1 - i, 1 - i, 1);
            PlayDisplay[i].text = playedhand;
            AddedScore[i].text = "+" + score;
        }
        if (score != 0 && score < 20)
            Audio.PlaySoundAtTransform("SmallHit", transform);
        else if (score < 45)
            Audio.PlaySoundAtTransform("MidHit", transform);
        else
            Audio.PlaySoundAtTransform("BigHit", transform);
        yield return new WaitForSeconds(0.45f);
        int k = 0;
        float gaptime = .5f;
        float lowergap = .15f;
        float mingap = .15f;
        foreach (Tuple<string, int> card in HandCards)
        {
            if (ActiveCards.Contains(k))
            {
                if (ActiveModifiers[0] && card.Item2 == 5)
                {
                    score += 15;
                    AddedScore[0].text = "+" + score;
                    AddedScore[1].text = "+" + score;
                    Audio.PlaySoundAtTransform("SmallHit", transform);
                    yield return new WaitForSeconds(gaptime);
                    if(gaptime > mingap)
                    gaptime -= lowergap;
                    Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Anti-Red modifier, the used 5 added fifteen chips! Your earned chips are now {1}!", _moduleId, score);

                }
                if (ActiveModifiers[6] && card.Item1 == "Black")
                {
                    score += 10;
                    AddedScore[0].text = "+" + score;
                    AddedScore[1].text = "+" + score;
                    Audio.PlaySoundAtTransform("SmallHit", transform);
                    yield return new WaitForSeconds(gaptime);
                    if (gaptime > mingap)
                        gaptime -= lowergap;
                    Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Black modifier, the used black card added ten chips! Your earned chips are now {1}!", _moduleId, score);

                }
                if (ActiveModifiers[14] && card.Item1 == "Light Pink")
                {
                    score += 5 * HandCards.Where(x => x.Item1 == "Light Pink").Count();
                    AddedScore[0].text = "+" + score;
                    AddedScore[1].text = "+" + score;
                    Audio.PlaySoundAtTransform("SmallHit", transform);
                    yield return new WaitForSeconds(gaptime);
                    if (gaptime > mingap)
                        gaptime -= lowergap;
                    Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Light Pink modifier, the used light pink card added {2} chips! Your earned chips are now {1}!", _moduleId, score, 5 * HandCards.Where(x => x.Item1 == "Light Pink").Count());

                }
            }
            k++;
        }
        if (ActiveModifiers[2] && CountNum(HandCards, NotAlone(HandCards)[0]) >= 2)
        {
            score += 5;
            AddedScore[0].text = "+" + score;
            AddedScore[1].text = "+" + score;
            Audio.PlaySoundAtTransform("SmallHit", transform);
            yield return new WaitForSeconds(gaptime);
            if (gaptime > mingap)
                gaptime -= lowergap;
            Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Anti-Green modifier, the pair in your hand added five chips! Your earned chips are now {1}!", _moduleId, score);

        }
        if (ActiveModifiers[13] && HandCards.Where(x => x.Item1 == "Null").Count() == 1)
        {
            discards++;
            yield return new WaitForSeconds(gaptime);
            if (gaptime > mingap)
                gaptime -= lowergap;
            Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Null modifier, your lone Null gifted you a discard! Your earned chips are now {1}!", _moduleId, score);

        }



        //Mult
        if (ActiveModifiers[1] && Straight(HandCards))
        {
            score = (int)(score * 1.2);
            AddedScore[0].text = "+" + score;
            AddedScore[1].text = "+" + score;
            Audio.PlaySoundAtTransform("SmallHit", transform);
            yield return new WaitForSeconds(gaptime);
            if (gaptime > mingap)
                gaptime -= lowergap;
            Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Anti-Yellow modifier, your straight is multiplied by 1.2x! Your earned chips are now {1}!", _moduleId, score);
        }
        if (ActiveModifiers[3] && FullHouse(HandCards))
        {
            score = (int)(score * 1.2);
            AddedScore[0].text = "+" + score;
            AddedScore[1].text = "+" + score;
            Audio.PlaySoundAtTransform("SmallHit", transform);
            yield return new WaitForSeconds(gaptime);
            if (gaptime > mingap)
                gaptime -= lowergap;
            Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Anti-Blue modifier, your full house is multiplied by 1.2x! Your earned chips are now {1}!", _moduleId, score);
        }
        if (ActiveModifiers[5] && Flush(HandCards))
        {
            score = (int)(score * 1.2);
            AddedScore[0].text = "+" + score;
            AddedScore[1].text = "+" + score;
            Audio.PlaySoundAtTransform("SmallHit", transform);
            yield return new WaitForSeconds(gaptime);
            if (gaptime > mingap)
                gaptime -= lowergap;
            Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Teal modifier, your flush is multiplied by 1.2x! Your earned chips are now {1}!", _moduleId, score);
        }
        if (ActiveModifiers[8] && (CountNum(HandCards, NotAlone(HandCards)[0]) >= 3 || FullHouse(HandCards)))
        {
            score = (int)(score * 1.3);
            AddedScore[0].text = "+" + score;
            AddedScore[1].text = "+" + score;
            Audio.PlaySoundAtTransform("SmallHit", transform);
            yield return new WaitForSeconds(gaptime);
            if (gaptime > mingap)
                gaptime -= lowergap;
            Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Brown modifier, the three of a kind in your hand multiplies it by 1.3x! Your earned chips are now {1}!", _moduleId, score);
        }
        for (int i = 0; i < 5; i++)
        {
            if (ActiveModifiers[10] && HandNumbers(HandCards).Where(x => HandNumbers(HandCards).Where(y => x == y).Count() > 1).Contains(HandCards[i].Item2) && HandCards[i].Item1 == "Pink")
            {
                score = (int)(score * 1.5);
                AddedScore[0].text = "+" + score;
                AddedScore[1].text = "+" + score;
                Audio.PlaySoundAtTransform("SmallHit", transform);
                yield return new WaitForSeconds(gaptime);
                if (gaptime > mingap)
                    gaptime -= lowergap;
                Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Pink modifier, the pink in a pair multiplies your chips by 1.5x! Your earned chips are now {1}!", _moduleId, score);
            }
            if (ActiveModifiers[11] && HandCards[i].Item1 == "Omni" && ActiveCards.Contains(i))
            {
                score = (int)(score * 1.75);
                AddedScore[0].text = "+" + score;
                AddedScore[1].text = "+" + score;
                Audio.PlaySoundAtTransform("SmallHit", transform);
                yield return new WaitForSeconds(gaptime);
                if (gaptime > mingap)
                    gaptime -= lowergap;
                Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Omni modifier, the omni multiplies your chips by 1.75x! Your earned chips are now {1}!", _moduleId, score);
            }
            if (ActiveModifiers[12] && HandCards[i].Item1 == "Muddy Rainbow" && !ActiveCards.Contains(i))
            {
                score = (int)(score * 2);
                AddedScore[0].text = "+" + score;
                AddedScore[1].text = "+" + score;
                Audio.PlaySoundAtTransform("SmallHit", transform);
                yield return new WaitForSeconds(gaptime);
                if (gaptime > mingap)
                    gaptime -= lowergap;
                Debug.LogFormat("[Hanabi Poker #{0}]: Due to the Muddy Rainbow modifier, the unused muddy rainbow multiplies your chips by 2x! Your earned chips are now {1}!", _moduleId, score);
            }
        }
        if(!playedhands.IsNullOrEmpty() && playedhands.Last() == playedhand && ActiveModifiers[22])
        {
            if(OmniStreak == 0)
            {
                OmniStreak = 1;
            }
            OmniStreak++;
            LoopingSounds[0].Play();
            for (int i = 0; i < OmniStreak; i++)
            {
                score = (int)(score * 1.5f);
                AddedScore[0].text = "+" + score;
                AddedScore[1].text = "+" + score;
                yield return new WaitForSeconds(0.9f);
            }
            LoopingSounds[0].Stop();
            Debug.LogFormat("[Hanabi Poker #{0}]: You have been blessed by Dark Omni! With your streak of {2}, your chips are multiplied by x1.5 ^ {2} to {1}.", _moduleId, score, OmniStreak);
            
        }
        else
        {
            OmniStreak = 0;
        }
        if (playedhands.Contains(playedhand) && ActiveModifiers[15])
        {
            score = 0;
            AddedScore[0].text = "+" + score;
            AddedScore[1].text = "+" + score;
            Audio.PlaySoundAtTransform("DNullShatter", transform);
            yield return new WaitForSeconds(0.5f);
            Debug.LogFormat("[Hanabi Poker #{0}]: You have been cursed by Dark Null. You've already used this hand.", _moduleId, score);
        }
        else if (ActiveModifiers[15])
        {
            score = (int)(score * 2.5);
            AddedScore[0].text = "+" + score;
            AddedScore[1].text = "+" + score;
            Audio.PlaySoundAtTransform("DNullUse", transform);
            yield return new WaitForSeconds(0.5f);
            Debug.LogFormat("[Hanabi Poker #{0}]: You have been blessed by Dark Null! Your chips are multiplied by x2.5 to {1}.", _moduleId, score);
            
        }
        playedhands.Add(playedhand);



       
        //Debug.LogFormat("[Hanabi Poker #{0}]: This leaves a total of {1} chips! Nice hand.", _moduleId, score);
        string BHText = File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPBestHand.txt"));
        int besthand = int.Parse(Regex.Match(BHText, @"\n([1234567890]+)").ToString());
        if (score > besthand)
        {
            newbest = true;
            Debug.LogFormat("[Hanabi Poker #{0}]: That's a new best hand!", _moduleId);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "HPBestHand.txt"), "Best Hand\n" + score + "\n" + playedhand + ActiveModifierNames());
        }
        if(score > (ActiveModifiers[24] ? 225 : 150))
        {
            Audio.PlaySoundAtTransform("holyheck", transform);
            Debug.LogFormat("[Hanabi Poker #{0}]: HOLY HECK!", _moduleId);
            yield return new WaitForSeconds(.25f);
        }
        yield return new WaitForSeconds(.25f);
        var benchmarkA = (score + 3) % 4;
        var benchmarkB = (score + 2) % 3;
        while (score > 0)
        {
            totalscore += 1;
            score -= 1;

            if (score % 3 == 0)
                Audio.PlaySoundAtTransform(score % 4 == benchmarkA ? "TickBig" : "TickSmall", transform);
            AddedScore[0].text = "+" + score;
            AddedScore[1].text = "+" + score;
            TotalScore[1].text = totalscore.ToString();
            TotalScore[0].text = totalscore.ToString();
            yield return new WaitForSeconds(.03f);
        }
        Audio.PlaySoundAtTransform("Bank", transform);
        Debug.LogFormat("[Hanabi Poker #{0}]: After adding to your total chips, you now have {1}.", _moduleId, totalscore);
        if (totalscore >= (ActiveModifiers[24] ? 225 : 150) && !Solved)
        {
            yield return new WaitForSeconds(.15f);
            Audio.PlaySoundAtTransform("ScoreThresholdReached", transform);
            Debug.LogFormat("[Hanabi Poker #{0}]: And that's all you need! Chip goal reached. Module solved, but you can keep playing if you like.", _moduleId);
            Solved = true;
            Module.HandlePass();
            SolvedText[0].color = Color.white;
            SolvedText[1].color = Color.black;
        }
        yield return new WaitForSeconds(.2f);
        float wer = 0;
        while (wer < 1)
        {
            yield return null;
            wer += Time.deltaTime * 2f;
            for (int i = 0; i < 2; i++)
            {
                PlayDisplay[i].color = new Color(1 - i, 1 - i, 1 - i, 1 - wer);
                AddedScore[i].color = new Color(1 - i, 1 - i, 1 - i, 1 - wer);
                TotalScore[i].color = new Color(1 - i, 1 - i, 1 - i, 1 - wer);
            }
        }
        for (int i = 0; i < 2; i++)
        {
            PlayDisplay[i].color = new Color(1 - i, 1 - i, 1 - i, 0);
            AddedScore[i].color = new Color(1 - i, 1 - i, 1 - i, 0);
            TotalScore[i].color = new Color(1 - i, 1 - i, 1 - i, 0);
        }

        discards += 1;
        animating = true;
        play = false;
        reset:
        foreach (var card in HandCards)
        {
            Audio.PlaySoundAtTransform("Succ", transform);
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 8f;
                for (int i = 0; i < HandCards.Count(); i++)
                {
                    if (i == 0)
                    {
                        Cards[i].localPosition = Vector3.LerpUnclamped(HandPositions[HandCards.Count() - 1][i], NullHand, easeInSine(t));
                        Cards[i].localScale = Vector3.LerpUnclamped(One, Zero, easeInSine(t));
                    }
                    else
                    {
                        Cards[i].localPosition = Vector3.LerpUnclamped(HandPositions[HandCards.Count() - 1][i], HandPositions[HandCards.Count() - 2][i - 1], EaseOutBack(0, 1, t));
                    }
                }
                yield return null;
            }
            HandCards.RemoveAt(0);
            for (int i = 0; i < 5; i++)
            {
                if (HandCards.Count() > i)
                {
                    Cards[i].localPosition = HandPositions[HandCards.Count() - 1][i];
                    Cards[i].localScale = One;
                    CardSuit(Cards[i]).sprite = SuitImages[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                    CardNumber(Cards[i]).sprite = NumberNumbers[HandCards[i].Item2 - 1];
                    CardNumber(Cards[i]).color = NumberColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                    CardRenderer(Cards[i]).color = SuitColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                }
                else
                {
                    Cards[i].localPosition = new Vector3(0, 0, 0);
                    Cards[i].localScale = Zero;
                }
            }
            if (HandCards.Count() > 0)
                goto reset;
            else
            {
                break;
            }

        }
        yield return new WaitForSeconds(.2f);
        animating = false;
        StartCoroutine(DrawToFive());
    }

    List<int> NotAlone(List<Tuple<string, int>> hand)
    {
        var x = HandNumbers(hand);
        if (x.Where(y => x.Where(z => z == y).Count() != 1).Count() == 0)
        {
            return new List<int> { 0 };
        }
        return x.Where(y => x.Where(z => z == y).Count() != 1).ToList();
    }

    List<int> NotFirstNumber(List<Tuple<string, int>> hand)
    {
        var x = HandNumbers(hand);
        return x.Where(y => y != x[0]).ToList();
    }
    List<int> NotFirstNumberButWithListInt(List<int> hand)
    {
        var x = hand.Where(y => y != hand[0]).ToList();
        return x.Count() == 0 ? new List<int> { 0 } : x;
    }


    int CountNum(List<Tuple<string, int>> hand, int number)
    {
        return hand.Where(x => x.Item2 == number).Count();
    }

    List<int> NumIndexes(List<Tuple<string, int>> hand, int number)
    {
        List<int> x = new List<int>();
        for (int i = 0; i < hand.Count(); i++)
        {
            if (hand[i].Item2 == number) x.Add(HandCards.IndexOf(hand[i]));
        }
        return x;
    }

    bool Flush(List<Tuple<string, int>> hand)
    {
        for (int i = 0; i < hand.Count(); i++)
        {
            if (hand.Where(x => x.Item1 == hand[i].Item1 || x.Item1 == "Rainbow" || x.Item1 == "White" || (x.Item2 == 3 && ActiveModifiers[4])).Count() == 5)
                return true;
            if (ActiveModifiers[18] && hand.Where(x => x.Item1 == hand[i].Item1 || x.Item1 == "Rainbow" || x.Item1 == "White" || (x.Item2 == 3 && ActiveModifiers[4])).Count() == 4)
            {
                return true;
            }
        }
        
        return false;
    }
    new int[][] Straights =
    {
        new int[] {1,2,3,4,5},
        new int[] {6,2,3,4,5},
    };
    new int[][] FStraights =
    {
        new int[] {1,2,3,4},
        new int[] {2,3,4,5},
        new int[] {3,4,5,6 },
    };

    bool Straight(List<Tuple<string, int>> hand)
    {
        for (int l = 0; l < 2; l++)
            for (int i = 0; i < 5; i++) {
                if (hand.Where(x => x.Item2 == Straights[l][i]).Count() < 1)
                {
                    break;
                }
                if (i == 4)
                    return true;
            }
        for (int l = 0; l < 3; l++)
            for (int i = 0; i < 4; i++)
            {
                if (!ActiveModifiers[16]|| hand.Where(x => x.Item2 == FStraights[l][i]).Count() < 1)
                {
                    break;
                }
                if (i == 3)
                {
                    Debug.LogFormat("[Hanabi Poker #{0}]: You have been blessed by Dark Brown! This hand is a Straight.", _moduleId);
                    return true;
                }
            }
        return false;
    }
    bool FullHouse(List<Tuple<string, int>> hand)
    {
        return (CountNum(hand, hand[0].Item2) == 2 && CountNum(hand, NotFirstNumber(hand)[0]) == 3) || (CountNum(hand, hand[0].Item2) == 3 && CountNum(hand, NotFirstNumber(hand)[0]) == 2);
    }
    List<int> HandNumbers(List<Tuple<string, int>> hand)
    {
        return hand.Select(x => x.Item2).ToList();
    }

    string ActiveModifierNames()
    {
        string allmods = "";
        for (int i = 0; i < ActiveModifiers.Length; i++)
        {
            if (ActiveModifiers[i])
            {
                if (i < 5)
                    allmods += "\nAnti-" + SuitOrder[i];
                else
                    allmods += "\n" + SuitOrder[i];
            }
        }
        if (allmods == "")
        {
            return "Base Deck";
        }
        return allmods;
    }

    IEnumerator Clue(bool rank)
    {
        anyclue = false;
        yield return null;
        float t = 0;
        if (!animating)
            if (discards == 0 || (rank && HandCards.Where(x => !Colorless.Contains(x.Item1)).Count() == 0) || (!rank && HandCards.Where(x => !Numberless.Contains(x.Item1)).Count() == 0))
            {
                Audio.PlaySoundAtTransform("No", transform);
                float angle = Rnd.Range(-20f, 20f);
                CardCounter.text = "X";
                while (t < 1)
                {
                    yield return null;
                    t += Time.deltaTime * 3.5f;
                    CardCounter.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 100f), new Vector3(2f, 2f, 100f), t);
                    CardCounter.color = Color32.Lerp(new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 0), easeInSine(t));
                    CardCounter.transform.localEulerAngles = Vector3.Lerp(new Vector3(90f, 0f, 0f), new Vector3(90f, angle, 0f), t);
                }
            }
            else if ((rank && Cluing == 1) || (!rank && Cluing == 2))
            {
                Audio.PlaySoundAtTransform("SideConfirm", transform);
                animating = true;
                StartCoroutine(DiscardCards(!rank, ClueValue, false));
            }
            else if (rank)
            {
                Audio.PlaySoundAtTransform("SidePress", transform);
                if (HandCards.Where(x => EveryNumber.Contains(x.Item1)).Count() != 0)
                {
                    anyclue = true;
                }
                if (Cluing == 0)
                {
                    ClueValue = 0;
                    while (!anyclue && HandCards.Where(x => x.Item2 == ClueValue + 1 && !Numberless.Contains(x.Item1)).Count() == 0)
                    {
                        ClueValue++;
                        ClueValue %= ActiveModifiers[23] ? 6 : 5;
                    }

                }
                else
                {
                    do
                    {
                        ClueValue++;
                        ClueValue %= ActiveModifiers[23] ? 6 : 5;
                    }
                    while (!anyclue && HandCards.Where(x => x.Item2 == ClueValue + 1 && !Numberless.Contains(x.Item1)).Count() == 0);
                }
                animating = true;
                Cluing = 2;
                float angle = Rnd.Range(-20f, 20f);
                CardCounter.text = (ClueValue + 1).ToString();
                while (t < 1)
                {
                    yield return null;
                    t += Time.deltaTime * 3.5f;
                    CardCounter.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 100f), new Vector3(1f, 1f, 100f), t);
                    CardCounter.color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine(t));
                    CardCounter.transform.localEulerAngles = Vector3.Lerp(new Vector3(90f, 0f, 0f), new Vector3(90f, angle, 0f), t);
                }
                animating = false;
            }
            else
            {
                Audio.PlaySoundAtTransform("SidePress", transform);
                RealActiveSuit = ActiveSuit.Where(x => !Colorless.Contains(x) && !Multicolor.Contains(x)).ToList();
                RealActiveSuit = RealActiveSuit.OrderBy(x => SuitOrder.IndexOf(x)).ToList();
                //Debug.Log(RealActiveSuit.Join(" "));
                if (HandCards.Where(x => Multicolor.Contains(x.Item1)).Count() != 0)
                {
                    anyclue = true;
                }
                if (Cluing == 0)
                {
                    ClueValue = 0;
                    while (!anyclue && HandCards.Where(x => x.Item1 == RealActiveSuit[ClueValue]).Count() == 0)
                    {
                        ClueValue++;
                        ClueValue %= RealActiveSuit.Count();
                    }

                }
                else
                {
                    do
                    {
                        ClueValue++;
                        ClueValue %= RealActiveSuit.Count();
                    }
                    while (!anyclue && HandCards.Where(x => x.Item1 == RealActiveSuit[ClueValue]).Count() == 0);
                }
                animating = true;
                Cluing = 1;
                float angle = Rnd.Range(-20f, 20f);
                SuitClue.sprite = SuitImages[Array.IndexOf(SuitOrder, RealActiveSuit[ClueValue])];
                while (t < 1)
                {
                    yield return null;
                    t += Time.deltaTime * 3.5f;
                    SuitClue.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 100f), new Vector3(0.0025f, 0.0025f, 100f), t);
                    SuitClue.color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine(t));
                    SuitClue.transform.localEulerAngles = Vector3.Lerp(new Vector3(90f, 0f, 0f), new Vector3(90f, angle, 0f), t);
                }
                animating = false;
            }
    }

    IEnumerator DiscardCards(bool rank, int clueval, bool all)
    {
        discards--;
        float y = 0;
        CardCounter.text = discards.ToString();
        while (y < 1 && !all)
        {
            y += Time.deltaTime * 4f;
            CardCounter.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 100f), new Vector3(1f, 1f, 100f), y);
            CardCounter.color = Color32.Lerp(new Color32(255, 0, 0, 255), new Color32(255, 0, 0, 0), easeInSine(y));
            CardCounter.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            yield return null;

        }
        yield return new WaitForSeconds(.2f);
        CardCounter.color = new Color32(255, 255, 255, 0);
        Cluing = 0;
        yield return null;
        restart:
        int j = 0;
        foreach (var card in HandCards)
        {
            if (all || (rank && (card.Item2 == clueval + 1 || EveryNumber.Contains(card.Item1)) && !Numberless.Contains(card.Item1)) || (!rank && (card.Item1 == RealActiveSuit[clueval] || Multicolor.Contains(card.Item1))))
            {
                Audio.PlaySoundAtTransform("Succ", transform);
                float t = 0;
                while (t < 1)
                {
                    t += Time.deltaTime * 5f;
                    for (int i = 0; i < HandCards.Count(); i++)
                    {
                        if (i == j)
                        {
                            Cards[i].localPosition = Vector3.LerpUnclamped(HandPositions[HandCards.Count() - 1][i], NullHand, easeInSine(t));
                            Cards[i].localScale = Vector3.LerpUnclamped(One, Zero, easeInSine(t));
                        }
                        else if (i > j)
                        {
                            Cards[i].localPosition = Vector3.LerpUnclamped(HandPositions[HandCards.Count() - 1][i], HandPositions[HandCards.Count() - 2][i - 1], EaseOutBack(0, 1, t));
                        }
                        else
                        {
                            Cards[i].localPosition = Vector3.LerpUnclamped(HandPositions[HandCards.Count() - 1][i], HandPositions[HandCards.Count() - 2][i], EaseOutBack(0, 1, t));
                        }
                    }
                    yield return null;
                }
                HandCards.RemoveAt(j);
                for (int i = 0; i < 5; i++)
                {
                    if (HandCards.Count() > i)
                    {
                        Cards[i].localPosition = HandPositions[HandCards.Count() - 1][i];
                        Cards[i].localScale = One;
                        CardSuit(Cards[i]).sprite = SuitImages[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                        CardNumber(Cards[i]).sprite = NumberNumbers[HandCards[i].Item2 - 1];
                        CardNumber(Cards[i]).color = NumberColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                        CardRenderer(Cards[i]).color = SuitColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                    }
                    else
                    {
                        Cards[i].localPosition = new Vector3(0, 0, 0);
                        Cards[i].localScale = Zero;
                    }
                }
                goto restart;
            }
            j++;
        }
        yield return new WaitForSeconds(.25f);
        if (!all)
            StartCoroutine(DrawToFive());
    }

    void GenerateDeck()
    {
        if (ButtonAnims[2] != null)
            StopCoroutine(ButtonAnims[2]);
        ButtonAnims[2] = StartCoroutine(ButtonAnim(StartButton.transform));
        Audio.PlaySoundAtTransform("Start", transform);
        totalscore = 0;
        discards = 4;
        started = true;
        StartCoroutine(TurnOnBackground());
    }

    IEnumerator TurnOnBackground()
    {
        yield return null;
        speed = 3;
        float f = 5;
        while (f > 3)
        {
            TotalScore[0].color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine((5f - f) / 2f));
            TotalScore[1].color = Color32.Lerp(new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 0), easeInSine((5f - f) / 2f));
            HighScoreDisplay[0].color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine((5f - f) / 2f));
            HighScoreDisplay[1].color = Color32.Lerp(new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 0), easeInSine((5f - f) / 2f));
            BestHandDisplay[0].color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine((5f - f) / 2f));
            BestHandDisplay[1].color = Color32.Lerp(new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 0), easeInSine((5f - f) / 2f));
            yield return null;
            f -= Time.deltaTime * 2f;
            Background.material.color = Color32.Lerp(new Color32(15, 26, 19, 255), new Color32(92, 206, 132, 255), easeInSine((5f - f) / 2f));
            SetupScale.localScale = Vector3.Lerp(new Vector3(.1f, .1f, 1f), new Vector3(0f, 0f, 1f), easeInSine((5f - f) / 2f));
            if (ActiveModifiers[19])
            {
                Psychic[0].GetComponent<SpriteRenderer>().color = Color32.Lerp(new Color32(255,255,255,0), new Color32(255, 255, 255, 45), (5f - f) / 2f);
                Psychic[1].GetComponent<SpriteRenderer>().color = Color32.Lerp(new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 45), (5f - f) / 2f);
            }
        }

        TotalScore[0].color = new Color32(255, 255, 255, 0);
        TotalScore[1].color = new Color32(0, 0, 0, 0);
        TotalScore[0].text = "";
        TotalScore[1].text = "";
        speed = 3;
        yield return new WaitForSeconds(0.25f);
        SetupScale.position = new Vector3(1203f, 0f, 0f);
        if (ActiveModifiers.Where(x => x).Count() == 0)
        {
            Debug.LogFormat("[Hanabi Poker #{0}]: Shuffling a regular deck together for round {1}!", _moduleId, Round);
        }
        else if (ActiveModifiers.Where(x => x).Count() == 1)
        {
            Debug.LogFormat("[Hanabi Poker #{0}]: Shuffling a deck together with the modifier {1} for round {2}!", _moduleId, (Array.IndexOf(SuitOrder, ActiveSuit[0]) < 5 ? "Anti-" : "") + ActiveSuit[0], Round);
        }
        else
        {
            Debug.LogFormat("[Hanabi Poker #{0}]: Shuffling a deck together with the modifiers {1} and {2} for round {3}!", _moduleId, (Array.IndexOf(SuitOrder, ActiveSuit[0]) < 5 ? "Anti-" : "") + ActiveSuit[0], (Array.IndexOf(SuitOrder, ActiveSuit[1]) < 5 ? "Anti-" : "") + ActiveSuit[1], Round);
        }
        if (ActiveSuit.Contains("Riff-Raff"))
        {
            float t = 0;
            speed = 2;
            Audio.PlaySoundAtTransform("Outsiders-Test", transform);
            while (t < 1)
            {
                yield return null;
                t += Time.deltaTime;
                RiffRaffOverlay.color = Color.Lerp(new Color(1,1,1,0), new Color(1,1,1,.5f), t);
                Background.material.color = Color32.Lerp(new Color32(15, 26, 19, 255), new Color32(92, 206, 132, 255), easeInSine(1-t));
            }
            yield return new WaitForSeconds(1.5f);
            speed = 1;
            for (int i = 0; i < edging; i++)
            {
                ActiveSuit.Add(Modifiers[i + 2]);
                ActiveModifiers[SuitOrder.IndexOf(Modifiers[i + 2])] = true;
                Debug.LogFormat("[Hanabi Poker #{0}]: Riff-Raff adds {1} to the fray!", _moduleId, (Array.IndexOf(SuitOrder, Modifiers[i + 2]) < 5 ? "Anti-" : "") + Modifiers[i + 2]);
                float angle = Rnd.Range(-20f, 20f);
                SuitClue.sprite = SuitImages[Array.IndexOf(SuitOrder, Modifiers[i+2])];
                 t = 0;
                while (t < 1)
                {
                    yield return null;
                    t += Time.deltaTime * 2f;
                    SuitClue.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 100f), new Vector3(0.0025f, 0.0025f, 100f), t);
                    SuitClue.color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine(t));
                    SuitClue.transform.localEulerAngles = Vector3.Lerp(new Vector3(90f, 0f, 0f), new Vector3(90f, angle, 0f), t);
                }
                yield return new WaitForSeconds(.05f);
            }
             t = 1;
            while (t >0)
            {
                yield return null;
                t -= Time.deltaTime;
                Background.material.color = Color32.Lerp(new Color32(15, 26, 19, 255), new Color32(92, 206, 132, 255), easeInSine(1-t));
                RiffRaffOverlay.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, .5f), t);
            }
            speed = 3;
            RiffRaffOverlay.color = new Color(1, 1, 1, 00);
            ActiveSuit.Remove("Riff-Raff");
            yield return new WaitForSeconds(.5f);
        }
        for (int i = 0; i < 5; i++)
        {
            if (ActiveSuit.Contains(SuitOrder[i]))
                ActiveSuit.Remove(SuitOrder[i]);
            else ActiveSuit.Insert(0, SuitOrder[i]);
        }
        foreach (string suit in ActiveSuit)
        {
            if(suit == "Risky Dice")
            {
                for (int i = 0; i < 10; i++)
                    Deck.Add(new Tuple<string, int>(suit, 6));
            }
            else if (!Singlet.Contains(suit))
            {
                for (int i = 0; i < 3; i++)
                    Deck.Add(new Tuple<string, int>(suit, 1));
                for (int i = 0; i < 2; i++)
                {
                    Deck.Add(new Tuple<string, int>(suit, 2));
                    Deck.Add(new Tuple<string, int>(suit, 3));
                    Deck.Add(new Tuple<string, int>(suit, 4));
                }
                Deck.Add(new Tuple<string, int>(suit, 5));
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    Deck.Add(new Tuple<string, int>(suit, i + 1));
                }
            }
        }
        Deck = Deck.Shuffle();
        StartCoroutine(DrawToFive());
    }

    IEnumerator ResetMod()
    {
        Psychic[0].GetComponent<SpriteRenderer>().color = new Color32(255,255,255,0);
        Psychic[1].GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
        playedhands = new List<string>();
      edging = Rnd.Range(2, 5);
         Debug.LogFormat("[Hanabi Poker #{0}]: That marks the end of round {1}! Now, for round {2}, these modifiers are availiable:", _moduleId, Round, ++Round);
        string BHText = File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPHighScore.txt"));
        string HSText = File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPHighScore.txt"));
        int highscore = int.Parse(Regex.Match(HSText, @"\n([1234567890]+)").ToString());
        Debug.Log(HSText);
        Debug.Log(highscore);
        if (totalscore > highscore)
        {
            Audio.PlaySoundAtTransform("NewHighScore", transform);
            Debug.LogFormat("[Hanabi Poker #{0}]: That's a new high score! Nice one!", _moduleId, Round, ++Round);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "HPHighScore.txt"), "High Score\n" + totalscore + ActiveModifierNames());
            for (int i = 0; i < 2; i++)
            {
                HighScoreDisplay[i].text = "NEW HIGH SCORE!\n" + File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPHighScore.txt"));

            }
        }
        else
        {
            if (totalscore >= (ActiveModifiers[24] ? 225 : 150))
            {
                Audio.PlaySoundAtTransform("RoundFinishPass", transform);
            }
            else
            {
                Audio.PlaySoundAtTransform("RoundFinishfail", transform);
            }
            for (int i = 0; i < 2; i++)
            {
                HighScoreDisplay[i].text = File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPHighScore.txt"));
            }
        }
        if (newbest)
        {
            for (int i = 0; i < 2; i++)
            {
                BestHandDisplay[i].text = "NEW BEST HAND!\n" + File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPBestHand.txt"));

            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                BestHandDisplay[i].text = File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPBestHand.txt"));
            }
        }
        newbest = false;

        play = false;
        TotalScore[1].text = totalscore.ToString();
        TotalScore[0].text = totalscore.ToString();
        StartCoroutine(DiscardCards(false, 0, true));
        green[0] = false;
        green[1] = false;
        ModifierButtons[0].GetComponent<KMSelectable>().OnHighlightEnded();
        ModifierButtons[1].GetComponent<KMSelectable>().OnHighlightEnded();
        ActiveSuit = new List<string>();
        yield return new WaitForSeconds(.5f);
        speed = 5;
        float f = 3;
        Modifiers = Modifiers.Shuffle();
        ActiveModifiers = new bool[SuitOrder.Length];
        SetupScale.localPosition = Vector3.zero;
        int j = 0;
        foreach (var modbutton in ModifierButtons)
        {
            Debug.LogFormat("[Hanabi Poker #{0}]: {1} is availiable. [{2}]. If taken, {3}.", _moduleId, (Array.IndexOf(SuitOrder, Modifiers[j]) < 5 ? "Anti-" : "") + Modifiers[j], ModifierList[Modifiers[j]].Item1, ModifierList[Modifiers[j]].Item2);
            for (int i = 0; i < 2; i++)
            {

                ModName(modbutton)[i].text = ModifierList[Modifiers[j]].Item1;
                ModEffect(modbutton)[i].text = ModifierList[Modifiers[j]].Item2;
                ModColor(modbutton)[i].color = SelectButtonColors[Array.IndexOf(SuitOrder, Modifiers[j])];
                if (Modifiers[j] == "Riff-Raff")
                    ModColor(modbutton)[i].sprite = CardBacks[3];
                else
                    ModColor(modbutton)[i].sprite = CardBacks[2];
            }
            j++;
        }
        while (f < 5)
        {

            yield return null;
            f += Time.deltaTime * 1f;
            TotalScore[0].color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine((5f - f) / 2f));
            TotalScore[1].color = Color32.Lerp(new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 0), easeInSine((5f - f) / 2f));
            HighScoreDisplay[0].color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine((5f - f) / 2f));
            HighScoreDisplay[1].color = Color32.Lerp(new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 0), easeInSine((5f - f) / 2f));
            BestHandDisplay[0].color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine((5f - f) / 2f));
            BestHandDisplay[1].color = Color32.Lerp(new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 0), easeInSine((5f - f) / 2f));
            Background.material.color = Color32.Lerp(new Color32(15, 26, 19, 255), new Color32(92, 206, 132, 255), easeInSine((5f - f) / 2f));
            SetupScale.localScale = Vector3.Lerp(new Vector3(.1f, .1f, 1f), new Vector3(0f, 0f, 1f), easeInSine((5f - f) / 2f));
        }
        started = false;
        play = false;
        yield return null;
    }
    IEnumerator DrawToFive()
    {
        play = false;
        yield return null;
        if (Deck.Count() < 5 && HandCards.Count() == 0)
        {
            StartCoroutine(ResetMod());
        }
        else
            while (HandCards.Count < 5)
            {
                if (Deck.Count == 0)
                {
                    StartCoroutine(ResetMod());
                    break;
                }

                int j = 0;
                for (int i = 0; i < HandCards.Count; i++)
                {
                    if (Array.IndexOf(SuitOrder, HandCards[i].Item1) < Array.IndexOf(SuitOrder, Deck.First().Item1))
                    {
                        j++;
                    }
                    else if (Array.IndexOf(SuitOrder, HandCards[i].Item1) == Array.IndexOf(SuitOrder, Deck.First().Item1))
                    {
                        if (HandCards[i].Item2 < Deck.First().Item2)
                        {
                            j++;
                        }
                        else if (HandCards[i].Item2 == Deck.First().Item2)
                        {
                            j++;
                            break;
                        }
                    }
                }
                HandCards.Insert(j, Deck.First());
                Deck.Remove(Deck.First());
                if (Deck.Count() < 16)
                {
                    speed = 2;
                    if (Deck.Count() < 5)
                    {
                        speed = 1;
                    }
                }
                for (int i = 0; i < HandCards.Count; i++)
                {
                    CardSuit(Cards[i]).sprite = SuitImages[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                    CardNumber(Cards[i]).sprite = NumberNumbers[HandCards[i].Item2 - 1];
                    CardNumber(Cards[i]).color = NumberColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                    CardRenderer(Cards[i]).color = SuitColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                    if (i != j)
                    {
                        Cards[i].localPosition = HandPositions[HandCards.Count() - 1][i];
                        Cards[i].localScale = One;
                    }
                    else
                    {
                        CardRenderer(Cards[i]).sprite = CardBacks[1];
                        CardRenderer(Cards[i]).color = Color.white;
                        CardRenderer(Cards[i]).sortingOrder += 5;
                    }

                }
                CardCounter.text = Deck.Count().ToString();

                Audio.PlaySoundAtTransform("Deal", transform);
                float angle = Rnd.Range(-20f, 20f);
                float Timer = 0;
                bool donethateverytime = false;
                while (Timer < 1)
                {
                    Timer += Time.deltaTime * 4f;
                    CardCounter.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 100f), new Vector3(1f, 1f, 100f), Timer);
                    CardCounter.color = Color32.Lerp(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 0), easeInSine(Timer));
                    CardCounter.transform.localEulerAngles = Vector3.Lerp(new Vector3(90f, 0f, 0f), new Vector3(90f, angle, 0f), Timer);
                    for (int i = 0; i < HandCards.Count; i++)
                    {
                        if (i == j)
                        {
                            if (!donethateverytime && Timer > .5f)
                            {
                                donethateverytime = true;
                                CardRenderer(Cards[i]).sprite = CardBacks[0];
                                CardRenderer(Cards[i]).color = SuitColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                                CardRenderer(Cards[i]).sortingOrder -= 5;
                            }
                            Cards[i].localPosition = Vector3.LerpUnclamped(NullHand, HandPositions[HandCards.Count() - 1][i], EaseOutBack(0, 1, Timer));
                            Cards[i].localScale = Vector3.LerpUnclamped(Zero, One, EaseOutBack(0, 1, Timer));
                            Cards[i].localEulerAngles = Vector3.LerpUnclamped(new Vector3(-90, 90, 90), new Vector3(90, 90, 90), Timer);
                        }
                        else if (i > j)
                        {
                            Cards[i].localPosition = Vector3.LerpUnclamped(HandPositions[HandCards.Count() - 2][i - 1], HandPositions[HandCards.Count() - 1][i], EaseOutBack(0, 1, Timer));
                        }
                        else
                        {
                            Cards[i].localPosition = Vector3.LerpUnclamped(HandPositions[HandCards.Count() - 2][i], HandPositions[HandCards.Count() - 1][i], EaseOutBack(0, 1, Timer));
                        }
                    }
                    yield return null;
                }
                for (int i = 0; i < 5; i++)
                {
                    Cards[i].localEulerAngles = new Vector3(90, 90, 90);
                }
                for (int i = 0; i < HandCards.Count; i++)
                {
                    Cards[i].localPosition = HandPositions[HandCards.Count() - 1][i];
                    Cards[i].localScale = One;
                    CardSuit(Cards[i]).sprite = SuitImages[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                    CardNumber(Cards[i]).sprite = NumberNumbers[HandCards[i].Item2 - 1];
                    CardNumber(Cards[i]).color = NumberColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                    CardRenderer(Cards[i]).color = SuitColors[Array.IndexOf(SuitOrder, HandCards[i].Item1)];
                }
                yield return new WaitForSeconds(.0001f);
            }
        play = true;

        animating = false;

        Audio.PlaySoundAtTransform("Ready", transform);
    }
    void HL(bool b, int i)
    {
        if (b)
            foreach (var highlight in ModHL(ModifierButtons[i]))
            {
                highlight.enabled = true;
                if (!over[i])
                    highlight.color = new Color32(255, 100, 00, 255);
            }
        else
        {
            over[i] = false;
            foreach (var highlight in ModHL(ModifierButtons[i]))
            {
                if (!green[i])
                    highlight.enabled = false;
                if (green[i])
                    highlight.color = new Color32(53, 210, 93, 255);
            }
        }
    }

    void ModifierSelect(int i)
    {
        if (ButtonAnims[i] != null)
            StopCoroutine(ButtonAnims[i]);
        ButtonAnims[i] = StartCoroutine(ButtonAnim(ModifierButtons[i].transform));
        green[i] = !green[i];
        if (green[i])
        {
            Audio.PlaySoundAtTransform("UIPress", transform);
            over[i] = true;
            foreach (var highlight in ModHL(ModifierButtons[i]))
            {
                if (green[i])
                    highlight.color = new Color32(53, 210, 93, 255);
            }
            if (Modifiers[i] == "Riff-Raff" && Rnd.Range(0, 10) == 0)
            {
                Audio.PlaySoundAtTransform("RiffRaffBuzz", transform);
                riffrafftimer = 5;
                ledge = Rnd.Range(2, edging + 1);
            }
            ActiveSuit.Add(Modifiers[i]);
            ActiveModifiers[Array.IndexOf(SuitOrder, Modifiers[i])] = true;
        }
        else
        {
            Audio.PlaySoundAtTransform("UIUnpress", transform);
            foreach (var highlight in ModHL(ModifierButtons[i]))
            {
                highlight.color = new Color32(255, 100, 00, 255);
            }
            ActiveModifiers[Array.IndexOf(SuitOrder, Modifiers[i])] = false;
            ActiveSuit.Remove(Modifiers[i]);
        }
    }

    // Use this for initialization
    void Start()
    {
        edging = Rnd.Range(2, 5);
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "HPHighScore.txt")))
        {
            Debug.LogFormat("No saved high score!");
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "HPHighScore.txt"), "High Score\n0");
        }
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "HPBestHand.txt")))
        {
            Debug.LogFormat("No saved best hand!");
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "HPBestHand.txt"), "Best Hand\n0");
        }
        for (int i = 0; i < 2; i++)
        {
            HighScoreDisplay[i].text = File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPHighScore.txt"));
            BestHandDisplay[i].text = File.ReadAllText(Path.Combine(Application.persistentDataPath, "HPBestHand.txt"));
        }
        Round++;
        Debug.LogFormat("[Hanabi Poker #{0}]: Welcome to Hanabi Poker!", _moduleId);
        Background.material.color = new Color32(15, 26, 19, 255);

        Modifiers = Modifiers.Shuffle();
        ActiveModifiers = new bool[SuitOrder.Length];
        wiggleup = new bool[WiggleTransforms.Count()];
        wigglenums = new float[WiggleTransforms.Count()];
        for (int i = 0; i < WiggleTransforms.Count(); i++)
        {
            wigglenums[i] = Rnd.Range(-2000, 2000) / 1000f;
            wiggleup[i] = Rnd.Range(0, 2) == 0;
        }
        int j = 0;
        Debug.LogFormat("[Hanabi Poker #{0}]: For round {1}, the following modifications are available:", _moduleId, Round);
        foreach (var modbutton in ModifierButtons)
        {
            Debug.LogFormat("[Hanabi Poker #{0}]: {1} is availiable. [{2}]. If taken, {3}.", _moduleId, (Array.IndexOf(SuitOrder, Modifiers[j]) < 5 ? "Anti-" : "") + Modifiers[j], ModifierList[Modifiers[j]].Item1, ModifierList[Modifiers[j]].Item2);
            for (int i = 0; i < 2; i++)
            {

                ModName(modbutton)[i].text = ModifierList[Modifiers[j]].Item1;
                ModEffect(modbutton)[i].text = ModifierList[Modifiers[j]].Item2;
                ModColor(modbutton)[i].color = SelectButtonColors[Array.IndexOf(SuitOrder, Modifiers[j])];
                if (Modifiers[j] == "Riff-Raff")
                    ModColor(modbutton)[i].sprite = CardBacks[3];
                else
                    ModColor(modbutton)[i].sprite = CardBacks[2];
            }
            j++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int j = 0; j < HandCards.Count; j++)
        {
            if (HandCards[j].Item1 == "Rainbow")
            {
                CardRenderer(Cards[j]).sprite = CardBacks[12];
            }
            else if (HandCards[j].Item1 == "Muddy Rainbow")
            {
                CardRenderer(Cards[j]).sprite = CardBacks[5];

            }
            else if (HandCards[j].Item1 == "Cocoa Rainbow")
            {
                CardRenderer(Cards[j]).sprite = CardBacks[7];

            }
            else if (HandCards[j].Item1 == "Dark Rainbow")
            {
                CardRenderer(Cards[j]).sprite = CardBacks[6];
            }
            else
            {
                CardRenderer(Cards[j]).sprite = CardBacks[0];
            }

        }
    }

    void FixedUpdate()
    {
        if (ActiveModifiers[19] && !Deck.IsNullOrEmpty())
        {
            Psychic[0].GetComponent<SpriteRenderer>().sprite = SuitImages[SuitOrder.IndexOf(Deck.First().Item1)];
            Psychic[1].GetComponent<SpriteRenderer>().sprite = NumberNumbers[Deck.First().Item2 - 1];
        }
        if (ActiveModifiers[19] && Deck.Count() == 0 && speed == 1)
        {
            Psychic[0].GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);
            Psychic[1].GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        }
        if (!play)
        {
            PlayScale.position = new Vector3(123132f, 1231f, 1231243f);
        }
        else
            PlayScale.localPosition = new Vector3(0, 0, 0);
        for (int j = 0; j < 4; j++)
        {
            ModColor(ModifierButtons[j / 2])[j % 2].color = SelectButtonColors[Array.IndexOf(SuitOrder, Modifiers[j / 2])];
        }
        if (riffrafftimer >= 0)
            riffrafftimer--;
        if (riffrafftimer > 0)
            for (int p = 0; p < 4; p++)
            {
                if (Modifiers[p % 2] == "Riff-Raff")
                {
                    ModName(ModifierButtons[p % 2])[p / 2].text = ModifierList[Modifiers[ledge]].Item1;
                    ModEffect(ModifierButtons[p % 2])[p / 2].text = ModifierList[Modifiers[ledge]].Item2;
                    ModColor(ModifierButtons[p % 2])[p / 2].color = SelectButtonColors[Array.IndexOf(SuitOrder, Modifiers[ledge])];
                    ModColor(ModifierButtons[p % 2])[p / 2].sprite = CardBacks[2];
                }

            }
        else
        {
            for (int p = 0; p < 4; p++)
            {
                if (Modifiers[p % 2] == "Riff-Raff")
                {
                    ModName(ModifierButtons[p % 2])[p / 2].text = ModifierList["Riff-Raff"].Item1;
                    ModEffect(ModifierButtons[p % 2])[p / 2].text = ModifierList["Riff-Raff"].Item2;
                    ModColor(ModifierButtons[p % 2])[p / 2].color = SelectButtonColors[Array.IndexOf(SuitOrder, "Riff-Raff")];
                    ModColor(ModifierButtons[p % 2])[p / 2].sprite = CardBacks[3];
                }
                if (Modifiers[p % 2] == "Rainbow")
                {
                    ModColor(ModifierButtons[p % 2])[p / 2].sprite = CardBacks[8];
                }
                if (Modifiers[p % 2] == "Muddy Rainbow")
                {
                    ModColor(ModifierButtons[p % 2])[p / 2].sprite = CardBacks[9];
                }
                if (Modifiers[p % 2] == "Cocoa Rainbow")
                {
                    ModColor(ModifierButtons[p % 2])[p / 2].sprite = CardBacks[11];
                }
                if (Modifiers[p % 2] == "Dark Rainbow")
                {
                    ModColor(ModifierButtons[p % 2])[p / 2].sprite = CardBacks[10];
                }
            }
        }
        
        bgoffset++;
        Background.material.mainTextureOffset = new Vector2(.25f * (bgoffset / (int)speed % 4), .5f * (bgoffset / (int)speed / 4 % 2));
        if (bgoffset % 50 == 0)
        {
            Color32 omniman = new Color32((byte)Rnd.Range(125, 256), (byte)Rnd.Range(125, 256), (byte)Rnd.Range(125, 256), 255);
            Color32 omniman2 = new Color32((byte)Rnd.Range(0, 125), (byte)Rnd.Range(0, 125), (byte)Rnd.Range(0, 125), 255);
            SuitColors[11] = omniman;
            NumberColors[11] = omniman;
            SelectButtonColors[11] = omniman;
            SuitColors[22] = omniman2;
            NumberColors[22] = omniman2;
            SelectButtonColors[22] = omniman2;
            for (int j = 0; j < 4; j++)
            {
                ModColor(ModifierButtons[j / 2])[j % 2].color = SelectButtonColors[Array.IndexOf(SuitOrder, Modifiers[j / 2])];
            }
            for (int j = 0; j < HandCards.Count; j++)
            {
                if (HandCards[j].Item1 == "Omni")
                {
                    CardRenderer(Cards[j]).color = SuitColors[11];
                    CardNumber(Cards[j]).color = NumberColors[11];
                    CardSuit(Cards[j]).sprite = SuitImages[11];
                    if (Rnd.Range(0, 8196) == 0)
                    {
                        Audio.PlaySoundAtTransform("bwomp", transform);
                        CardSuit(Cards[j]).sprite = CardBacks[4];
                    }
                }
                if (HandCards[j].Item1 == "Dark Omni")
                {
                    CardRenderer(Cards[j]).color = SuitColors[22];
                    CardNumber(Cards[j]).color = NumberColors[22];
                    CardSuit(Cards[j]).sprite = SuitImages[22];
                    if (Rnd.Range(0, 8196) == 0)
                    {
                        Audio.PlaySoundAtTransform("bwomp", transform);
                        CardSuit(Cards[j]).sprite = CardBacks[4];
                    }
                }
            }
        }
        int i = 0;
        if (wigglenums != null)
            foreach (var wiggle in WiggleTransforms)
            {
                if (i < 3)
                    wigglenums[i] += Time.fixedDeltaTime / (Mathf.Pow(wigglenums[i], 2) + 1f + Rnd.Range(0f, 1f)) * (wiggleup[i] ? -1 : 1);
                else
                    wigglenums[i] += Time.fixedDeltaTime / (Mathf.Sqrt(Mathf.Abs(wigglenums[i])) + .0001f + Rnd.Range(0f, 1f)) * (wiggleup[i] ? -3 : 3);
                if (Mathf.Abs(wigglenums[i]) > (i > 2 ? 10f : 2f)) wiggleup[i] = !wiggleup[i];
                wiggle.localEulerAngles = new Vector3(wiggle.localEulerAngles.x, (i > 2 ? 90 : wiggle.localEulerAngles.y), wigglenums[i] + (i > 2 ? 90 : 0));
                i++;
            }
    }

    TextMesh[] ModName(Transform button)
    {
        return new TextMesh[] { button.Find("ModText").GetComponent<TextMesh>(), button.Find("ModTextDark").GetComponent<TextMesh>() };
    }
    TextMesh[] ModEffect(Transform button)
    {
        return new TextMesh[] { button.Find("ModInfo").GetComponent<TextMesh>(), button.Find("ModInfoDark").GetComponent<TextMesh>() };
    }
    SpriteRenderer[] ModColor(Transform button)
    {
        return new SpriteRenderer[] { button.GetComponent<SpriteRenderer>(), button.Find("Info").GetComponent<SpriteRenderer>() };
    }
    SpriteRenderer[] ModHL(Transform button)
    {
        return new SpriteRenderer[] { button.Find("Highlight").GetComponent<SpriteRenderer>(), button.Find("HighlightSmall").GetComponent<SpriteRenderer>() };
    }
   

    SpriteRenderer CardNumber(Transform card)
    {
        return card.Find("Count").GetComponent<SpriteRenderer>();
    }
    SpriteRenderer CardSuit(Transform card)
    {
        return card.Find("Suit").GetComponent<SpriteRenderer>();
    }
    SpriteRenderer CardRenderer(Transform card)
    {
        return card.GetComponent<SpriteRenderer>();
    }
    public static float EaseOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }
    float easeInSine(float x)
    {
        return 1 - Mathf.Cos((x * Mathf.PI) / 2);
    }

}
static class DictionaryTupleDuckTyping
{
    public static void Add<TKey, TValue1, TValue2>(
        this Dictionary<TKey, Tuple<TValue1, TValue2>> dictionary,
        TKey key,
        TValue1 value1,
        TValue2 value2
    )
    {
        dictionary.Add(key, new Tuple<TValue1, TValue2>(value1, value2));
    }
}
