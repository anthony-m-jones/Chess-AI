using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Chess;
using Chess.Game;

public class MLPlayerAgent : Agent
{
    int wins = 0;
    public float timePerMove = 0;
    public bool logDebugs = false;
    public bool logLegalMoves = false;
    public bool logChosenMove = false;
    public enum PlayerColor { White, Black };
    public PlayerColor myPlayerColor = PlayerColor.White;
    bool canEndGame = true;
    EnvironmentParameters defaultParameters;
    BufferSensorComponent playerColorBufferSensorComponent;
    BufferSensorComponent whiteKingBufferSensorComponent;
    BufferSensorComponent blackKingBufferSensorComponent;
    BufferSensorComponent whiteQueenBufferSensorComponent;
    BufferSensorComponent blackQueenBufferSensorComponent;
    BufferSensorComponent whiteRookBufferSensorComponent;
    BufferSensorComponent blackRookBufferSensorComponent;
    BufferSensorComponent whiteBishopBufferSensorComponent;
    BufferSensorComponent blackBishopBufferSensorComponent;
    BufferSensorComponent whiteKnightBufferSensorComponent;
    BufferSensorComponent blackKnightBufferSensorComponent;
    BufferSensorComponent whitePawnBufferSensorComponent;
    BufferSensorComponent blackPawnBufferSensorComponent;
    BufferSensorComponent stateBufferSensorComponent;
    // BufferSensorComponent legalMovesBufferSensorComponent;
    Board board;
    MLPlayer mlPlayer;
    Move move;
    List<Move> moves;
    GameManager gm;
    Evaluation evaluation;
    bool choseMove;
    MoveGenerator moveGenerator;
    float score;
    float highestDelta;
    float lowestDelta;
    float currentScore;
    float delta;
    float inCheck;

    public override void Initialize()
    {
        score = 0;
        highestDelta = 0;
        lowestDelta = 0;
        moveGenerator = new MoveGenerator();
        evaluation = new Evaluation();
        choseMove = false;
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        base.Initialize();
        // this.col
        defaultParameters = Academy.Instance.EnvironmentParameters;
        BufferSensorComponent[] bufferSensorComponents = this.GetComponents<BufferSensorComponent>();
        foreach (BufferSensorComponent bufferSensorComponent in bufferSensorComponents)
        {
            switch (bufferSensorComponent.SensorName)
            {
                case "PlayerColor":
                    playerColorBufferSensorComponent = bufferSensorComponent;
                    break;
                case "WhiteKing":
                    whiteKingBufferSensorComponent = bufferSensorComponent;
                    break;
                case "BlackKing":
                    blackKingBufferSensorComponent = bufferSensorComponent;
                    break;
                case "WhiteQueen":
                    whiteQueenBufferSensorComponent = bufferSensorComponent;
                    break;
                case "BlackQueen":
                    blackQueenBufferSensorComponent = bufferSensorComponent;
                    break;
                case "WhiteRook":
                    whiteRookBufferSensorComponent = bufferSensorComponent;
                    break;
                case "BlackRook":
                    blackRookBufferSensorComponent = bufferSensorComponent;
                    break;
                case "WhiteKnight":
                    whiteKnightBufferSensorComponent = bufferSensorComponent;
                    break;
                case "BlackKnight":
                    blackKnightBufferSensorComponent = bufferSensorComponent;
                    break;
                case "WhiteBishop":
                    whiteBishopBufferSensorComponent = bufferSensorComponent;
                    break;
                case "BlackBishop":
                    blackBishopBufferSensorComponent = bufferSensorComponent;
                    break;
                case "WhitePawn":
                    whitePawnBufferSensorComponent = bufferSensorComponent;
                    break;
                case "BlackPawn":
                    blackPawnBufferSensorComponent = bufferSensorComponent;
                    break;
                case "State":
                    stateBufferSensorComponent = bufferSensorComponent;
                    break;
                // case "LegalMoves":
                //     legalMovesBufferSensorComponent = bufferSensorComponent;
                //     break;
                default:
                    Debug.Log($"Shits broke, {bufferSensorComponent.SensorName}");
                    break;
            }
        };
        board = gm.board;
        // Debug.Log($"board {board}");
        if (myPlayerColor == PlayerColor.White)
        {
            if (gm.whitePlayerType == GameManager.PlayerType.ML)
            {
                mlPlayer = gm.whitePlayer as MLPlayer;
            } 
        }
        else if (myPlayerColor == PlayerColor.Black) {
            if (gm.blackPlayerType == GameManager.PlayerType.ML) {
                mlPlayer = gm.whitePlayer as MLPlayer;
            }
        }

    }

    void AddBufferObservation(int pieceType, int playerColorIndex, BufferSensorComponent sensorComponent)
    {
        if (board == null)
        {
            board = gm.board;
        }
        int[] occupiedSquares = board.GetPieceList(pieceType, playerColorIndex).occupiedSquares;
        float[] observation = new float[64];
        foreach (var item in occupiedSquares)
        {
            observation[item] = 1f;
        }
        sensorComponent.AppendObservation(observation);
        // Debug.Log($"Sensor Component Name: {sensorComponent.name}");
        // for (int i = 0; i < observation.Length; i++) {
        //     Debug.Log($"observation {i}: {observation[i]}");
        // }
            
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        if (this.logDebugs) Debug.Log("Collecting observervations");
        if (board == null)
        {
            board = gm.board;
        }
        if ((myPlayerColor == PlayerColor.White && board.WhiteToMove) || (myPlayerColor == PlayerColor.Black && !board.WhiteToMove))
        {
            AddBufferObservation(Piece.King, Board.WhiteIndex, whiteKingBufferSensorComponent);
            AddBufferObservation(Piece.King, Board.BlackIndex, blackKingBufferSensorComponent);
            AddBufferObservation(Piece.Bishop, Board.BlackIndex, blackBishopBufferSensorComponent);
            AddBufferObservation(Piece.Bishop, Board.WhiteIndex, whiteBishopBufferSensorComponent);
            AddBufferObservation(Piece.Knight, Board.BlackIndex, blackKnightBufferSensorComponent);
            AddBufferObservation(Piece.Knight, Board.WhiteIndex, whiteKnightBufferSensorComponent);
            AddBufferObservation(Piece.Pawn, Board.BlackIndex, blackPawnBufferSensorComponent);
            AddBufferObservation(Piece.Pawn, Board.WhiteIndex, whitePawnBufferSensorComponent);
            AddBufferObservation(Piece.Queen, Board.BlackIndex, blackQueenBufferSensorComponent);
            AddBufferObservation(Piece.Queen, Board.WhiteIndex, whiteQueenBufferSensorComponent);
            AddBufferObservation(Piece.Rook, Board.BlackIndex, blackRookBufferSensorComponent);
            AddBufferObservation(Piece.Rook, Board.WhiteIndex, whiteRookBufferSensorComponent);
            
            inCheck = moveGenerator.InCheck() ? 1f : 0f;
            stateBufferSensorComponent.AppendObservation(new float[1] { inCheck });

            moves = moveGenerator.GenerateMoves(board);
            playerColorBufferSensorComponent.AppendObservation(new float[1] { myPlayerColor == PlayerColor.White ? 1f : 0f });
        }
    }
    void Update()
    {
        if (board == null)
        {
            board = gm.board;
        }
        if (gm.gameResult == GameManager.Result.Playing)
        {
            canEndGame = true;
            if (
                (mlPlayer == null)
                || ((myPlayerColor == PlayerColor.White) && (mlPlayer != gm.whitePlayer))
                || ((myPlayerColor == PlayerColor.Black) && (mlPlayer != gm.blackPlayer))
            )
            {
                if ((myPlayerColor == PlayerColor.White) && (gm.whitePlayerType == GameManager.PlayerType.ML)) mlPlayer = (MLPlayer)gm.whitePlayer;
                else if ((myPlayerColor == PlayerColor.Black) && (gm.blackPlayerType == GameManager.PlayerType.ML)) mlPlayer = (MLPlayer)gm.blackPlayer;
            }
            if ((myPlayerColor == PlayerColor.White && board.WhiteToMove) || (myPlayerColor == PlayerColor.Black && !board.WhiteToMove) && !choseMove)
            {
                if(mlPlayer == null) return;
                if (this.logDebugs) Debug.Log("requesting decision");
                RequestDecision();
            }
        }
        else
        {
            if(canEndGame) {
                canEndGame = false;
                mlPlayer = null;
                
                // else if (gm.gameResult == GameManager.Result.WhiteIsMated) {
                //     Debug.Log("We lost, -1");
                //     SetReward(-1);
                // }
                choseMove = false;
                EndEpisode();
                mlPlayer = null;
                score = 0;
                Debug.Log("Game is over, ending episode");
                // gm.NewMLAgentVersusAIGame();
            }
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (this.logDebugs) Debug.Log("Action Received");
        base.OnActionReceived(actions);
        // Debug.Log($"Discrete Actions Length: {actions.DiscreteActions.Length}");
        // Debug.Log($"Continuous Actions Length: {actions.ContinuousActions.Length}");
        // Debug.Log($"Discrete Actions [0]: {actions.DiscreteActions[0]}");

        // Debug.Log($"discreteActions: {actions.DiscreteActions[0]}, {actions.DiscreteActions[1]}");

        // float reward = 0f;
        // foreach (Move m in moves) {
        //     if (m.StartSquare == actions.DiscreteActions[0]) {
        //         reward += 0.4f;
        //         break;
        //     }
        // }
        // foreach (Move m in moves) {
        //     if (m.TargetSquare == actions.DiscreteActions[1]) {
        //         reward += 0.4f;
        //         break;
        //     }
        // }

        if (logDebugs) Debug.Log($"Chosen move: {actions.DiscreteActions[0]}, {actions.DiscreteActions[1]}");

        move = moves.Find((x) =>
        {
            return x.StartSquare == actions.DiscreteActions[0] && x.TargetSquare == actions.DiscreteActions[1];
        });
        if (logChosenMove) Debug.Log($"ChosenMove IsPromotion:{move.IsPromotion } PromotionPieceType:{move.PromotionPieceType} Name{move.Name} IsInvalid{move.IsInvalid} MoveFlag{move.MoveFlag} StartSquare{move.StartSquare} TargetSquare{move.TargetSquare} Value{move.Value}");
        // Debug.Log($"move: {move.StartSquare}, {move.TargetSquare}");
        if (move.StartSquare == 0 && move.TargetSquare == 0)
        {
            
            if (logDebugs) Debug.Log($"reward: 0");
            SetReward(-0.0001f);

            return;
        }

        choseMove = true;
        // if (logDebugs) Debug.Log($"reward: {10}");
        // AddReward(10);

        choseMove = false;
        // Debug.Log(move.Name);
        mlPlayer.publicChoseMove(move);

        currentScore = evaluation.Evaluate(board);
        delta = score - currentScore;
        // Debug.Log($"delta, {delta}");
        if (delta > highestDelta)
        {
            highestDelta = delta;
            Debug.Log($"New highest delta! {highestDelta}");
        }
        if (delta < lowestDelta)
        {
            lowestDelta = delta;
            Debug.Log($"New lowest delta! {lowestDelta}");
        }

        // Normalize reward to between 0 and 0.25 assuming delta can only be between -1500 and +1500
        float reward = (delta + 1500f) / 12000f;
        if (reward > 1 || reward <= 0) Debug.Log($"reward: {reward}");
        score = currentScore;
        SetReward(reward);

        if ((myPlayerColor == PlayerColor.White && gm.gameResult == GameManager.Result.BlackIsMated) || (myPlayerColor == PlayerColor.Black && gm.gameResult == GameManager.Result.WhiteIsMated))
        {
            wins++;
            Debug.Log($"{myPlayerColor} won! {wins} wins");
            SetReward(1);
        } else {
            // SetReward(0f);
            SetReward(reward);
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
    }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }
}
