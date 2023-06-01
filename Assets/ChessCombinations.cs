using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ChessCombinations : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable ColorBucket;
    public MeshRenderer KingTexture;
    public MeshRenderer QueenTexture;
    public MeshRenderer RookTexture;
    public MeshRenderer BishopTexture;
    public MeshRenderer KnightTexture;
    public List<MeshRenderer> ButtonRenderers;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    int currentColorIndex = 7;
    string randomPosition = "";
    List<string> randomPositions = new List<string> { };
    List<string> randomColors = new List<string> { };
    List<string> randomPieces = new List<string> { };
    int setRow;
    int setColumn;

    string pieces = "KQRBN";
    List<string> pieceNames = new List<string> { "King", "Queen", "Rook", "Bishop", "Knight" };
    List<Color> colors = new List<Color> {Color.black, Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow, Color.white};
    List<string> binaryColors = new List<string> { "000", "100", "010", "001", "011", "101", "110", "111" };
    List<string> colorNames = new List<string> {"Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White"};
    List<string> RedValues = new List<string>
    {
        "000000",
        "000000",
        "000000",
        "000000",
        "000000",
        "000000"
    };
    List<string> GreenValues = new List<string>
    {
        "000000",
        "000000",
        "000000",
        "000000",
        "000000",
        "000000"
    };
    List<string> BlueValues = new List<string>
    {
        "000000",
        "000000",
        "000000",
        "000000",
        "000000",
        "000000"
    };
    List<string> Pieces = new List<string>
    {
        "      ",
        "      ",
        "      ",
        "      ",
        "      ",
        "      "
    };

    void Awake () {
        ModuleId = ModuleIdCounter++;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */
        //button.OnInteract += delegate () { buttonPress(); return false; };
        ColorBucket.OnInteract += delegate () { ColorSwitch(); return false; };
    }

   void ColorSwitch()
   {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ColorBucket.transform);

        currentColorIndex = (currentColorIndex + 1) % 8;

        KingTexture.material.color = colors[currentColorIndex];
        QueenTexture.material.color = colors[currentColorIndex];
        RookTexture.material.color = colors[currentColorIndex];
        BishopTexture.material.color = colors[currentColorIndex];
        KnightTexture.material.color = colors[currentColorIndex];
    }

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        for (int i = 0; i < 6; i++)
        {
            randomPosition = Rnd.Range(0, 6).ToString() + Rnd.Range(0, 6).ToString();
            while (randomPositions.Contains(randomPosition))
            {
                randomPosition = Rnd.Range(0, 6).ToString() + Rnd.Range(0, 6).ToString();
            }
            randomPositions.Add(randomPosition);
        }

        for (int i = 0; i < 6; i++)
        {
            randomColors.Add(Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString());
        }

        for (int i = 0; i < 6; i++)
        {
            randomPieces.Add(pieces[Rnd.Range(0, 5)].ToString());
        }
        Debug.LogFormat("[RGB Chess #{0}] The generated pieces are - {1} {2} at {3}, {4} {5} at {6}, {7} {8} at {9}, {10} {11} at {12}, {13} {14} at {15} and {16} {17} at {18}", ModuleId,
            LogGenColors(0), LogGenPieces(0), LogGenCoordinates(0),
            LogGenColors(1), LogGenPieces(1), LogGenCoordinates(1),
            LogGenColors(2), LogGenPieces(2), LogGenCoordinates(2),
            LogGenColors(3), LogGenPieces(3), LogGenCoordinates(3),
            LogGenColors(4), LogGenPieces(4), LogGenCoordinates(4),
            LogGenColors(5), LogGenPieces(5), LogGenCoordinates(5));

        calculateBoardColors();
        setBoardColors();
    }

    string LogGenCoordinates(int index)
    {
        return "ABCDEF"[Int32.Parse(randomPositions[index][1].ToString())].ToString() + (Int32.Parse(randomPositions[index][0].ToString()) + 1).ToString();
    }

    string LogGenColors(int index)
    {
        return colorNames[binaryColors.IndexOf(randomColors[index])];
    }

    string LogGenPieces(int index)
    {
        return pieceNames[pieces.IndexOf(randomPieces[index])];
    }

    void calculateBoardColors()
    {
        for (int i = 0; i < 6; i++)
        {
            int row = Int32.Parse(randomPositions[i][0].ToString());
            int column = Int32.Parse(randomPositions[i][1].ToString());
            addColorToCell(row, column, randomColors[i]);
            switch (randomPieces[i])
            {
                case "K":
                    if (row + 1 < 6)
                    {
                        addColorToCell(row + 1, column, randomColors[i]);
                        if (column + 1 < 6)
                        {
                            addColorToCell(row + 1, column + 1, randomColors[i]);
                        }
                        if (column - 1 >= 0)
                        {
                            addColorToCell(row + 1, column - 1, randomColors[i]);
                        }
                    }
                    if (row - 1 >= 0)
                    {
                        addColorToCell(row - 1, column, randomColors[i]);
                        if (column + 1 < 6)
                        {
                            addColorToCell(row - 1, column + 1, randomColors[i]);
                        }
                        if (column - 1 >= 0)
                        {
                            addColorToCell(row - 1, column - 1, randomColors[i]);
                        }
                    }
                    if (column + 1 < 6)
                    {
                        addColorToCell(row, column + 1, randomColors[i]);
                    }
                    if (column - 1 >= 0)
                    {
                        addColorToCell(row, column - 1, randomColors[i]);
                    }
                    break;
                case "R":
                    for (int r = 0; r < 6; r++)
                    {
                        addColorToCell(row, r, randomColors[i]);
                        addColorToCell(r, column, randomColors[i]);
                    }
                    break;
                case "B":
                    for (int b = 0; b < 6; b++)
                    {
                        if (row + b < 6)
                        {
                            if (column + b < 6)
                            {
                                addColorToCell(row + b, column + b, randomColors[i]);
                            }
                            if (column - b >= 0)
                            {
                                addColorToCell(row + b, column - b, randomColors[i]);
                            }
                        }
                        if (row - b >= 0)
                        {
                            if (column + b < 6)
                            {
                                addColorToCell(row - b, column + b, randomColors[i]);
                            }
                            if (column - b >= 0)
                            {
                                addColorToCell(row - b, column - b, randomColors[i]);
                            }
                        }
                    }
                    break;
                case "Q":
                    for (int q = 0; q < 6; q++)
                    {
                        addColorToCell(row, q, randomColors[i]);
                        addColorToCell(q, column, randomColors[i]);
                        if (row + q < 6)
                        {
                            if (column + q < 6)
                            {
                                addColorToCell(row + q, column + q, randomColors[i]);
                            }
                            if (column - q >= 0)
                            {
                                addColorToCell(row + q, column - q, randomColors[i]);
                            }
                        }
                        if (row - q >= 0)
                        {
                            if (column + q < 6)
                            {
                                addColorToCell(row - q, column + q, randomColors[i]);
                            }
                            if (column - q >= 0)
                            {
                                addColorToCell(row - q, column - q, randomColors[i]);
                            }
                        }
                    }
                    break;
                case "N":
                    if (row - 2 >= 0)
                    {
                        if (column - 1 >= 0)
                        {
                            addColorToCell(row - 2, column - 1, randomColors[i]);
                        }
                        if (column + 1 < 6)
                        {
                            addColorToCell(row - 2, column + 1, randomColors[i]);
                        }
                    }
                    if (row + 2 < 6)
                    {
                        if (column - 1 >= 0)
                        {
                            addColorToCell(row + 2, column - 1, randomColors[i]);
                        }
                        if (column + 1 < 6)
                        {
                            addColorToCell(row + 2, column + 1, randomColors[i]);
                        }
                    }
                    if (column - 2 >= 0)
                    {
                        if (row - 1 >= 0)
                        {
                            addColorToCell(row - 1, column - 2, randomColors[i]);
                        }
                        if (row + 1 < 6)
                        {
                            addColorToCell(row + 1, column - 2, randomColors[i]);
                        }
                    }
                    if (column + 2 >= 0)
                    {
                        if (row - 1 >= 0)
                        {
                            addColorToCell(row - 1, column + 2, randomColors[i]);
                        }
                        if (row + 1 < 6)
                        {
                            addColorToCell(row + 1, column + 2, randomColors[i]);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void addColorToCell(int row, int column, string color)
    {
        RedValues[row] = RedValues[row].Substring(0, column) + ((RedValues[row][column] + color[0]) % 2).ToString() + RedValues[row].Substring(column + 1);
        GreenValues[row] = GreenValues[row].Substring(0, column) + ((GreenValues[row][column] + color[1]) % 2).ToString() + GreenValues[row].Substring(column + 1);
        BlueValues[row] = BlueValues[row].Substring(0, column) + ((BlueValues[row][column] + color[2]) % 2).ToString() + BlueValues[row].Substring(column + 1);
    }

    void setBoardColors()
    {
        for (int i = 0; i < 36; i++)
        {
            setRow = (int)(i / 6);
            setColumn = i % 6;
            ButtonRenderers[i].material.color = colors[binaryColors.IndexOf(RedValues[setRow][setColumn].ToString() + GreenValues[setRow][setColumn].ToString() + BlueValues[setRow][setColumn].ToString())];
        }
    }

    void Update()
    {

    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string Command) {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve () {
        yield return null;
    }
}
