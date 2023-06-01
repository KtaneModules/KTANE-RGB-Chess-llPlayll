using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class RGBChess : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMColorblindMode Colorblind;

    public KMSelectable ColorBucket;
    public List<KMSelectable> PieceButtons;
    public List<MeshRenderer> PieceTextures;
    public List<Material> PieceMaterials;
    public List<MeshRenderer> GridButtonRenderers;
    public List<KMSelectable> GridButtons;
    public List<TextMesh> ColorblindTexts;
    public List<GameObject> GridPieces;
    public List<MeshRenderer> GridPieceRenderers;
    public List<TextMesh> GridPieceColorblindTexts;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    string randomPosition = "";
    string randomColor = "";
    string selectedPiece = "";
    List<string> randomPositions = new List<string> { };
    List<string> randomColors = new List<string> { };
    List<string> randomPieces = new List<string> { };
    int currentColorIndex = 7;
    int setColorIndex;
    int setRow;
    int setColumn;
    int placedPieces;

    string pieces = "KQRBN";
    List<string> pieceNames = new List<string> { "King", "Queen", "Rook", "Bishop", "Knight" };
    List<Color> colors = new List<Color> { Color.black, Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow, Color.white };
    List<string> binaryColors = new List<string> { "000", "100", "010", "001", "011", "101", "110", "111" };
    List<string> shortColorNames = new List<string> { "K", "R", "G", "B", "C", "M", "Y", "W" };
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

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        ColorBucket.OnInteract += delegate () { ColorSwitch(); return false; };
        foreach (KMSelectable piece in PieceButtons)
        {
            piece.OnInteract += delegate () { PiecePressed(piece); return false; };
        }
        foreach (KMSelectable cell in GridButtons)
        {
            cell.OnInteract += delegate () { GridButtonPressed(cell); return false; };
        }
    }

   void ColorSwitch()
   {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ColorBucket.transform);

        currentColorIndex = (currentColorIndex + 1) % 8;

        for (int i = 0; i < 5; i++)
        {
            PieceTextures[i].material.color = colors[currentColorIndex];
        }
        selectedPiece = shortColorNames[currentColorIndex];
    }
    
    void PiecePressed(KMSelectable piece)
    {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, piece.transform);
        piece.AddInteractionPunch();
        for (int i = 0; i < PieceButtons.Count; i++)
        {
            if (piece == PieceButtons[i])
            {
                selectedPiece = shortColorNames[currentColorIndex] + pieces[i];
                Debug.LogFormat("[RGB Chess #{0}] The {1} piece was pressed, selecting the {1}.", ModuleId, pieceNames[i]);
                Debug.LogFormat("[RGB Chess #{0}] Currently selected piece is {1} {2}.", ModuleId, colorNames[currentColorIndex], pieceNames[i]);
            }
        }
    }

    void GridButtonPressed(KMSelectable cell)
    {
        if (ModuleSolved)
        {
            return;
        }
        for (int i = 0; i < GridButtons.Count; i++)
        {
            if (cell == GridButtons[i])
            {
                if (!GridPieces[i].activeSelf)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, GridPieces[i].transform);
                    GridPieces[i].SetActive(true);
                    GridPieceRenderers[i].material = PieceMaterials[pieces.IndexOf(selectedPiece[1].ToString())];
                    GridPieceRenderers[i].material.color = colors[shortColorNames.IndexOf(selectedPiece[0].ToString())];
                    if (Colorblind.ColorblindModeActive)
                    {
                        GridPieceColorblindTexts[i].text = selectedPiece[0].ToString();
                    }
                    else
                    {
                        GridPieceColorblindTexts[i].text = "";
                    }
                    placedPieces++;
                }
                else
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, GridPieces[i].transform);
                    GridPieces[i].SetActive(false);
                    placedPieces--;
                }
                if (placedPieces == 6)
                {
                    Debug.Log("Checking submission");
                }
            }
        }
    }

    void Start()
    {
        GenerateBoard();
        for (int c = 0; c < 36; c++)
        {
            ColorblindTexts[c].gameObject.SetActive(Colorblind.ColorblindModeActive);
        }
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
            randomColor = Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString();
            while (randomColor == "000")
            {
                randomColor = Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString();
            }
            randomColors.Add(randomColor);

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

        CalculateBoardColors();
        SetBoardColors();
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

    void CalculateBoardColors()
    {
        for (int i = 0; i < 6; i++)
        {
            int row = Int32.Parse(randomPositions[i][0].ToString());
            int column = Int32.Parse(randomPositions[i][1].ToString());
            AddColorToCell(row, column, randomColors[i]);
            switch (randomPieces[i])
            {
                case "K":
                    if (row + 1 < 6)
                    {
                        AddColorToCell(row + 1, column, randomColors[i]);
                        if (column + 1 < 6)
                        {
                            AddColorToCell(row + 1, column + 1, randomColors[i]);
                        }
                        if (column - 1 >= 0)
                        {
                            AddColorToCell(row + 1, column - 1, randomColors[i]);
                        }
                    }
                    if (row - 1 >= 0)
                    {
                        AddColorToCell(row - 1, column, randomColors[i]);
                        if (column + 1 < 6)
                        {
                            AddColorToCell(row - 1, column + 1, randomColors[i]);
                        }
                        if (column - 1 >= 0)
                        {
                            AddColorToCell(row - 1, column - 1, randomColors[i]);
                        }
                    }
                    if (column + 1 < 6)
                    {
                        AddColorToCell(row, column + 1, randomColors[i]);
                    }
                    if (column - 1 >= 0)
                    {
                        AddColorToCell(row, column - 1, randomColors[i]);
                    }
                    break;
                case "R":
                    for (int r = 0; r < 6; r++)
                    {
                        AddColorToCell(row, r, randomColors[i]);
                        AddColorToCell(r, column, randomColors[i]);
                    }
                    break;
                case "B":
                    for (int b = 0; b < 6; b++)
                    {
                        if (row + b < 6)
                        {
                            if (column + b < 6)
                            {
                                AddColorToCell(row + b, column + b, randomColors[i]);
                            }
                            if (column - b >= 0)
                            {
                                AddColorToCell(row + b, column - b, randomColors[i]);
                            }
                        }
                        if (row - b >= 0)
                        {
                            if (column + b < 6)
                            {
                                AddColorToCell(row - b, column + b, randomColors[i]);
                            }
                            if (column - b >= 0)
                            {
                                AddColorToCell(row - b, column - b, randomColors[i]);
                            }
                        }
                    }
                    break;
                case "Q":
                    for (int q = 0; q < 6; q++)
                    {
                        AddColorToCell(row, q, randomColors[i]);
                        AddColorToCell(q, column, randomColors[i]);
                        if (row + q < 6)
                        {
                            if (column + q < 6)
                            {
                                AddColorToCell(row + q, column + q, randomColors[i]);
                            }
                            if (column - q >= 0)
                            {
                                AddColorToCell(row + q, column - q, randomColors[i]);
                            }
                        }
                        if (row - q >= 0)
                        {
                            if (column + q < 6)
                            {
                                AddColorToCell(row - q, column + q, randomColors[i]);
                            }
                            if (column - q >= 0)
                            {
                                AddColorToCell(row - q, column - q, randomColors[i]);
                            }
                        }
                    }
                    break;
                case "N":
                    if (row - 2 >= 0)
                    {
                        if (column - 1 >= 0)
                        {
                            AddColorToCell(row - 2, column - 1, randomColors[i]);
                        }
                        if (column + 1 < 6)
                        {
                            AddColorToCell(row - 2, column + 1, randomColors[i]);
                        }
                    }
                    if (row + 2 < 6)
                    {
                        if (column - 1 >= 0)
                        {
                            AddColorToCell(row + 2, column - 1, randomColors[i]);
                        }
                        if (column + 1 < 6)
                        {
                            AddColorToCell(row + 2, column + 1, randomColors[i]);
                        }
                    }
                    if (column - 2 >= 0)
                    {
                        if (row - 1 >= 0)
                        {
                            AddColorToCell(row - 1, column - 2, randomColors[i]);
                        }
                        if (row + 1 < 6)
                        {
                            AddColorToCell(row + 1, column - 2, randomColors[i]);
                        }
                    }
                    if (column + 2 < 6)
                    {
                        if (row - 1 >= 0)
                        {
                            AddColorToCell(row - 1, column + 2, randomColors[i]);
                        }
                        if (row + 1 < 6)
                        {
                            AddColorToCell(row + 1, column + 2, randomColors[i]);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void AddColorToCell(int row, int column, string color)
    {
        RedValues[row] = RedValues[row].Substring(0, column) + ((RedValues[row][column] + color[0]) % 2).ToString() + RedValues[row].Substring(column + 1);
        GreenValues[row] = GreenValues[row].Substring(0, column) + ((GreenValues[row][column] + color[1]) % 2).ToString() + GreenValues[row].Substring(column + 1);
        BlueValues[row] = BlueValues[row].Substring(0, column) + ((BlueValues[row][column] + color[2]) % 2).ToString() + BlueValues[row].Substring(column + 1);
    }

    void SetBoardColors()
    {
        for (int i = 0; i < 36; i++)
        {
            setRow = i / 6;
            setColumn = i % 6;
            setColorIndex = binaryColors.IndexOf(RedValues[setRow][setColumn].ToString() + GreenValues[setRow][setColumn].ToString() + BlueValues[setRow][setColumn].ToString());
            GridButtonRenderers[i].material.color = colors[setColorIndex];
            ColorblindTexts[i].text = shortColorNames[setColorIndex];
        }
    }

    void Submit()
    {

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
