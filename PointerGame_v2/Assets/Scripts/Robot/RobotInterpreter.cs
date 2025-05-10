using UnityEngine;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;

[assembly: CLSCompliant(false)]
public class RobotInterpreter : RobotCBaseListener
{
    private readonly string deref = "*";
    // private readonly string addr = "&";
    private readonly string eof = ";";
    private readonly string nullword = "null";

    //checks obj exists
    private Node ValidateObj(string objName) => GameObject.Find(objName)?.GetComponent<Node>();

    private void PrintError(string errmsg) => LevelManager.instance.robot.PrintError(errmsg);

    //gets possible actions based on context
    private void GetPossibleAction(RobotCParser.AssignmentStmtContext context, Node n1, string n1Name, Node n2)
    {
        //possible starts
        bool transferRight = false;
        bool derefRight = false;
        bool laserconnection = false;

        //possible ends
        bool transferLeft = false;
        bool derefLeft = false;

        //parsing type of statement

        if (context.variableExpr().modifier != null)
        {
            if (context.variableExpr().modifier.Text.Equals(deref))
            {
                //deref on right
                // Debug.Log("Deref on right");
                derefRight = true;
            }
            else //if(context.variableExpr().modifier.Equals(addr))
            {
                //addr on right
                // Debug.Log("laser connection");
                laserconnection = true;
            }
        }
        else
        {
            //regular assignment
            transferRight = true;
        }

        if (context.deref != null)
        {
            //deref on left
            derefLeft = true;
        }
        else
        {
            //assignment
            transferLeft = true;
        }

        RobotActionType possibleAction = RobotActionType.None;

        //checking break connection logic breakConnection
        if (n1Name.Equals(nullword))
        {
            if (laserconnection) PrintError("Cannot take the address of null");
            // else if(transferLeft && )
            possibleAction = RobotActionType.BreakConnection;
        }

        //types of connection
        else if (laserconnection)
        {
            //point laser
            possibleAction = RobotActionType.PointLaser;
        }
        else if (transferRight)
        {
            //transfer to transfer
            if (transferLeft) possibleAction = RobotActionType.TransferToTransferConnection;
            //transfer to deref
            else if (derefLeft) possibleAction = RobotActionType.TransferToDerefConnection;
        }
        else if (derefRight)
        {
            //deref to transfer
            if (transferLeft) possibleAction = RobotActionType.DerefToTransferConnection;
            //deref to deref
            else if (derefLeft) possibleAction = RobotActionType.DerefToDerefConnection;
        }

        if (possibleAction != RobotActionType.None)
        {
            // Debug.Log($"n1: {n1.name} with {possibleAction} to n2: {n2.name}");

            if(possibleAction == RobotActionType.PointLaser){
                LevelManager.instance.robot.EnqueueAction(new RobotAction(n2.transform, possibleAction, n1));
            }
            else{
                LevelManager.instance.robot.EnqueueAction(new RobotAction(n1.transform, possibleAction, n2));
            }
        }
    }

    public override void EnterAssignmentStmt([NotNull] RobotCParser.AssignmentStmtContext context)
    {
        Node n2 = ValidateObj(context.VARNAME().GetText());
        string n1Name = context.variableExpr().VARNAME().GetText();
        Node n1 = ValidateObj(context.variableExpr().VARNAME().GetText());

        //checking for errors
        if (context.ChildCount != 3)
        {
            PrintError("Statement must contain 3 things: a node, an assignment, and another node. For example: node2 = node");
            return;
        }
        //context seems to remove ";" anyway so this check is irrelevant
        // if(!context.GetText().EndsWith(eof)){
        //     Debug.Log(context.GetText());
        //     PrintError("Statement must end with ';'");
        //     return;
        // }
        if (n1 == null)
        {
            PrintError($"{n1} is Invalid");
            return;
        }
        if (!n1Name.Equals(nullword) && n2 == null)
        {
            PrintError($"{n2} is Invalid");
            return;
        }

        GetPossibleAction(context, n1, n1Name, n2);
    }

    //interprets statements
    public void InterpretStatement(string stmt)
    {
        //parse statement
        ICharStream stream = CharStreams.fromString(stmt);
        ITokenSource lexer = new RobotCLexer(stream);
        ITokenStream tokens = new CommonTokenStream(lexer);
        RobotCParser parser = new RobotCParser(tokens);
        
        // parser.ErrorHandler = new BailErrorStrategy();

        IParseTree tree = parser.statement(); //use try-except/catch to print errors
        // Debug.Log(tree.GetText());

        //checking if tree ends with ';'
        if(!tree.GetText().EndsWith(eof)){
            PrintError("Statement must end with ';'");
            return;
        }

        // RobotInterpretor interpretor = new RobotInterpretor();
        ParseTreeWalker.Default.Walk(this, tree);
    }
}