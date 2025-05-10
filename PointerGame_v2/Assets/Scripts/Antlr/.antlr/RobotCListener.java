// Generated from /Users/aidanfox/Documents/Unity/Pointer-Game-v2/Assets/Scripts/Antlr/RobotC.g4 by ANTLR 4.13.1
import org.antlr.v4.runtime.tree.ParseTreeListener;

/**
 * This interface defines a complete listener for a parse tree produced by
 * {@link RobotCParser}.
 */
public interface RobotCListener extends ParseTreeListener {
	/**
	 * Enter a parse tree produced by {@link RobotCParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStatement(RobotCParser.StatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link RobotCParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStatement(RobotCParser.StatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link RobotCParser#variableExpr}.
	 * @param ctx the parse tree
	 */
	void enterVariableExpr(RobotCParser.VariableExprContext ctx);
	/**
	 * Exit a parse tree produced by {@link RobotCParser#variableExpr}.
	 * @param ctx the parse tree
	 */
	void exitVariableExpr(RobotCParser.VariableExprContext ctx);
	/**
	 * Enter a parse tree produced by {@link RobotCParser#assignmentStmt}.
	 * @param ctx the parse tree
	 */
	void enterAssignmentStmt(RobotCParser.AssignmentStmtContext ctx);
	/**
	 * Exit a parse tree produced by {@link RobotCParser#assignmentStmt}.
	 * @param ctx the parse tree
	 */
	void exitAssignmentStmt(RobotCParser.AssignmentStmtContext ctx);
}