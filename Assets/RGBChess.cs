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

    public KMSelectable ColorSwitcher;
    public MeshRenderer ColorSwitcherRenderer;
    public TextMesh ColorSwitcherColorblindText;
    public List<KMSelectable> PieceButtons;
    public List<MeshRenderer> PieceTextures;
    public List<Material> PieceMaterials;
    public Material DefaultPieceMaterial;
    public List<MeshRenderer> GridButtonRenderers;
    public List<KMSelectable> GridButtons;
    public List<TextMesh> GridColorblindTexts;
    public List<GameObject> GridPieces;
    public List<MeshRenderer> GridPieceRenderers;
    public List<TextMesh> GridPieceColorblindTexts;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    string randomPosition = "";
    string randomColor = "";
    string selectedPiece = "";
    string logGeneration = "[RGB Chess {0}] The generated solution is -";
    string logSubmission = "[RGB Chess {0}] Submitted solution is -";
    List<string> randomPositions = new List<string> { };
    List<string> randomColors = new List<string> { };
    List<string> randomPieces = new List<string> { };
    List<string> submissionPositions = new List<string> { };
    List<string> submissionColors = new List<string> { };
    List<string> submissionPieces = new List<string> { };
    int genPieceAmount = 3;
    int currentColorIndex = 7;
    int setColorIndex;
    int setRow;
    int setColumn;
    int placedPieces;
    bool solveFlag;

    string pieces = "KQRBN";
    List<string> pieceNames = new List<string> { "King", "Queen", "Rook", "Bishop", "Knight" };
    List<Color> colors = new List<Color> { new Color32(50, 50, 50, 255), Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow, Color.white };
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
    List<string> SubmissionRedValues = new List<string> { };
    List<string> SubmissionGreenValues = new List<string> { };
    List<string> SubmissionBlueValues = new List<string> { };

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        ColorSwitcher.OnInteract += delegate () { ColorSwitch(); return false; };
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
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ColorSwitcher.transform);

        currentColorIndex = (currentColorIndex + 1) % 8;

        for (int i = 0; i < 5; i++)
        {
            PieceTextures[i].material.color = colors[currentColorIndex];
        }
        ColorSwitcherRenderer.material.color = colors[currentColorIndex];
        ColorSwitcherColorblindText.text = shortColorNames[currentColorIndex];

        Debug.LogFormat("[RGB Chess #{0}] The color switcher was press, switching colors, current color is {1}.", ModuleId, colorNames[currentColorIndex]);

        if (selectedPiece != "")
        {
            selectedPiece = shortColorNames[currentColorIndex] + selectedPiece[1];
            Debug.LogFormat("[RGB Chess #{0}] Currently selected piece is {1} {2}.", ModuleId, colorNames[currentColorIndex], pieceNames[pieces.IndexOf(selectedPiece[1].ToString())]);
        }   
    }
    
    void PiecePressed(KMSelectable piece)
    {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, piece.transform);
        for (int i = 0; i < PieceButtons.Count; i++)
        {
            if (piece == PieceButtons[i])
            {
                selectedPiece = shortColorNames[currentColorIndex] + pieces[i];
                Debug.LogFormat("[RGB Chess #{0}] The {1} piece was pressed, selecting the {1}.", ModuleId, pieceNames[i]);
                Debug.LogFormat("[RGB Chess #{0}] Currently selected piece is a {1} {2}.", ModuleId, colorNames[currentColorIndex], pieceNames[i]);
            }
        }
    }

    void GridButtonPressed(KMSelectable cell)
    {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, cell.transform);
        cell.AddInteractionPunch();
        for (int i = 0; i < GridButtons.Count; i++)
        {
            if (cell == GridButtons[i])
            {
                if (selectedPiece != "")
                {
                    if (!GridPieces[i].activeSelf)
                    {
                        Debug.LogFormat("[RGB Chess #{0}] The {1} cell was pressed, and there isn't already a piece on it, placing the currently selected piece, which is a {2} {3}.", ModuleId, "ABCDEF"[i / 6].ToString() + (i % 6 + 1).ToString(),
                            colorNames[shortColorNames.IndexOf(selectedPiece[0].ToString())], pieceNames[pieces.IndexOf(selectedPiece[1].ToString())]);

                        GridPieces[i].SetActive(true);
                        GridPieceRenderers[i].material = PieceMaterials[pieces.IndexOf(selectedPiece[1].ToString())];
                        GridPieceRenderers[i].material.color = colors[shortColorNames.IndexOf(selectedPiece[0].ToString())];
                        if (Colorblind.ColorblindModeActive)
                        {
                            GridColorblindTexts[i].text = "";
                            GridPieceColorblindTexts[i].text = selectedPiece[0].ToString() + selectedPiece[1].ToString();
                        }
                        else
                        {
                            GridPieceColorblindTexts[i].text = "";
                        }
                        placedPieces++;
                    }
                    else
                    {
                        Debug.LogFormat("[RGB Chess #{0}] The {1} cell was pressed, but there is already a piece on it, removing the piece placed on {1}.", ModuleId, "ABCDEF"[i / 6].ToString() + (i % 6 + 1).ToString());
                        GridPieceRenderers[i].material = DefaultPieceMaterial;
                        GridPieces[i].SetActive(false);
                        placedPieces--;
                        if (Colorblind.ColorblindModeActive)
                        {
                            setRow = i / 6;
                            setColumn = i % 6;
                            setColorIndex = binaryColors.IndexOf(RedValues[setRow][setColumn].ToString() + GreenValues[setRow][setColumn].ToString() + BlueValues[setRow][setColumn].ToString());
                            GridButtonRenderers[i].material.color = colors[setColorIndex];
                            GridColorblindTexts[i].text = shortColorNames[setColorIndex];
                        }
                    }
                }
                else
                {
                    Debug.LogFormat("[RGB Chess #{0}] The {1} cell was pressed, but a piece is not selected, doing nothing.", ModuleId, "ABCDEF"[i / 6].ToString() + (i % 6 + 1).ToString());
                }
                if (placedPieces == genPieceAmount)
                {
                    Debug.LogFormat("[RGB Chess #{0}] 6 Pieces were placed, checking submission.", ModuleId);
                    SubmissionCheck();
                }
            }
        }
    }

    void Start()
    {
        GenerateBoard();
        ColorSwitcherColorblindText.gameObject.SetActive(Colorblind.ColorblindModeActive);
        for (int c = 0; c < 36; c++)
        {
            GridColorblindTexts[c].gameObject.SetActive(Colorblind.ColorblindModeActive);
        }
    }

    void GenerateBoard()
    {
        for (int i = 0; i < genPieceAmount; i++)
        {
            randomPosition = Rnd.Range(0, 6).ToString() + Rnd.Range(0, 6).ToString();
            while (randomPositions.Contains(randomPosition))
            {
                randomPosition = Rnd.Range(0, 6).ToString() + Rnd.Range(0, 6).ToString();
            }
            randomPositions.Add(randomPosition);

            randomColor = Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString();
            while (randomColor == "000")
            {
                randomColor = Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString() + Rnd.Range(0, 2).ToString();
            }
            randomColors.Add(randomColor);

            randomPieces.Add(pieces[Rnd.Range(0, 5)].ToString());
        }

        LogFinal(logGeneration, randomPieces, randomColors, randomPositions);

        CalculateBoardColors(RedValues, GreenValues, BlueValues, randomPositions, randomColors, randomPieces);
        SetBoardColors();
    }

    string LogCoordinates(int index, List<string> positions)
    {
        return "ABCDEF"[Int32.Parse(positions[index][1].ToString())].ToString() + (Int32.Parse(positions[index][0].ToString()) + 1).ToString();
    }

    string LogColors(int index, List<string> colors)
    {
        return colorNames[binaryColors.IndexOf(colors[index])];
    }

    string LogPieces(int index, List<string> piecesList)
    {
        return pieceNames[pieces.IndexOf(piecesList[index])];
    }

    void CalculateBoardColors(List<string> redGrid, List<string> greenGrid, List<string> blueGrid, List<string> positions, List<string> colors, List<string> pieces)
    {
        for (int i = 0; i < genPieceAmount; i++)
        {
            int row = Int32.Parse(positions[i][0].ToString());
            int column = Int32.Parse(positions[i][1].ToString());
            AddColorToCell(row, column, colors[i], redGrid, greenGrid, blueGrid);
            switch (pieces[i])
            {
                case "K":
                    if (row + 1 < 6)
                    {
                        AddColorToCell(row + 1, column, colors[i], redGrid, greenGrid, blueGrid);
                        if (column + 1 < 6)
                        {
                            AddColorToCell(row + 1, column + 1, colors[i], redGrid, greenGrid, blueGrid);
                        }
                        if (column - 1 >= 0)
                        {
                            AddColorToCell(row + 1, column - 1, colors[i], redGrid, greenGrid, blueGrid);
                        }
                    }
                    if (row - 1 >= 0)
                    {
                        AddColorToCell(row - 1, column, colors[i], redGrid, greenGrid, blueGrid);
                        if (column + 1 < 6)
                        {
                            AddColorToCell(row - 1, column + 1, colors[i], redGrid, greenGrid, blueGrid);
                        }
                        if (column - 1 >= 0)
                        {
                            AddColorToCell(row - 1, column - 1, colors[i], redGrid, greenGrid, blueGrid);
                        }
                    }
                    if (column + 1 < 6)
                    {
                        AddColorToCell(row, column + 1, colors[i], redGrid, greenGrid, blueGrid);
                    }
                    if (column - 1 >= 0)
                    {
                        AddColorToCell(row, column - 1, colors[i], redGrid, greenGrid, blueGrid);
                    }
                    break;
                case "R":
                    for (int r = 0; r < 6; r++)
                    {
                        AddColorToCell(row, r, colors[i], redGrid, greenGrid, blueGrid);
                        AddColorToCell(r, column, colors[i], redGrid, greenGrid, blueGrid);
                    }
                    break;
                case "B":
                    for (int b = 0; b < 6; b++)
                    {
                        if (row + b < 6)
                        {
                            if (column + b < 6)
                            {
                                AddColorToCell(row + b, column + b, colors[i], redGrid, greenGrid, blueGrid);
                            }
                            if (column - b >= 0)
                            {
                                AddColorToCell(row + b, column - b, colors[i], redGrid, greenGrid, blueGrid);
                            }
                        }
                        if (row - b >= 0)
                        {
                            if (column + b < 6)
                            {
                                AddColorToCell(row - b, column + b, colors[i], redGrid, greenGrid, blueGrid);
                            }
                            if (column - b >= 0)
                            {
                                AddColorToCell(row - b, column - b, colors[i], redGrid, greenGrid, blueGrid);
                            }
                        }
                    }
                    break;
                case "Q":
                    for (int q = 0; q < 6; q++)
                    {
                        AddColorToCell(row, q, colors[i], redGrid, greenGrid, blueGrid);
                        AddColorToCell(q, column, colors[i], redGrid, greenGrid, blueGrid);
                        if (row + q < 6)
                        {
                            if (column + q < 6)
                            {
                                AddColorToCell(row + q, column + q, colors[i], redGrid, greenGrid, blueGrid);
                            }
                            if (column - q >= 0)
                            {
                                AddColorToCell(row + q, column - q, colors[i], redGrid, greenGrid, blueGrid);
                            }
                        }
                        if (row - q >= 0)
                        {
                            if (column + q < 6)
                            {
                                AddColorToCell(row - q, column + q, colors[i], redGrid, greenGrid, blueGrid);
                            }
                            if (column - q >= 0)
                            {
                                AddColorToCell(row - q, column - q, colors[i], redGrid, greenGrid, blueGrid);
                            }
                        }
                    }
                    break;
                case "N":
                    if (row - 2 >= 0)
                    {
                        if (column - 1 >= 0)
                        {
                            AddColorToCell(row - 2, column - 1, colors[i], redGrid, greenGrid, blueGrid);
                        }
                        if (column + 1 < 6)
                        {
                            AddColorToCell(row - 2, column + 1, colors[i], redGrid, greenGrid, blueGrid);
                        }
                    }
                    if (row + 2 < 6)
                    {
                        if (column - 1 >= 0)
                        {
                            AddColorToCell(row + 2, column - 1, colors[i], redGrid, greenGrid, blueGrid);
                        }
                        if (column + 1 < 6)
                        {
                            AddColorToCell(row + 2, column + 1, colors[i], redGrid, greenGrid, blueGrid);
                        }
                    }
                    if (column - 2 >= 0)
                    {
                        if (row - 1 >= 0)
                        {
                            AddColorToCell(row - 1, column - 2, colors[i], redGrid, greenGrid, blueGrid);
                        }
                        if (row + 1 < 6)
                        {
                            AddColorToCell(row + 1, column - 2, colors[i], redGrid, greenGrid, blueGrid);
                        }
                    }
                    if (column + 2 < 6)
                    {
                        if (row - 1 >= 0)
                        {
                            AddColorToCell(row - 1, column + 2, colors[i], redGrid, greenGrid, blueGrid);
                        }
                        if (row + 1 < 6)
                        {
                            AddColorToCell(row + 1, column + 2, colors[i], redGrid, greenGrid, blueGrid);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void AddColorToCell(int row, int column, string color, List<string> redGrid, List<string> greenGrid, List<string> blueGrid)
    {
        redGrid[row] = redGrid[row].Substring(0, column) + ((redGrid[row][column] + color[0]) % 2).ToString() + redGrid[row].Substring(column + 1);
        greenGrid[row] = greenGrid[row].Substring(0, column) + ((greenGrid[row][column] + color[1]) % 2).ToString() + greenGrid[row].Substring(column + 1);
        blueGrid[row] = blueGrid[row].Substring(0, column) + ((blueGrid[row][column] + color[2]) % 2).ToString() + blueGrid[row].Substring(column + 1);
    }

    void SetBoardColors()
    {
        for (int i = 0; i < 36; i++)
        {
            setRow = i / 6;
            setColumn = i % 6;
            setColorIndex = binaryColors.IndexOf(RedValues[setRow][setColumn].ToString() + GreenValues[setRow][setColumn].ToString() + BlueValues[setRow][setColumn].ToString());
            GridButtonRenderers[i].material.color = colors[setColorIndex];
            GridColorblindTexts[i].text = shortColorNames[setColorIndex];
        }
    }

    void SubmissionCheck()
    {
        SubmissionRedValues = new List<string>
        {
        "000000",
        "000000",
        "000000",
        "000000",
        "000000",
        "000000"
        };
        SubmissionGreenValues = new List<string>
        {
        "000000",
        "000000",
        "000000",
        "000000",
        "000000",
        "000000"
        };
        SubmissionBlueValues = new List<string>
        {
        "000000",
        "000000",
        "000000",
        "000000",
        "000000",
        "000000"
        };
        submissionPositions = new List<string> { };
        submissionColors = new List<string> { };
        submissionPieces = new List<string> { };
        solveFlag = true;
        for (int i = 0; i < 36; i++)
        {
            if (GridPieceRenderers[i].material.ToString() != "Default-Material (Instance) (UnityEngine.Material)")
            {
                submissionPositions.Add(((int)(i / 6)).ToString() + (i % 6).ToString());
                switch (GridPieceRenderers[i].material.ToString())
                {
                    case "King (Instance) (UnityEngine.Material)":
                        submissionPieces.Add("K");
                        break;
                    case "Queen (Instance) (UnityEngine.Material)":
                        submissionPieces.Add("Q");
                        break;
                    case "Rook (Instance) (UnityEngine.Material)":
                        submissionPieces.Add("R");
                        break;
                    case "Bishop (Instance) (UnityEngine.Material)":
                        submissionPieces.Add("B");
                        break;
                    case "Knight (Instance) (UnityEngine.Material)":
                        submissionPieces.Add("N");
                        break;
                }
                switch (GridPieceRenderers[i].material.color.ToString())
                {
                    case "RGBA(0.196, 0.196, 0.196, 1.000)":
                        submissionColors.Add("000");
                        break;
                    case "RGBA(1.000, 0.000, 0.000, 1.000)":
                        submissionColors.Add("100");
                        break;
                    case "RGBA(0.000, 1.000, 0.000, 1.000)":
                        submissionColors.Add("010");
                        break;
                    case "RGBA(0.000, 0.000, 1.000, 1.000)":
                        submissionColors.Add("001");
                        break;
                    case "RGBA(0.000, 1.000, 1.000, 1.000)":
                        submissionColors.Add("011");
                        break;
                    case "RGBA(1.000, 0.000, 1.000, 1.000)":
                        submissionColors.Add("101");
                        break;
                    case "RGBA(1.000, 0.922, 0.016, 1.000)":
                        submissionColors.Add("110");
                        break;
                    case "RGBA(1.000, 1.000, 1.000, 1.000)":
                        submissionColors.Add("111");
                        break;
                }
            }
        }
        logSubmission = "[RGB Chess {0}] Submitted solution is -";
        LogFinal(logSubmission, submissionPieces, submissionColors, submissionPositions);
        CalculateBoardColors(SubmissionRedValues, SubmissionGreenValues, SubmissionBlueValues, submissionPositions, submissionColors, submissionPieces);

        for (int i = 0; i < 6; i++)
        {
            if (RedValues[i] != SubmissionRedValues[i] || GreenValues[i] != SubmissionGreenValues[i] || BlueValues[i] != SubmissionBlueValues[i])
            {
                solveFlag = false;
            }
        }
        if (solveFlag)
        {
            Debug.LogFormat("[RGB Chess {0}] Submitted solution generated the same colors as the initial ones, module solved.", ModuleId);
            GetComponent<KMBombModule>().HandlePass();
            ModuleSolved = true;
            for (int i = 0; i < 36; i++)
            {
                GridButtonRenderers[i].material.color = Color.green;
                GridColorblindTexts[i].text = "!";
                if (GridPieceRenderers[i].material.ToString() != "Default-Material (Instance) (UnityEngine.Material)")
                {
                    GridPieceRenderers[i].material.color = Color.green;
                    GridPieceColorblindTexts[i].text = "";
                }
            }
        }
        else
        {
            Debug.LogFormat("[RGB Chess {0}] Submitted solution did not generate the same colors as the initial ones, strike!", ModuleId);
            GetComponent<KMBombModule>().HandleStrike();
            StartCoroutine(ModuleStrike());
        }
    }

    IEnumerator ModuleStrike()
    {
        for (int i = 0; i < 36; i++)
        {
            GridButtonRenderers[i].material.color = Color.red;
            GridColorblindTexts[i].text = "X";
            if (GridPieceRenderers[i].material.ToString() != "Default-Material (Instance) (UnityEngine.Material)")
            {
                GridPieceRenderers[i].material.color = Color.red;
                GridPieceColorblindTexts[i].text = "";
            }
        }

        yield return new WaitForSeconds(2);

        placedPieces = 0;
        for (int i = 0; i < 36; i++)
        {
            GridPieces[i].SetActive(false);
            if (GridPieceRenderers[i].material.ToString() != "Default-Material (Instance) (UnityEngine.Material)")
            {
                GridPieceRenderers[i].material = DefaultPieceMaterial;
            }
        }

        SetBoardColors();
    }

    void LogFinal(string log, List<string> pieceList, List<string> colorList, List<string> positions)
    {
        for (int i = 0; i < genPieceAmount; i++)
        {
            log += " " + LogColors(i, colorList) + " " + LogPieces(i, pieceList) + " at " + LogCoordinates(i, positions);
            if (i < genPieceAmount - 2)
            {
                log += ",";
            }
            else if (i == genPieceAmount - 2)
            {
                log += " and";
            }
            else
            {
                log += ".";
            }
        }

        Debug.LogFormat(log, ModuleId);
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
