using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class hatguessing : MonoBehaviour
{

    static private int _moduleIdCounter = 1;
    private int _moduleId;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Bomb;

    public Sprite[] Numbers;
    public Sprite[] Suits;
    public Sprite[] Symbols;
    public Sprite[] FuseSprites;
    public Transform[] Cards;
    public Transform[] PlayPiles;
    public TextMesh[] NameText;
    private string[] DeckOfCards =
    {
        "1R","1R","1R","2R","2R","3R","3R","4R","4R","5R",
        "1Y","1Y","1Y","2Y","2Y","3Y","3Y","4Y","4Y","5Y",
        "1G","1G","1G","2G","2G","3G","3G","4G","4G","5G",
        "1B","1B","1B","2B","2B","3B","3B","4B","4B","5B",
        "1P","1P","1P","2P","2P","3P","3P","4P","4P","5P",
    };
    private string[] Names = { "Alice", "Bob", "Cathy", "Donald", "Emily", "Frank", "Grace", "Hnter", "J.C. Indo.", "Jackson", "Klyzx", "Logan", "Maddy", "Niel", "Oscar", "Phillip", "Quinn", "Rand", "Shareoff", "Tiger", "U.F.R.T.F.", "Vince", "Will", "Xavier", "You", "Zoe", "Mine" };
    static string[] LogText = { "First", "Then", "Next", "Finally" };
    private static Dictionary<string, string> NameExtensions = new Dictionary<string, string>
    {
        {"J.C. Indo.", "Jarlos Carlos Indonesia"},
        {"U.F.R.T.F.", "Unnecessary Forced Reverse Trash Finesse"},
        {"Alice", "Alice"},
        {"Bob", "Bob, The Bomber of Yellow 4s"},
        {"Cathy", "Cathy"},
        {"Donald", "Donald"},
        {"Emily", "Emily"},
        {"Frank", "Frank"},
        {"Hnter", "Hnter"},
        {"Jackson", "SpikedJackson"},
        {"Klyzx", "Klyzx"},
        {"Maddy", "MaddyMoos (That's Me!)"},
        {"Rand", "Rand P"},
        {"Shareoff", "Shareoff"},
        {"Vince", "Vincent"},
        {"Will", "Willflame"},
        {"You", "you"},
        {"Mine", "M1n3c4rt"},
        {"Tiger", "Tiger"},
        {"Grace","Grace" },
        {"Logan","Logan Stone" },
        {"Niel","Niel" },
        {"Oscar","Oscar" },
        {"Phillip","Phillip" },
        {"Quinn","Quinn" },
        {"Xavier","Xavier" },
        {"Zoe","Zoe, Bob's Replacement" },
    };
    List<Tuple<int, string>> Discarded = new List<Tuple<int, string>>();
    List<Tuple<int, string>> HandCards = new List<Tuple<int, string>>();
    List<Tuple<int, string>> MarkedForPlay = new List<Tuple<int, string>>();
    List<Tuple<int, int, string>> Fireworks = new List<Tuple<int, int, string>>();
    private int DeckPosition = 0;
    private static string Colorder = "RYGBP";
    private static string[] ColorNames = { "red", "yellow", "green", "blue", "purple" };
    private static int[] counts = { 3, 2, 2, 2, 1 };
    private static string[] order = { "1", "23", "123", "2345", "12345" };
    private int[] discards = { 0, 0, 0, 0, 0 };
    private bool[] trash = new bool[16];
    private bool[] crit = new bool[16];
    private bool[] clue = new bool[16];
    private bool[] play = new bool[16];
    private int Total = 0;
    private bool fuse = false, anim = false;
    private bool suit = false;
    private int position = 0;
    private int Fusetaps;
    private float timer;

    private bool testma = false;
    private bool strike = false;

    public Transform Fuse;
    public Transform FuseFlame;
    public Transform FuseFlame2;
    public AudioSource[] FuseSounds;

    public KMSelectable[] Buttons;

    public Color[] CardBackColors;
    public Color[] NumberColors;
    public Color[] TouchColors;
    public Color[] FuseColors;

    private int[] Pride2 =
    {0,1,2,3,1,2,3,4,2,3,4,5,3,4,5,6
    };
    private int[] Pride =
    {0,0,0,0,1,1,1,1,2,2,2,2,3,3,3,3
    };
    private int[] PrideOrder =
    {
        0,9,1,2,3,4,5,6,7,8
    };
    private int[] PrideBack =
    {
        0,14,1,2,3,4,10,11,12,13
    };

    //TP variables to save the solution. If you want to use these for the solution check, be careful because there are a few shortcuts for the value=12 case.
    private int playerSolution;
    private bool mustTouchCardOne;
    private bool isSolutionNumber;

    void Awake()
    {

        //do you have QKRisi's? if not, ya ur outdated
        _moduleId = _moduleIdCounter++;
        for (int j = 0; j < 16; j++)
        {
            int i = j;
            Buttons[i].OnInteract += delegate
            {
                StartCoroutine(HandlePress(i));
                return false;
            };
            Buttons[i].OnHighlight += delegate
            {
                HL(true, i);
            };
            Buttons[i].OnHighlightEnded += delegate
            {
                HL(false, i);
            };
        }
    }

    IEnumerator HandlePress(int button)
    {
        yield return null;
        if (fuse || anim)
        {
            position = button;
            suit = !suit;
            timer += .2f;
            if (timer > 3f)
            {
                timer = 3f;
            }
            Fusetaps++;
        }
        else
        {
            position = button;
            Fusetaps = 0;
            float metatimer = 1f;
            float callouttimer = 0;
            fuse = true;
            timer = 3f;
            CardRenderer(FuseFlame).enabled = true;
            FuseSounds[0].Play();
            while (timer > 0)
            {

                Fuse.localPosition = Vector3.Lerp(new Vector3(0.0828f, 0.0137f, -0.0049f), new Vector3(0.0828f, 0.0137f, -0.0049f + .075f), Mathf.Pow(1 - (timer / 3f), 2));
                Fuse.localScale = Vector3.Lerp(new Vector3(0.006f, .075f, .005f), new Vector3(.006f, 0, .005f), Mathf.Pow(1 - (timer / 3f), 2));
                FuseFlame.localPosition = Vector3.Lerp(new Vector3(.0828f, .0197f, -.07997f), new Vector3(.0828f, .0197f, -.07997f + (.075f * 2f)), Mathf.Pow(1 - (timer / 3f), 2));
                CardRenderer(FuseFlame).sprite = FuseSprites[(int)(timer * 15f) % 3];
                CardRenderer(FuseFlame2).sprite = FuseSprites[(int)(timer * 15f) % 3];
                CardRenderer(FuseFlame).color = FuseColors[(int)(timer * 12f) % 4 + (suit ? 8 : 0)];
                Fuse.GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", Vector2.Lerp(new Vector2(1, -15), new Vector2(1, 0), Mathf.Pow(1 - (timer / 3f), 2)));
                Fuse.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", Vector2.Lerp(new Vector2(0, 0), new Vector2(0, -15), Mathf.Pow(1 - (timer / 3f), 2)));
                yield return null;
                timer -= Time.deltaTime * metatimer;
                //if (timer < 0) timer = 3f;
                callouttimer += Time.deltaTime;
                metatimer += Time.deltaTime / 10f;
            }
            FuseSounds[0].Stop();
            if (Fusetaps > 10)
            {
                Debug.LogFormat("[Hat Guessing #{0}]: Having fun? You fuse-tapped {1} times, surviving for {2} seconds.", _moduleId, Fusetaps, (int)callouttimer);
            }
            CardRenderer(FuseFlame).enabled = false;
            bool correct = false;
            Debug.Log(suit + " " + position + " " + ((int)(position / 4) + 1 == (int)Math.Ceiling(((float)(Total + 1)) / 4f)) + " " + Total);
            if (Total == 12)
            {
                if (position / 4 == 3)
                {
                    correct = true;
                }
            }
            else
                switch (Total % 4)
                {
                    case 0:
                        Debug.Log("You must give a number clue that touches slot one to player " + Math.Ceiling(((float)(Total + 1)) / 4f));
                        if (!suit && (int)(position / 4) + 1 == (int)Math.Ceiling(((float)(Total + 1)) / 4f) && HandCards[(int)((Math.Ceiling(((float)(Total + 1)) / 4f) - 1) * 4) + 3].Item1 == HandCards[position].Item1)
                        {
                            correct = true; break;
                        }
                        correct = false;
                        break;
                    case 1:
                        Debug.Log("You must give a color clue that touches slot one to player " + Math.Ceiling(((float)(Total + 1)) / 4f));
                        if (suit && (int)(position / 4) + 1 == (int)Math.Ceiling(((float)(Total + 1)) / 4f) && HandCards[(int)((Math.Ceiling(((float)(Total + 1)) / 4f) - 1) * 4) + 3].Item2 == HandCards[position].Item2)
                        {
                            correct = true; break;
                        }
                        correct = false;
                        break;
                    case 2:
                        Debug.Log("You must give a number clue that does not touch slot one to player " + Math.Ceiling(((float)(Total + 1)) / 4f));
                        if (!suit && (int)(position / 4) + 1 == (int)Math.Ceiling(((float)(Total + 1)) / 4f) && HandCards[(int)((Math.Ceiling(((float)(Total + 1)) / 4f) - 1) * 4) + 3].Item1 != HandCards[position].Item1)
                        {
                            correct = true; break;
                        }
                        correct = false; break;
                    case 3:
                        Debug.Log("You must give a color clue that does not touch slot one to player " + Math.Ceiling(((float)(Total + 1)) / 4f));
                        if (suit && (int)(position / 4) + 1 == (int)Math.Ceiling(((float)(Total + 1)) / 4f) && HandCards[(int)((Math.Ceiling(((float)(Total + 1)) / 4f) - 1) * 4) + 3].Item2 != HandCards[position].Item2)
                        {
                            correct = true; break;
                        }
                        correct = false;
                        break;
                }
            bool suitstorage = suit;
            suit = false;
            anim = true; fuse = false;
            if (correct)
            {
                FuseSounds[1].Play();
                yield return new WaitForSeconds(2f);
                if (!strike)
                {
                    Audio.PlaySoundAtTransform("Perfect", Module.GetComponent<Transform>());

                }
                else
                    Audio.PlaySoundAtTransform("Solve", Module.GetComponent<Transform>());
                yield return new WaitForSeconds(1f);

                Module.HandlePass();
                if ((Rnd.Range(0, 500) == 0 || Application.isEditor) && !strike)
                {
                    for (int i = 0; i < 50; i++)
                        Fireworks.Add(new Tuple<int, int, string>(Rnd.Range(0, 4), Rnd.Range(0, 5), ""));
                    StartCoroutine(Transgender());
                }
                foreach (var firework in Fireworks)
                {
                    switch (firework.Item1)
                    {
                        case 0:
                            for (int i = 0; i < firework.Item2 + 1; i++)
                            {
                                Audio.PlaySoundAtTransform("Play", Module.GetComponent<Transform>());
                                yield return new WaitForSeconds(.7f / (firework.Item2 + 1 + Rnd.value));
                            }
                            yield return new WaitForSeconds(.3f);

                            break;
                        case 2:
                            for (int i = 0; i < firework.Item2 + 1; i++)
                            {
                                Audio.PlaySoundAtTransform("Discard", Module.GetComponent<Transform>());
                                yield return new WaitForSeconds(1f / (firework.Item2 + 1 + Rnd.value));
                            }
                            yield return new WaitForSeconds(.3f);

                            break;
                        case 1:
                            for (int i = 0; i < firework.Item2 + 1; i++)
                            {
                                Audio.PlaySoundAtTransform("Save", Module.GetComponent<Transform>());
                                yield return new WaitForSeconds(.5f / (firework.Item2 + 1 + Rnd.value));
                            }
                            yield return new WaitForSeconds(.2f);

                            break;
                        case 3:
                            yield return new WaitForSeconds(.25f);
                            break;
                    }
                }

            }
            else
            {
                Debug.LogFormat("[Hat Guessing #{0}]: Erm... No. Not quite. You clued {1} for {2}'s {3} {4}. From the top!",
                    _moduleId,
                    suitstorage ? "color" : "number",
                    NameExtensions[Names[position / 4]],
                    ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[position].Item2))],
                    HandCards[position].Item1 + 1
                    );
                strike = true;
                Audio.PlaySoundAtTransform("Strike", Module.GetComponent<Transform>());
                anim = false;
                //im lazy
                Fuse.localPosition = Vector3.Lerp(new Vector3(0.0828f, 0.0137f, -0.0049f), new Vector3(0.0828f, 0.0137f, -0.0049f + .075f), 0);
                Fuse.localScale = new Vector3(0.006f, .075f, .005f);
                FuseFlame.localPosition = Vector3.Lerp(new Vector3(.0828f, .01675f, -.07997f), new Vector3(.0828f, .01675f, -.07997f + (.075f * 2f)), 0);
                Fuse.GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", Vector2.Lerp(new Vector2(1, -15), new Vector2(1, 0), 0));
                Fuse.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", Vector2.Lerp(new Vector2(0, 0), new Vector2(0, -15), 0));
                Module.HandleStrike();
                GenerateModule();
            }
        }
    }
    /*
    IEnumerator CometLerp(int comet, string color)
    {
        float time = 0f;
        Comets[comet].GetComponent<Transform>().localPosition =
                new Vector3(
                    Comets[comet].GetComponent<Transform>().localPosition.x,
                    Comets[comet].GetComponent<Transform>().localPosition.y,
                    -.08f);
        Comets[comet].colorOverLifetime.color = 
            
            
            
            Gradients[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(color))];
        Comets[comet].Play();
        while (time < 1f)
        {
            Comets[comet].GetComponent<Transform>().localPosition = Vector3.Lerp(
                new Vector3(
                    Comets[comet].GetComponent<Transform>().localPosition.x,
                    Comets[comet].GetComponent<Transform>().localPosition.y,
                    -.08f),
                new Vector3(
                    Comets[comet].GetComponent<Transform>().localPosition.x,
                    Comets[comet].GetComponent<Transform>().localPosition.y,
                    .08f),
                easeOutExpo(time)
                    ) ;
            time += Time.deltaTime * 2;
            yield return null;
        }
        Comets[comet].Pause();
        yield return null;
    }
    float easeOutExpo(float x) 
    {
        return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
    }*/


    void HL(bool SelectOn, int Position)
    {
        if (SelectOn)
        {
            CardTouched(Cards[Position]).enabled = true;
            CardTouched(Cards[Position]).color = TouchColors[3];
        }
        else
        {
            if (clue[Position])
            {
                if (play[Position])
                {
                    CardTouched(Cards[Position]).color = TouchColors[0];
                }
                else if (trash[Position])
                {
                    CardTouched(Cards[Position]).color = TouchColors[1];
                }
                else if (crit[Position])
                {
                    CardTouched(Cards[Position]).color = TouchColors[2];
                }
            }
            else
                CardTouched(Cards[Position]).enabled = false;
        }
    }

    // Use this for initialization
    void Start()
    {
        GenerateModule();
    }

    void GenerateModule()
    {
        testma = false;
        Total = 0;
        DeckPosition = 0;
        play = new bool[16];
        trash = new bool[16];
        clue = new bool[16];
        crit = new bool[16];
        discards = new int[] { 0, 0, 0, 0, 0 };
        Discarded = new List<Tuple<int, string>>();
        HandCards = new List<Tuple<int, string>>();
        MarkedForPlay = new List<Tuple<int, string>>();
        DeckOfCards.Shuffle();
        int SuitStorage = 0;
        int RankStorage = 0;
        string suitother;
        Transform CurrentCard;
        Names = Names.Shuffle();
        for (int i = 0; i < 8; i++)
        {
            NameText[i].text = Names[i % 4];
        }
        for (DeckPosition = 0; DeckPosition < 16; DeckPosition++)
        {
            suitother = DeckOfCards[DeckPosition][1].ToString();
            CurrentCard = Cards[DeckPosition];
            SuitStorage = Array.IndexOf(Colorder.ToCharArray(), DeckOfCards[DeckPosition][1]);
            RankStorage = int.Parse(DeckOfCards[DeckPosition][0].ToString()) - 1;
            HandCards.Add(new Tuple<int, string>(RankStorage, suitother));
            CardNumber(CurrentCard).sprite = Numbers[RankStorage];
            CardNumber(CurrentCard).color = NumberColors[SuitStorage];
            CardSuit(CurrentCard).sprite = Suits[SuitStorage];
            CardRenderer(CurrentCard).color = CardBackColors[SuitStorage];
            CardSpecial(CurrentCard).enabled = false;
            CardTouched(CurrentCard).enabled = false;
        }
        int DiscardCount = Rnd.Range(10, 20);
        for (DeckPosition = 16; DeckPosition < DiscardCount + 16; DeckPosition++)
        {
            suitother = DeckOfCards[DeckPosition][1].ToString();
            RankStorage = int.Parse(DeckOfCards[DeckPosition][0].ToString()) - 1;
            Discarded.Add(new Tuple<int, string>(RankStorage, suitother));
        }
        List<Tuple<int, string>> temptuple = new List<Tuple<int, string>>();
        for (int i = 0; i < 6; i++)
        {
            temptuple = Discarded.Where(x => x.Item1 == i + 1).ToList();
            foreach (var card in temptuple)
            {
                SuitStorage = Array.IndexOf(Colorder.ToArray(), Convert.ToChar(card.Item2));
                if (discards[SuitStorage] == i)
                {
                    discards[SuitStorage] = i + 1;
                }
            }
        }
        for (int i = 0; i < 5; i++)
        {
            CardNumber(PlayPiles[i]).enabled = discards[i] != 0;
            if (CardNumber(PlayPiles[i]).enabled)
            {
                CardNumber(PlayPiles[i]).sprite = Numbers[discards[i] - 1];
            }
            CardRenderer(PlayPiles[i]).color = CardBackColors[discards[i] != 0 ? i : i + 5];
        }
        int k = -1;
        foreach (var card in HandCards)
        {
            k++;
            if (discards[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(card.Item2))] > card.Item1)
            {
                trash[k] = true;
                CardSpecial(Cards[k]).sprite = Symbols[1];
                CardSpecial(Cards[k]).enabled = true;
            }
            for (int i = 0; i < card.Item1; i++)
            {
                if (Discarded.Where(x => x.Item1 == i && x.Item2 == card.Item2).Count() == counts[i] && i > discards[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(card.Item2))])
                {
                    trash[k] = true;
                    CardSpecial(Cards[k]).sprite = Symbols[1];
                    CardSpecial(Cards[k]).enabled = true;
                }
            }
            if (Discarded.Where(x => x.Item1 == card.Item1 && x.Item2 == card.Item2).Count() == counts[card.Item1] - 1 && !trash[k])
            {
                crit[k] = true;
                CardSpecial(Cards[k]).enabled = true;
                CardSpecial(Cards[k]).sprite = Symbols[0];
            }
        }
        //bug.Log(HandCards.Join(" , "));
        //ebug.Log(Discarded.Join(" , "));

        k = -1;
        int count = 0;
        temptuple.Clear();
        foreach (var card in HandCards)
        {
            k++;
            if (k % 4 == 0) count = 0;
            if (Rnd.Range(0, 100) <= 35 && count < 2)
            {
                if (discards[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(card.Item2))] == card.Item1 && !temptuple.Contains(card))
                {
                    //Debug.Log("Cluing Card " + k);
                    temptuple.Add(card);
                    CardTouched(Cards[k]).enabled = true;
                    CardTouched(Cards[k]).color = TouchColors[0];
                    clue[k] = true;
                    play[k] = true;
                    count++;
                }
                else
                if (trash[k] || temptuple.Contains(card))
                {
                    trash[k] = true;
                    //Debug.Log("Cluing Card " + k);
                    CardTouched(Cards[k]).enabled = true;
                    CardTouched(Cards[k]).color = TouchColors[1];
                    clue[k] = true;
                    count++;
                }
                else
                if (crit[k])
                {
                    //Debug.Log("Cluing Card " + k);
                    CardTouched(Cards[k]).enabled = true;
                    CardTouched(Cards[k]).color = TouchColors[2];
                    clue[k] = true;
                    count++;
                }
            }

        }
        k = -1;
        foreach (var card in HandCards)
        {
            k++;
            if (temptuple.Contains(card) && !clue[k])
            {
                trash[k] = true;
            }
        }
        MarkedForPlay = temptuple;

        bool fort = false;
        if ((!discards.Contains(3) && !discards.Contains(4) && !discards.Contains(5)))
        {
            GenerateModule();
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                if (HandCards.Skip(i * 4).Take(4).Where(x => x.Item1 == HandCards[i * 4].Item1).Count() == 4 || HandCards.Skip(i * 4).Take(4).Where(x => x.Item2 == HandCards[i * 4].Item2).Count() == 4)
                {
                    GenerateModule();
                    break;
                }
                if (i == 3)
                {
                    Debug.LogFormat("[Hat Guessing #{0}]: Welcome{5} to Hat Guessing. Today, our players are {1}, {2}, {3}, and {4}.", _moduleId, NameExtensions[Names[0]], NameExtensions[Names[1]], NameExtensions[Names[2]], NameExtensions[Names[3]], strike ? " back" : "");
                    Debug.LogFormat("[Hat Guessing #{0}]: The play piles have {1}, {2}, {3}, {4}, and {5}.",
                        _moduleId,
                        "Red on " + discards[0],
                        "Yellow on " + discards[1],
                        "Green on " + discards[2],
                        "Blue on " + discards[3],
                        "Purple on " + discards[4]


                        );
                    for (int j = 0; j < 4; j++)
                        Debug.LogFormat("[Hat Guessing #{0}]: {23}, {1} {22} {2} {18}{3}{4} {5}, {6} {19}{7}{8} {9}, {10} {20}{11}{12} {13}, and {14} {21}{15}{16} {17}.",
                            _moduleId,
                            NameExtensions[Names[j]],
                            (HandCards[j * 4 + 3].Item1 == 4 ? "the" : "a"),
                            (crit[j * 4 + 3] ? "critical " : trash[j * 4 + 3] ? "trash " : ""),
                            ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[j * 4 + 3].Item2))],
                            HandCards[j * 4 + 3].Item1 + 1,
                            (HandCards[j * 4 + 2].Item1 == 4 ? "the" : "a"),
                            (crit[j * 4 + 2] ? "critical " : trash[j * 4 + 2] ? "trash " : ""),
                            ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[j * 4 + 2].Item2))],
                            HandCards[j * 4 + 2].Item1 + 1,
                            (HandCards[j * 4 + 1].Item1 == 4 ? "the" : "a"),
                            (crit[j * 4 + 1] ? "critical " : trash[j * 4 + 1] ? "trash " : ""),
                            ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[j * 4 + 1].Item2))],
                            HandCards[j * 4 + 1].Item1 + 1,
                            (HandCards[j * 4].Item1 == 4 ? "the" : "a"),
                            (crit[j * 4] ? "critical " : trash[j * 4] ? "trash " : ""),
                            ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[j * 4].Item2))],
                            HandCards[j * 4].Item1 + 1,
                            (clue[j * 4 + 3] ? "clued " : ""),
                            (clue[j * 4 + 2] ? "clued " : ""),
                            (clue[j * 4 + 1] ? "clued " : ""),
                            (clue[j * 4] ? "clued " : ""),
                            Names[j] == "You" ? "hold" : "holds",
                            LogText[j]
                            );
                    Debug.LogFormat("[Hat Guessing #{0}]: As for the clues you need to give;", _moduleId);
                    SolveCards();
                }
            }

        }
    }
    void SolveCards()
    {
        Total = 0;
        bool reset = false;
        string responseorder = "";
        int[] playcounts = new int[4];
        for (int j = 0; j < 4; j++)
        {
            playcounts[j] = play.Skip(j * 4).Take(4).Where(x => x == true).Count();
        }
        for (int j = 0; j < 4; j++)
        {
            for (int k = 0; k < 4; k++)
            {
                if (playcounts[k] == j)
                    responseorder += k;
            }
        }
        responseorder = responseorder.Reverse();
        int i = 0;
        for (int k = 0; k < 4; k++)
        {
            i = int.Parse(responseorder[k].ToString());

            reset = false;
            for (int l = 3; l >= 0; l--)
            {
                if (clue[i * 4 + l] || MarkedForPlay.Contains(HandCards[i * 4 + l])) continue;
                if (crit[i * 4 + l] && discards[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))] == HandCards[i * 4 + l].Item1)
                {
                    if (Names[i] != "You")
                        Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [Critical Playable] on their {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);
                    else
                        Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [Critical Playable] on your {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);

                    Fireworks.Add(new Tuple<int, int, string>(0, HandCards[i * 4 + l].Item1, HandCards[i * 4 + l].Item2));
                    Total += 4 - l;
                    reset = true;
                    break;
                }
            }
            if (reset) continue;
            for (int l = 3; l >= 0; l--)
            {
                if (clue[i * 4 + l] || MarkedForPlay.Contains(HandCards[i * 4 + l])) continue;
                if (HandCards.Skip(i * 4).Take(4).Where(x => HandCards[i * 4 + l] == x).Count() == 2 && HandCards[i * 4 + l].Item1 != 0 && discards[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))] == HandCards[i * 4 + l].Item1)
                {
                    if (Names[i] != "You")
                        Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [In-Hand Duplicate Playable] on their {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);
                    else
                        Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [In-Hand Duplicate Playable] on your {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);


                    Fireworks.Add(new Tuple<int, int, string>(0, HandCards[i * 4 + l].Item1, HandCards[i * 4 + l].Item2));
                    Total += 4 - l;
                    reset = true;
                    break;
                }
            }
            if (reset) continue;
            for (int l = 3; l >= 0; l--)
            {
                if (clue[i * 4 + l] || MarkedForPlay.Contains(HandCards[i * 4 + l])) continue;
                if (HandCards.Skip(i * 4).Take(4).Where(x => HandCards[i * 4 + l] == x).Count() >= 2 && HandCards[i * 4 + l].Item1 == 0 && discards[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))] == HandCards[i * 4 + l].Item1)
                {
                    if (Names[i] != "You")
                        Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [Duplicate 1 Playable] on their {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);
                    else
                        Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [Duplicate 1 Playable] on your {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);
                    Fireworks.Add(new Tuple<int, int, string>(0, HandCards[i * 4 + l].Item1, HandCards[i * 4 + l].Item2));
                    Total += 4 - l;
                    reset = true;
                    break;
                }
            }
            if (reset) continue;
            var temp = new Tuple<int, string>(6, "J");
            int temp2 = 0;
            for (int l = 3; l >= 0; l--)
            {
                if (clue[i * 4 + l] || MarkedForPlay.Contains(HandCards[i * 4 + l])) continue;
                if (discards[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))] == HandCards[i * 4 + l].Item1 && temp.Item1 > HandCards[i * 4 + l].Item1)
                {
                    temp = HandCards[i * 4 + l];
                    temp2 = l;
                    reset = true;
                }
            }
            if (reset)
            {
                MarkedForPlay.Add(temp);
                if (Names[i] != "You")

                    Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [Lowest Playable] on their {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(temp.Item2))], temp.Item1 + 1);
                else Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [Lowest Playable] on your {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(temp.Item2))], temp.Item1 + 1);
                Fireworks.Add(new Tuple<int, int, string>(0, HandCards[i * 4 + temp2].Item1, HandCards[i * 4 + temp2].Item2));
                Total += 4 - temp2;
                continue;
            }
            var temp3 = new List<Tuple<int, string>>();
            for (int l = 3; l >= 0; l--)
            {
                if (clue[i * 4 + l]) continue;
                if (HandCards.Skip(i * 4).Take(4).Where(x => HandCards[i * 4 + l] == x && !trash[i * 4 + l]).Count() >= 2 && !temp3.Contains(HandCards[i * 4 + l]))
                {
                    temp = HandCards[i * 4 + l];
                    temp3.Add(temp);
                    temp2 = l;
                    reset = true;
                    testma = true;
                }
            }
            if (reset)
            {
                if (Names[i] != "You")
                    Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [Rightmost-Leftmost Duplicate Discard] on their {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(temp.Item2))], temp.Item1 + 1);
                else Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [Rightmost-Leftmost Duplicate Discard] on your {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(temp.Item2))], temp.Item1 + 1);
                Fireworks.Add(new Tuple<int, int, string>(1, HandCards[i * 4 + temp2].Item1, HandCards[i * 4 + temp2].Item2));
                Total += 8 - temp2;
                continue;
            }
            for (int l = 0; l < 4; l++)
            {
                if (clue[i * 4 + l] && trash[i * 4 + l])
                {
                    reset = false;
                    break;
                }
                if (trash[i * 4 + l] && !reset)
                {
                    temp = HandCards[i * 4 + l];
                    temp2 = l;
                    reset = true;
                }
            }
            if (reset)
            {
                if (Names[i] != "You")
                    Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [Single Trash Discard] on their {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(temp.Item2))], temp.Item1 + 1);
                else Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [Single Trash Discard] on your {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(temp.Item2))], temp.Item1 + 1);
                Fireworks.Add(new Tuple<int, int, string>(1, HandCards[i * 4 + temp2].Item1, HandCards[i * 4 + temp2].Item2));
                Total += 8 - temp2;
                continue;
            }
            for (int l = 0; l < 4; l++)
            {
                if (clue[i * 4 + l])
                    continue;
                if (crit[i * 4 + l])
                {
                    if (Names[i] != "You")
                        Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [Critical Save] on their {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);
                    else Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [Critical Save] on your {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);
                    Fireworks.Add(new Tuple<int, int, string>(2, HandCards[i * 4 + l].Item1, HandCards[i * 4 + l].Item2));
                    Total += 12 - l;
                    reset = true;
                    break;
                }
            }
            if (reset) continue;
            for (int l = 0; l < 4; l++)
            {
                if (clue[i * 4 + l])
                    continue;
                if (trash[i * 4 + l])
                {
                    if (Names[i] != "You")
                        Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [Trash Discard] on their {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);
                    else Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [Trash Discard] on your {3} {4}.", _moduleId, LogText[k], NameExtensions[Names[i]], ColorNames[Array.IndexOf(Colorder.ToArray(), Convert.ToChar(HandCards[i * 4 + l].Item2))], HandCards[i * 4 + l].Item1 + 1);
                    Fireworks.Add(new Tuple<int, int, string>(1, HandCards[i * 4 + l].Item1, HandCards[i * 4 + l].Item2));
                    Total += 8 - l;
                    reset = true;
                    break;
                }
            }
            if (reset) continue;
            if (Names[i] != "You")
                Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}'s clue is [Clue Clue].", _moduleId, LogText[k], NameExtensions[Names[i]]);
            else Debug.LogFormat("[Hat Guessing #{0}]: {1}, {2}r clue is [Clue Clue].", _moduleId, LogText[k], NameExtensions[Names[i]]);
            Fireworks.Add(new Tuple<int, int, string>(3, 0, ""));
        }
        Debug.LogFormat("[Hat Guessing #{0}]: The total of these clues before modulo is {1}, making the Clue Value {2}.", _moduleId, Total, Total % 13);
        Total %= 13;
        if (Total == 12)
        {
            Debug.LogFormat("[Hat Guessing #{0}]: Since the Clue Value is 12, any clue to {1} is valid.", _moduleId, NameExtensions[Names[3]]);
            playerSolution = 3;
            mustTouchCardOne = true;
            isSolutionNumber = true;
        }
        else
        {
            if (Names[i] != "You")
                Debug.LogFormat("[Hat Guessing #{0}]: Since the Clue Value is {1}, you must give a {2} clue that {3} slot one to {4}.", _moduleId, Total, Total % 2 == 1 ? "color" : "number", Total % 4 > 1 ? "does not touch" : "touches", NameExtensions[Names[(int)(Math.Ceiling(((float)(Total + 1)) / 4f) - 1)]]);
            else Debug.LogFormat("[Hat Guessing #{0}]: Since the Clue Value is {1}, you must give a {2} clue that {3} slot one to yourself.", _moduleId, Total, Total % 2 == 1 ? "color" : "number", Total % 4 > 1 ? "does not touch" : "touches", NameExtensions[Names[(int)(Math.Ceiling(((float)(Total + 1)) / 4f) - 1)]]);
            playerSolution = (int)(Math.Ceiling(((float)(Total + 1)) / 4f) - 1);
            isSolutionNumber = Total % 2 == 0;
            mustTouchCardOne = Total % 4 <= 1;
        }
        //if (!testma) GenerateModule();
        //testma balls complete 
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var sound in FuseSounds)
        {
            sound.volume = ((float)Wawa.DDL.Preferences.Sound / Wawa.DDL.Preferences.MaxVolume);
        }
    }

    IEnumerator Transgender()
    {
        yield return null;
        NameText[2].text = "TRANS";
        NameText[3].text = "RIGHTS";
        NameText[0].text = "HAVE";
        NameText[1].text = "PRIDE";
        NameText[2 + 4].text = "TRANS";
        NameText[3 + 4].text = "RIGHTS";
        NameText[0 + 4].text = "HAVE";
        NameText[1 + 4].text = "PRIDE";
        for (int i = 0; i < 16; i++)
        {
            CardSpecial(Cards[i]).enabled = false;
            clue[i] = false;
            CardTouched(Cards[i]).enabled = false;
        }
        int gay = 0;
        while (true)
        {
            gay++;
            for (int i = 0; i < 16; i++)
            {

                CardNumber(Cards[i]).color = NumberColors[PrideOrder[(Pride[i] + gay) % 10]];
                if (PrideOrder[(Pride[i] + gay) % 10] == 8)
                {
                    CardNumber(Cards[i]).enabled = false;
                }
                else
                    CardNumber(Cards[i]).enabled = true;
                CardSuit(Cards[i]).sprite = Suits[PrideOrder[(Pride[i] + gay) % 10]];
                if (PrideOrder[(Pride[i] + gay) % 10] == 7)
                {
                    CardSuit(Cards[i]).enabled = false;
                }
                else
                    CardSuit(Cards[i]).enabled = true;
                CardRenderer(Cards[i]).color = CardBackColors[PrideBack[(Pride[i] + gay) % 10]];
            }
            yield return new WaitForSeconds(.15f);
        }
    }

    SpriteRenderer CardNumber(Transform card)
    {
        return card.Find("Count").GetComponent<SpriteRenderer>();
    }
    SpriteRenderer CardSuit(Transform card)
    {
        return card.Find("Suit").GetComponent<SpriteRenderer>();
    }
    SpriteRenderer CardSpecial(Transform card)
    {
        return card.Find("Symbol").GetComponent<SpriteRenderer>();
    }
    SpriteRenderer CardTouched(Transform card)
    {
        return card.Find("Touch").GetComponent<SpriteRenderer>();
    }
    SpriteRenderer CardRenderer(Transform card)
    {
        return card.GetComponent<SpriteRenderer>();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <player name> <card slot number> <number/color> [Give a <suit/color> to <player name> for their card on slot <card slot number>]";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpper();
        string[] upperNames = Names.Take(4).Select(n => n.ToUpper()).ToArray();
        string regexNames = string.Join("|", upperNames.Select(n => n.Replace(".", "\\.")).ToArray());
        string regex = $@"^({regexNames})\s+(1|2|3|4)\s+(NUMBER|COLOU?R)$";
        Debug.Log(regex);
        var res = Regex.Match(command, regex);
        if (res.Success)
        {
            yield return null;
            string[] commands = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int playerIndex = Array.IndexOf(upperNames, res.Groups[1].Value);
            int cardIndex = int.Parse(res.Groups[2].Value) - 1;
            int howManyTaps = res.Groups[3].Value.Equals("NUMBER") ? 1 : 2;

            for (int i = 0; i < howManyTaps; i++)
                //3- because the rightmost cards are first on the list
                Buttons[playerIndex * 4 + (3 - cardIndex)].OnInteract();
        }
    }
    private IEnumerator TwitchHandleForcedSolve()
    {
        if (mustTouchCardOne)
        {
            for (int i = 0; i < (isSolutionNumber ? 1 : 2); i++)
            {
                Buttons[playerSolution * 4 + 3].OnInteract();
            }
            yield break;
        }
        else
        {
            var playerHandWithoutFirstCard = HandCards.Skip(playerSolution * 4).Take(3).ToArray();
            var playerFirstCard = HandCards[playerSolution * 4 + 3];
            for (int i = 0; i < 3; i++)
            {
                if (isSolutionNumber)
                {
                    if (playerFirstCard.Item1 != playerHandWithoutFirstCard[i].Item1)
                    {
                        Buttons[playerSolution * 4 + i].OnInteract();
                        yield break;
                    }

                }
                else
                {
                    if (playerFirstCard.Item2 != playerHandWithoutFirstCard[i].Item2)
                    {
                        Buttons[playerSolution * 4 + i].OnInteract();
                        Buttons[playerSolution * 4 + i].OnInteract();
                        yield break;
                    }
                }
            }
        }
    }
}
